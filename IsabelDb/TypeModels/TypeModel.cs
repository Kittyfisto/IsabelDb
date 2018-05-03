using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace IsabelDb.TypeModels
{
	/// <summary>
	///     A description of types, their serializable fields and properties as well as their base types
	/// </summary>
	/// <remarks>
	///     This model is used to create the protobuf type model in order to compile a serializer.
	/// </remarks>
	internal sealed class TypeModel
		: IEnumerable<Type>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string TypeTableName = "isabel_types";

		/// <summary>
		/// The followin list of types are built-in to protobuf: We cannot override their
		/// serialization behaviour (and and for example, define that string inherits from object).
		/// </summary>
		public static HashSet<Type> BuiltInProtobufTypes = new HashSet<Type>
		{
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

		private readonly Dictionary<Type, TypeDescription> _typeDescriptions;
		private readonly Dictionary<int, Type> _idToTypes;
		private readonly Dictionary<Type, int> _typesToId;
		private int _nextId;

		public IEnumerable<Type> Keys => _typeDescriptions.Keys;

		public IEnumerable<TypeDescription> Values => _typeDescriptions.Values;

		public int Count => _typeDescriptions.Count;

		public TypeDescription this[Type key] => _typeDescriptions[key];

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
		/// 
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="typeRegistry"></param>
		/// <returns></returns>
		public static TypeModel Read(SQLiteConnection connection, TypeRegistry typeRegistry)
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
						if (!reader.IsDBNull(2))
						{
							baseId = reader.GetInt32(2);
						}
						else
						{
							baseId = null;
						}
						model.TryAddType(id, typeName, baseId, typeRegistry);
					}
				}
			}

			return model;
		}

		public void Add(List<Type> allTypes)
		{
			foreach(var type in allTypes)
			{
				AddType(type);
			}
		}

		private void TryAddType(int id, string typeName, int? baseId, TypeRegistry typeRegistry)
		{
			try
			{
				_nextId = Math.Max(_nextId, id + 1);
				var type = typeRegistry.Resolve(typeName);
				if (type != null)
				{
					var baseType = baseId != null ? GetType(baseId.Value) : null;
					AddType(typeName, type, id, baseType, null);
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
				Log.ErrorFormat("Unable to resolve '{0}' to a .NET type! Values of this type will not be readable: {0}", typeName,
								e);
			}
		}

		public static TypeModel Create(IEnumerable<Type> supportedTypes)
		{
			var model = new TypeModel();
			foreach(var type in supportedTypes)
			{
				model.AddType(type);
			}
			return model;
		}

		public void Write(SQLiteConnection connection)
		{
			using (var transaction = connection.BeginTransaction())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT OR REPLACE INTO {0} (id, typename, baseId) VALUES (@id, @typename, @baseId)", TypeTableName);
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

		private void AddType(string typename, Type type, int typeId,
			Type baseType, IEnumerable<PropertyDescription> properties)
		{
			var baseTypeDescription = baseType != null ? _typeDescriptions[baseType] : null;
			var typeDescription = TypeDescription.Create(typename, baseTypeDescription, properties);
			typeDescription.Type = type;
			typeDescription.TypeId = typeId;

			ThrowIfBreakingChangesDetect(typeDescription);

			_typeDescriptions.Add(type, typeDescription);
			_typesToId.Add(type, typeId);
			_idToTypes.Add(typeId, type);
		}

		public TypeDescription AddType(Type type)
		{
			if (!_typeDescriptions.TryGetValue(type, out var typeDescription))
			{
				var baseTypeDescription = AddBaseTypeOf(type);

				var id = _nextId;
				++_nextId;
				typeDescription = TypeDescription.Create(type, baseTypeDescription);
				typeDescription.TypeId = id;
				_typeDescriptions.Add(type, typeDescription);
				_typesToId.Add(type, id);
				_idToTypes.Add(id, type);
			}
			return typeDescription;
		}

		private void ThrowIfBreakingChangesDetect(TypeDescription typeDescription)
		{
			var type = typeDescription.Type;
			var current = Create(new[] { type }).GetTypeDescription(type);
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
			if (BuiltInProtobufTypes.Contains(type))
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

			return serializable[0];
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

		public IEnumerator<Type> GetEnumerator()
		{
			return _typesToId.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}