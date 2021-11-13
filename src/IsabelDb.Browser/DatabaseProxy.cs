using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using IsabelDb.TypeModels;
using log4net;

namespace IsabelDb.Browser
{
	public sealed class DatabaseProxy
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IDatabase _database;
		private readonly string _fileName;

		public DatabaseProxy(IDatabase database)
		{
			_fileName = "<In Memory>";
			_database = database;
		}

		public DatabaseProxy(string fileName)
		{
			_fileName = fileName;

			var connectionString = global::IsabelDb.Database.CreateConnectionString(fileName);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				global::IsabelDb.Database.EnsureTableSchema(connection);

				var wellKnownTypes = new List<Type>(ProtobufTypeModel.BuiltInTypes)
				{
					typeof(double[]) //<TODO: remove the need for this hack
				};
				var typeResolver = new TypeResolver(wellKnownTypes);
				var typeModel = TypeModel.Read(connection, typeResolver);
				var types = CompileCustomTypes(typeModel, typeResolver);
				_database = new IsabelDb(connection, fileName, types, true, isReadOnly: true);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception:\r\n{0}", e);
				connection.Dispose();
				throw;
			}
		}

		public string FileName => _fileName;

		private IReadOnlyList<Type> CompileCustomTypes(TypeModel typeModel, TypeResolver typeResolver)
		{
			var assemblyName = new AssemblyName("IsabelDb.Browser.GeneratedCode");
			var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var moduleName = assemblyName.Name + ".dll";
			var moduleBuilder = assembly.DefineDynamicModule(moduleName);

			var types = new List<Type>();
			foreach (var typeDescription in typeModel.TypeDescriptions)
			{
				if (typeDescription.ResolvedType == null)
				{
					var type = CreateCustomType(moduleBuilder, typeDescription, typeResolver);
					types.Add(type);
					typeResolver.Register(typeDescription.FullTypeName, type);
				}
			}

			return types;
		}

		private Type CreateCustomType(ModuleBuilder moduleBuilder,
		                              TypeDescription typeDescription,
		                              TypeResolver typeResolver)
		{
			if (typeDescription.Classification == TypeClassification.Enum)
			{
				return CreateEnum(moduleBuilder, typeDescription);
			}

			var baseType = typeDescription.BaseType.ResolvedType;
			if (baseType == null)
			{
				var name = typeDescription.BaseType.FullTypeName;
				baseType = typeResolver.Resolve(name);
			}

			return CreateCustomClass(moduleBuilder, typeDescription, baseType);
		}

		private static Type CreateEnum(ModuleBuilder moduleBuilder, TypeDescription typeDescription)
		{
			var typeBuilder = moduleBuilder.DefineEnum(typeDescription.FullTypeName,
			                                           TypeAttributes.Public,
			                                           typeDescription.UnderlyingEnumTypeDescription.ResolvedType);

			var attributeBuilder =
				new CustomAttributeBuilder(typeof(DataContractAttribute).GetConstructor(new Type[0]), new object[0]);
			typeBuilder.SetCustomAttribute(attributeBuilder);

			foreach (var fieldDescription in typeDescription.Fields)
			{
				var fieldBuilder = typeBuilder.DefineLiteral(fieldDescription.Name, fieldDescription.MemberId);
				fieldBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(EnumMemberAttribute).GetConstructor(new Type[0]), new object[0]));
			}

			return typeBuilder.CreateTypeInfo();
		}

		private static Type CreateCustomClass(ModuleBuilder moduleBuilder, TypeDescription typeDescription, Type baseType)
		{
			var typeBuilder = moduleBuilder.DefineType(typeDescription.FullTypeName,
			                                           TypeAttributes.Class | TypeAttributes.Public,
			                                           baseType);

			var attributeBuilder =
				new CustomAttributeBuilder(typeof(DataContractAttribute).GetConstructor(new Type[0]), new object[0]);
			typeBuilder.SetCustomAttribute(attributeBuilder);

			foreach (var fieldDescription in typeDescription.Fields)
			{
				var field = typeBuilder.DefineField(fieldDescription.Name,
				                                    fieldDescription.FieldTypeDescription.ResolvedType,
				                                    FieldAttributes.Public);

				var attribute = typeof(DataMemberAttribute);
				var attributeCtor = attribute.GetConstructor(new Type[0]);
				var order = attribute.GetProperty("Order");
				var builder = new CustomAttributeBuilder(attributeCtor,
				                                         new object[0],
				                                         new[] {order},
				                                         new object[] {fieldDescription.MemberId},
				                                         new FieldInfo[0],
				                                         new object[0]
				                                        );
				field.SetCustomAttribute(builder);
			}

			return typeBuilder.CreateType();
		}

		public IDatabase Database
		{
			get { return _database; }
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}