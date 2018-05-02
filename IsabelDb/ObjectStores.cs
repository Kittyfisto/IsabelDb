using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using IsabelDb.Serializers;
using IsabelDb.Stores;
using IsabelDb.TypeModel;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for providing access to various (user) created stores.
	///     Ensures type safety when opening a database again, etc...
	/// </summary>
	internal sealed class ObjectStores
	{
		private const string TableName = "isabel_stores";

		/// <summary>
		///     A list of serializers for .NET types which natively map to SQLite types
		///     (int, string, etc...).
		/// </summary>
		private static readonly IReadOnlyDictionary<Type, ISQLiteSerializer> NativeSerializers;

		private readonly Dictionary<string, IInternalObjectStore> _bags;

		private readonly SQLiteConnection _connection;

		private readonly Dictionary<string, IInternalObjectStore> _dictionaries;
		private readonly Serializer _serializer;
		private readonly CompiledTypeModel _typeModel;

		static ObjectStores()
		{
			var nativeSerializers = new Dictionary<Type, ISQLiteSerializer>
			{
				{typeof(byte), new ByteSerializer()},
				{typeof(sbyte), new SByteSerializer()},
				{typeof(ushort), new UInt16Serializer()},
				{typeof(short), new Int16Serializer()},
				{typeof(uint), new UInt32Serializer()},
				{typeof(int), new Int32Serializer()},
				{typeof(long), new Int64Serializer()},
				{typeof(IPAddress), new IpAddressSerializer()},
				{typeof(string), new StringSerializer()},
				{typeof(float), new SingleSerializer()},
				{typeof(double), new DoubleSerializer()},
				{typeof(byte[]), new ByteArraySerializer()}
			};
			NativeSerializers = nativeSerializers;
		}

		public ObjectStores(SQLiteConnection connection,
		                    IReadOnlyList<Type> supportedTypes)
		{
			_connection = connection;

			_typeModel = TypeModel.TypeModel.Create(connection, supportedTypes);
			_serializer = new Serializer(_typeModel);
			_dictionaries = new Dictionary<string, IInternalObjectStore>();
			_bags = new Dictionary<string, IInternalObjectStore>();
		}

		public IDictionaryObjectStore<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			if (!_dictionaries.TryGetValue(name, out var store))
			{
				if (TryRetrieveTableNameFor(name, out var tableName, out var keyType, out var valueType))
				{
					EnsureTypeSafety(name,
					                 typeof(TKey), typeof(TValue),
					                 keyType, valueType);
				}
				else
				{
					if (!_typeModel.IsRegistered(typeof(TKey)))
						throw new
							ArgumentException(string.Format("The type '{0}' has not been registered when the database was created and thus may not be used as the key type in a collection",
							                                typeof(TKey).FullName));

					if (!_typeModel.IsRegistered(typeof(TValue)))
						throw new
							ArgumentException(string.Format("The type '{0}' has not been registered when the database was created and thus may not be used as the value type in a collection",
							                                typeof(TValue).FullName));

					tableName = AddTable(name, typeof(TKey), typeof(TValue));
				}

				store = CreateDictionary<TKey, TValue>(tableName);
				_dictionaries.Add(name, store);
			}

			if (!(store is IDictionaryObjectStore<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                store.ObjectType.FullName));

			return target;
		}

		public IBagObjectStore<T> GetBag<T>(string name)
		{
			if (!_bags.TryGetValue(name, out var store))
			{
				if (TryRetrieveTableNameFor(name, out var tableName, out var unused, out var valueType))
				{
					EnsureTypeSafety(name, expectedKeyType: null, expectedValueType: typeof(T), actualKeyType: null,
					                 actualValueType: valueType);
				}
				else
				{
					if (!_typeModel.IsRegistered(typeof(T)))
						throw new
							ArgumentException(string.Format("The type '{0}' has not been registered when the database was created and thus may not be used as the value type in a collection",
							                                typeof(T).FullName));

					tableName = AddTable(name, keyType: null, valueType: typeof(T));
				}

				store = CreateBag<T>(tableName);
				_bags.Add(name, store);
			}

			if (!(store is IBagObjectStore<T> target))
				throw new
					ArgumentException(string.Format("The bag '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                store.ObjectType.FullName));

			return target;
		}

		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return IsabelDb.TableExists(connection, TableName);
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "name STRING PRIMARY KEY NOT NULL," +
				                                    "tableName STRING UNIQUE NOT NULL," +
				                                    "keyType INTEGER," +
				                                    "valueType INTEGER NOT NULL" +
				                                    ")", TableName);

				command.ExecuteNonQuery();
			}
		}

		private static ProtoBuf.Meta.TypeModel CompileTypeModel(IEnumerable<Type> supportedTypes)
		{
			var typeModel = ProtoBuf.Meta.TypeModel.Create();
			foreach (var type in supportedTypes)
			{
				typeModel.Add(type, applyDefaultBehaviour: true);
			}
			return typeModel.Compile();
		}

		private void EnsureTypeSafety(string collectionName,
		                              Type expectedKeyType,
		                              Type expectedValueType,
		                              Type actualKeyType,
		                              Type actualValueType)
		{
			if (expectedKeyType != null)
			{
				if (actualKeyType == null)
					throw new TypeResolveException(string.Format("The key type of the dictionary '{0}' could not be resolved",
					                                             collectionName));

				if (expectedKeyType != actualKeyType)
					throw new
						TypeMismatchException(string.Format("The dictionary '{0}' has been stored to use keys of type '{1}' which does not match the requested key '{2}'!",
						                                    collectionName,
						                                    actualKeyType,
						                                    expectedKeyType));
			}

			if (actualValueType == null)
				throw new
					TypeResolveException(string.Format("A collection named '{0}' already exists but it's value type could not be resolved: If your intent is to re-use this existing collection, then you need to investigate why the type resolver could not resolve it's type. If your intent is to create a new collection, then you need to pick a different name",
					                                   collectionName));

			if (expectedValueType != actualValueType)
				throw new
					TypeMismatchException(string.Format("The collection '{0}' has been stored to use values of type '{1}' which does not match the requested key '{2}'!",
					                                    collectionName,
					                                    actualValueType,
					                                    expectedValueType));
		}

		private IInternalObjectStore CreateDictionary<TKey, TValue>(string tableName)
		{
			var keySerializer = GetSerializer<TKey>();
			var valueSerializer = GetSerializer<TValue>();
			return new DictionaryObjectStore<TKey, TValue>(_connection,
			                                               tableName,
			                                               keySerializer,
			                                               valueSerializer);
		}

		private IInternalObjectStore CreateBag<T>(string tableName)
		{
			var serializer = GetSerializer<T>();
			return new BagObjectStore<T>(_connection,
			                             serializer,
			                             tableName);
		}

		private ISQLiteSerializer<T> GetSerializer<T>()
		{
			if (NativeSerializers.TryGetValue(typeof(T), out var serializer))
				return (ISQLiteSerializer<T>) serializer;

			return new GenericSerializer<T>(_serializer);
		}

		private bool TryRetrieveTableNameFor(string name,
		                                     out string tableName,
		                                     out Type keyType,
		                                     out Type valueType)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT tableName, keyType, valueType FROM {0} WHERE name = @name LIMIT 0,1",
				                                    TableName);
				command.Parameters.AddWithValue("@name", name);
				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
						tableName = null;
						keyType = null;
						valueType = null;
						return false;
					}

					tableName = reader.GetString(i: 0);
					if (!reader.IsDBNull(i: 1))
						keyType = _typeModel.GetType(reader.GetInt32(i: 1));
					else
						keyType = null;

					valueType = _typeModel.GetType(reader.GetInt32(i: 2));
					return true;
				}
			}
		}

		private string AddTable(string name, Type keyType, Type valueType)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (name, tableName, keyType, valueType)" +
				                                    "VALUES (@name, @tableName, @keyType, @valueType)", TableName);

				var keyTypeId = keyType != null ? (int?) _typeModel.GetTypeId(keyType) : null;
				var valueTypeId = _typeModel.GetTypeId(valueType);

				var tableName = CreateTableNameFor(name);

				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@tableName", tableName);
				command.Parameters.AddWithValue("@keyType", keyTypeId);
				command.Parameters.AddWithValue("@valueType", valueTypeId);

				command.ExecuteNonQuery();

				return tableName;
			}
		}

		private string CreateTableNameFor(string name)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT(*) FROM {0}", TableName);
				var count = Convert.ToInt32(command.ExecuteScalar());
				return string.Format("ObjectStore_{0}", count);
			}
		}
	}
}