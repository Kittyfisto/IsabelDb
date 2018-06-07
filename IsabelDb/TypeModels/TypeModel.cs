using System;
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
	{
		private const string TypeTableName = "isabel_types";
		private const string FieldTableName = "isabel_fields";

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
			typeof(ulong),
			typeof(byte[]),
			typeof(DateTime),
			typeof(Guid)
		};

		private readonly Dictionary<int, TypeDescription> _typeDescriptionsById;
		private readonly Dictionary<int, Type> _idToTypes;
		private readonly Dictionary<Type, TypeDescription> _typeDescriptions;
		private readonly Dictionary<Type, int> _typesToId;

		private int _nextId;

		public TypeModel()
		{
			_typeDescriptionsById = new Dictionary<int, TypeDescription>();
			_typeDescriptions = new Dictionary<Type, TypeDescription>();
			_idToTypes = new Dictionary<int, Type>();
			_typesToId = new Dictionary<Type, int>();
			_nextId = 1;
		}

		public IEnumerable<Type> Types => _typeDescriptions.Keys;

		public IEnumerable<TypeDescription> TypeDescriptions => _typeDescriptionsById.Values;

		public TypeDescription this[Type key] => _typeDescriptions[key];

		[Pure]
		public bool IsTypeRegistered(Type type)
		{
			return _typesToId.ContainsKey(type);
		}

		[Pure]
		public string GetTypeName(Type type)
		{
			if (!_typeDescriptions.TryGetValue(type, out var typeDescription))
				throw new ArgumentException(string.Format("The type '{0}' has not been registered with this type model!", type));
			return typeDescription.FullTypeName;
		}

		[Pure]
		public int GetTypeId(Type type)
		{
			if (!_typesToId.TryGetValue(type, out var typeId))
				throw new ArgumentException(string.Format("The type '{0}' has not been registered with this type model!", type));
			return typeId;
		}

		[Pure]
		public Type TryGetType(int typeId)
		{
			if (!_idToTypes.TryGetValue(typeId, out var type))
				return null;

			return type;
		}

		[Pure]
		public TypeDescription GetTypeDescription(int typeId)
		{
			_typeDescriptionsById.TryGetValue(typeId, out var typeDescription);
			return typeDescription;
		}

		[Pure]
		public TypeDescription GetTypeDescription(Type type)
		{
			return _typeDescriptions[type];
		}

		[Pure]
		public TypeDescription TryGetTypeDescription(int typeId)
		{
			var type = TryGetType(typeId);
			if (type == null)
				return null;

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

			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT declaringTypeId, name, fieldId, fieldTypeId FROM {0}", FieldTableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var declaringTypeId = reader.GetInt32(0);
						var fieldName = reader.GetString(1);
						var memberId = reader.GetInt32(2);
						var fieldTypeId = reader.GetInt32(3);

						var declaringType = model.TryGetTypeDescription(declaringTypeId);
						if (declaringType != null)
						{
							var fieldType = model.TryGetTypeDescription(fieldTypeId);
							var memberInfo = FieldDescription.TryGetMemberInfo(declaringType.Type, fieldName);
							if (memberInfo != null)
							{
								var memberDescription = FieldDescription.Create(memberInfo,
								                                                fieldType,
								                                                memberId);
								declaringType.Add(memberDescription);
							}
						}
					}
				}
			}

			return model;
		}

		public TypeDescription Add(Type type)
		{
			if (!_typeDescriptions.TryGetValue(type, out var typeDescription))
			{
				var baseTypeDescription = AddBaseTypeOf(type);

				var fields = FieldDescription.FindSerializableFields(type);
				var properties = FieldDescription.FindSerializableProperties(type);
				var members = CreateFieldDescriptions(fields, properties, baseTypeDescription);

				if (type.IsArray && type.GetArrayRank() == 1)
				{
					Add(type.GetElementType());
				}

				if (type.IsGenericType)
				{
					foreach (var argument in type.GenericTypeArguments)
					{
						Add(argument);
					}
				}

				var id = _nextId;
				++_nextId;
				typeDescription = TypeDescription.Create(type, id, baseTypeDescription, members);
				_typeDescriptionsById.Add(id, typeDescription);
				_typeDescriptions.Add(type, typeDescription);
				_typesToId.Add(type, id);
				_idToTypes.Add(id, type);
			}

			return typeDescription;
		}

		public static TypeModel Create(IEnumerable<Type> supportedTypes)
		{
			var model = new TypeModel();
			foreach (var type in supportedTypes)
			{
				model.Add(type);
			}
			return model;
		}

		public void Write(SQLiteConnection connection)
		{
			using (var transaction = connection.BeginTransaction())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText =
						string.Format("INSERT OR IGNORE INTO {0} (id, typename, baseId) VALUES (@id, @typename, @baseId)", TypeTableName);
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
				}

				using (var command = connection.CreateCommand())
				{
					command.CommandText = string.Format("INSERT OR IGNORE INTO {0} (declaringTypeId, fieldId, name, fieldTypeId)" +
					                                    "VALUES (@declaringTypeId, @fieldId, @name, @fieldTypeId)",
					                                    FieldTableName);

					var declaringTypeIdParameter = command.Parameters.Add("@declaringTypeId", DbType.Int32);
					var fieldIdParameter = command.Parameters.Add("@fieldId", DbType.Int32);
					var name = command.Parameters.Add("@name", DbType.String);
					var fieldTypeIdParameter = command.Parameters.Add("@fieldTypeId", DbType.Int32);

					foreach (var description in _typeDescriptions.Values)
					{
						foreach (var memberDescription in description.Fields)
						{
							declaringTypeIdParameter.Value = description.TypeId;
							fieldIdParameter.Value = memberDescription.MemberId;
							name.Value = memberDescription.Name;
							fieldTypeIdParameter.Value = memberDescription.FieldTypeDescription.TypeId;

							command.ExecuteNonQuery();
						}
					}
				}

				transaction.Commit();
			}
		}

		[Pure]
		public static bool IsBuiltIn(Type type)
		{
			if (type.IsArray)
				return true;
			if (BuiltInProtobufTypes.Contains(type))
				return true;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return true;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
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
			return Database.TableExists(connection, TypeTableName);
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
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "declaringTypeId INTEGER NOT NULL," +
				                                    "fieldId INTEGER NOT NULL," +
				                                    "name TEXT NOT NULL," +
				                                    "fieldTypeId INTEGER NOT NULL," +
				                                    "UNIQUE(declaringTypeId, fieldId)," +
				                                    "FOREIGN KEY(declaringTypeId) REFERENCES {1}(id)," +
				                                    "FOREIGN KEY(fieldTypeId) REFERENCES {1}(id)" +
				                                    ")", FieldTableName, TypeTableName);
				command.ExecuteNonQuery();
			}
		}

		private void TryAddType(int typeId, string typeName, int? baseId, TypeResolver typeResolver)
		{
			try
			{
				_nextId = Math.Max(_nextId, typeId + 1);
				var type = typeResolver.Resolve(typeName);
				var baseType = baseId != null ? TryGetType(baseId.Value) : null;
				Add(typeName, type, typeId, baseType, baseId, fields: null);

				if (type == null)
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

		private IReadOnlyList<FieldDescription> CreateFieldDescriptions(IReadOnlyList<FieldInfo> fields,
		                                                                  IReadOnlyList<PropertyInfo> properties,
		                                                                  TypeDescription baseTypeDescription)
		{
			int nextId = 1;

			int GetNextId()
			{
				int i;
				for (i = nextId; i < short.MaxValue; ++i)
				{
					if (baseTypeDescription != null)
					{
						if (i != baseTypeDescription.TypeId) break;
					}
					else
					{
						break;
					}
				}

				nextId = i + 1;
				return i;
			}

			var members = new List<FieldDescription>(fields.Count + properties.Count);
			foreach (var field in fields)
			{
				var typeDescription = Add(field.FieldType);
				var id = GetNextId();
				var description = FieldDescription.Create(field, typeDescription, id);
				members.Add(description);
			}

			foreach (var property in properties)
			{
				var typeDescription = Add(property.PropertyType);
				var id = GetNextId();
				var description = FieldDescription.Create(property, typeDescription, id);
				members.Add(description);
			}

			return members;
		}

		private void Add(string typename,
		                     Type type,
		                     int typeId,
		                     Type baseType,
		                     int? baseTypeId,
		                     IEnumerable<FieldDescription> fields)
		{
			//var baseTypeDescription = baseType != null ? _typeDescriptions[baseType] : null;
			var baseTypeDescription = baseTypeId != null ? _typeDescriptionsById[baseTypeId.Value] : null;
			var typeDescription = TypeDescription.Create(type,
			                                             typename,
			                                             typeId,
			                                             baseTypeDescription,
			                                             fields);

			_typeDescriptionsById.Add(typeId, typeDescription);
			if (type != null)
			{
				_typeDescriptions.Add(type, typeDescription);
				_typesToId.Add(type, typeId);
				_idToTypes.Add(typeId, type);
			}
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
				return Add(typeof(object));

			var baseType = type.BaseType;
			if (baseType == null)
				return null;

			// Let's find out if this type implements a serializable interface!
			var allInterfaces = type.GetInterfaces();
			var baseTypeInterfaces = baseType.GetInterfaces();
			var interfaces = allInterfaces.Except(baseTypeInterfaces).ToList();
			var serializable = FindSerializableInterface(type, interfaces);
			if (serializable != null)
				return Add(serializable);

			return Add(baseType);
		}

		private static Type FindSerializableInterface(Type type, List<Type> interfaces)
		{
			var serializable = interfaces.Where(IsSerializableInterface).ToList();
			if (serializable.Count == 0)
				return null;

			if (serializable.Count > 1)
			{
				var interfaceNames = string.Join(", ", interfaces.Select(x => x.FullName));
				throw new ArgumentException(string.Format("The type '{0}' implements too many interfaces with the [SerializableContract] attribute! It should implement no more than 1 but actually implements: {1}",
				                                          type.FullName,
				                                          interfaceNames));
			}

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

		public void ThrowOnBreakingChanges(TypeModel typeModel)
		{
			foreach (var pair in _typeDescriptions)
			{
				var type = pair.Key;
				var description = pair.Value;

				if (typeModel._typeDescriptions.TryGetValue(type, out var otherDescription))
				{
					description.ThrowOnBreakingChanges(otherDescription);
				}
			}
		}
	}
}