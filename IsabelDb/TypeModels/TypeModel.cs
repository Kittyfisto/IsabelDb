using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;

namespace IsabelDb.TypeModels
{
	/// <summary>
	///     A description of types, their serializable fields and properties as well as their base types.
	/// </summary>
	/// <remarks>
	///     This model can be stored in the database and then be restored again, in order to determine if
	///     breaking changes were made to any type.
	/// </remarks>
	/// <remarks>
	///     This model contains enough information to build a <see cref="ProtoBuf.Meta.TypeModel" />.
	/// </remarks>
	internal sealed class TypeModel
		: IEnumerable<Type>
	{
		public const string TypeTableName = "isabel_types";
		public const string FieldTableName = "isabel_fields";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     The followin list of types are built-in to protobuf: We cannot override their
		///     serialization behaviour (and and for example, define that string inherits from object).
		/// </summary>
		public static readonly HashSet<Type> BuiltInProtobufTypes = new HashSet<Type>
		{
			typeof(bool),
			typeof(string),
			typeof(float),
			typeof(double),
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(byte[])
		};

		private readonly Dictionary<int, Type> _idToTypes;

		private readonly Dictionary<Type, TypeDescription> _typeDescriptions;
		private readonly Dictionary<Type, int> _typesToId;
		private int _nextId;

		public TypeModel()
		{
			_typeDescriptions = new Dictionary<Type, TypeDescription>();
			_idToTypes = new Dictionary<int, Type>();
			_typesToId = new Dictionary<Type, int>();
			_nextId = 1;
		}

		private TypeModel(Dictionary<Type, TypeDescription> types)
		{
			_typeDescriptions = types;
		}

		public IEnumerable<Type> Keys => _typeDescriptions.Keys;

		public IEnumerable<TypeDescription> Values => _typeDescriptions.Values;

		public int Count => _typeDescriptions.Count;

		public TypeDescription this[Type key] => _typeDescriptions[key];

		public IEnumerator<Type> GetEnumerator()
		{
			return _typesToId.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[Pure]
		public bool IsTypeRegistered(Type type)
		{
			return _typesToId.ContainsKey(type);
		}

		public string GetTypeName(Type type)
		{
			return _typeDescriptions[type].FullTypeName;
		}

		public int GetTypeId(Type type)
		{
			if (!_typesToId.TryGetValue(type, out var typeId))
				throw new ArgumentException(string.Format("The type '{0}' has not been registered!", type));
			return typeId;
		}

		public Type GetType(int typeId)
		{
			if (!_idToTypes.TryGetValue(typeId, out var type))
				return null;

			return type;
		}

		public TypeDescription GetTypeDescription(Type type)
		{
			return _typeDescriptions[type];
		}

		/// <summary>
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="typeResolver"></param>
		/// <returns></returns>
		public static TypeModel Read(SQLiteConnection connection, TypeResolver typeResolver)
		{
			var model = new TypeModel();
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT id, typename, baseId FROM {0}", TypeTableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var id = reader.GetInt32(i: 0);
						var typeName = reader.GetString(i: 1);
						int? baseId;
						if (!reader.IsDBNull(i: 2))
							baseId = reader.GetInt32(i: 2);
						else
							baseId = null;
						model.TryAddType(id, typeName, baseId, typeResolver);
					}
				}
			}

			return model;
		}

		public void Add(List<Type> allTypes)
		{
			foreach (var type in allTypes) AddType(type);
		}

		private void TryAddType(int id, string typeName, int? baseId, TypeResolver typeResolver)
		{
			try
			{
				_nextId = Math.Max(_nextId, id + 1);
				var type = typeResolver.Resolve(typeName);
				if (type != null)
				{
					var baseType = baseId != null ? GetType(baseId.Value) : null;
					AddType(typeName, type, id, baseType, fields: null);
				}
				else
				{
					Log.ErrorFormat("Unable to resolve '{0}' to a .NET type! Values of this type will not be readable", typeName);
				}
			}
			catch (BreakingChangeException)
			{
				throw;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to resolve '{0}' to a .NET type! Values of this type will not be readable: {1}", typeName,
				                e);
			}
		}

		public static TypeModel Create(IEnumerable<Type> supportedTypes)
		{
			var model = new TypeModel();
			foreach (var type in supportedTypes) model.AddType(type);
			return model;
		}

		public void Write(SQLiteConnection connection)
		{
			using (var transaction = connection.BeginTransaction())
			using (var command = connection.CreateCommand())
			{
				command.CommandText =
					string.Format("INSERT OR REPLACE INTO {0} (id, typename, baseId) VALUES (@id, @typename, @baseId)", TypeTableName);
				var idParameter = command.Parameters.Add("@id", DbType.Int32);
				var typename = command.Parameters.Add("@typename", DbType.String);
				var baseIdParameter = command.Parameters.Add("@baseId", DbType.Int32);

				foreach (var pair in _typeDescriptions)
				{
					var type = pair.Key;
					var typeDescription = pair.Value;
					var typeName = pair.Value.FullTypeName;
					var typeId = _typesToId[type];
					var baseTypeId = GetBaseTypeId(typeDescription);

					typename.Value = typeName;
					idParameter.Value = typeId;
					baseIdParameter.Value = baseTypeId;
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		public TypeDescription AddType(Type type)
		{
			if (!_typeDescriptions.TryGetValue(type, out var typeDescription))
			{
				var baseTypeDescription = AddBaseTypeOf(type);

				var fields = TypeDescription.FindSerializableFields(type);
				var properties = TypeDescription.FindSerializableProperties(type);
				var members = CreateMemberDescriptions(fields, properties);

				if (type.IsArray && type.GetArrayRank() == 1)
				{
					AddType(type.GetElementType());
				}

				var id = _nextId;
				++_nextId;
				typeDescription = TypeDescription.Create(type, id, baseTypeDescription, members);
				_typeDescriptions.Add(type, typeDescription);
				_typesToId.Add(type, id);
				_idToTypes.Add(id, type);
			}

			return typeDescription;
		}

		private IReadOnlyList<MemberDescription> CreateMemberDescriptions(IReadOnlyList<FieldInfo> fields,
		                                                                  IReadOnlyList<PropertyInfo> properties)
		{
			var members = new List<MemberDescription>(fields.Count + properties.Count);
			foreach (var field in fields)
			{
				var typeDescription = AddType(field.FieldType);
				var description = MemberDescription.Create(field, typeDescription);
				members.Add(description);
			}

			foreach (var property in properties)
			{
				var typeDescription = AddType(property.PropertyType);
				var description = MemberDescription.Create(property, typeDescription);
				members.Add(description);
			}

			return members;
		}

		private void AddType(string typename,
		                     Type type,
		                     int typeId,
		                     Type baseType,
		                     IEnumerable<MemberDescription> fields)
		{
			var baseTypeDescription = baseType != null ? _typeDescriptions[baseType] : null;
			var typeDescription = TypeDescription.Create(type,
			                                             typename,
			                                             typeId,
			                                             baseTypeDescription,
			                                             fields);

			ThrowIfBreakingChangesDetect(typeDescription);

			_typeDescriptions.Add(type, typeDescription);
			_typesToId.Add(type, typeId);
			_idToTypes.Add(typeId, type);
		}

		private void ThrowIfBreakingChangesDetect(TypeDescription typeDescription)
		{
			var type = typeDescription.Type;
			var current = Create(new[] {type}).GetTypeDescription(type);
			typeDescription.ThrowIfIncompatibleTo(current);
		}

		private int? GetBaseTypeId(TypeDescription typeDescription)
		{
			if (BuiltInProtobufTypes.Contains(typeDescription.Type))
				return null;

			var baseType = typeDescription.BaseType;
			if (baseType == null)
				return null;

			return baseType.TypeId;
		}

		private TypeDescription AddBaseTypeOf(Type type)
		{
			if (IsBuiltIn(type))
				return null;

			if (IsSerializableInterface(type))
				return AddType(typeof(object));

			var baseType = type.BaseType;
			if (baseType == null)
				return null;

			// Let's find out if this type implements a serializable interface!
			var allInterfaces = type.GetInterfaces();
			var baseTypeInterfaces = baseType.GetInterfaces();
			var interfaces = allInterfaces.Except(baseTypeInterfaces).ToList();
			var serializable = FindSerializableInterface(interfaces);
			if (serializable != null)
				return AddType(serializable);

			return AddType(baseType);
		}

		[Pure]
		public static bool IsBuiltIn(Type type)
		{
			if (type.IsArray)
				return true;
			if (BuiltInProtobufTypes.Contains(type))
				return true;

			return false;
		}

		public static bool IsWellKnown(Type type)
		{
			if (type == typeof(object))
				return true;
			if (type == typeof(IPAddress))
				return true;
			if (type == typeof(ValueType))
				return true;

			return IsBuiltIn(type);
		}

		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return IsabelDb.TableExists(connection, TypeTableName);
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "id INTEGER PRIMARY KEY NOT NULL," +
				                                    "typename TEXT NOT NULL," +
				                                    "baseId INTEGER" +
				                                    ")", TypeTableName);
				command.ExecuteNonQuery();
			}
		}

		private static Type FindSerializableInterface(List<Type> interfaces)
		{
			var serializable = interfaces.Where(IsSerializableInterface).ToList();
			if (serializable.Count == 0)
				return null;

			if (serializable.Count > 1)
				throw new NotImplementedException();

			return serializable[index: 0];
		}

		[Pure]
		private static bool IsSerializableInterface(Type type)
		{
			if (type == null)
				return false;

			if (!type.IsInterface)
				return false;

			var attributes = type.GetCustomAttributes(typeof(SerializableContractAttribute), inherit: false);
			if (attributes.Length != 1)
				return false;

			return true;
		}
	}
}