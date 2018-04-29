using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using IsabelDb.Serializers;
using IsabelDb.Stores;
using ProtoBuf.Meta;

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

		private readonly SQLiteConnection _connection;

		private readonly Dictionary<string, IInternalObjectStore> _dictionaries;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

		static ObjectStores()
		{
			var nativeSerializers = new Dictionary<Type, ISQLiteSerializer>
			{
				{typeof(ushort), new UInt16Serializer()},
				{typeof(short), new Int16Serializer()},
				{typeof(uint), new UInt32Serializer()},
				{typeof(int), new Int32Serializer()},
				{typeof(long), new Int64Serializer()},
				{typeof(IPAddress), new IpAddressSerializer()},
				{typeof(string), new StringSerializer()}
			};
			NativeSerializers = nativeSerializers;
		}

		public ObjectStores(SQLiteConnection connection,
		                    TypeModel typeModel,
		                    TypeStore typeStore)
		{
			_connection = connection;
			_typeModel = typeModel;
			_typeStore = typeStore;
			_dictionaries = new Dictionary<string, IInternalObjectStore>();
		}

		public IDictionaryObjectStore<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			if (!_dictionaries.TryGetValue(name, out var store))
			{
				if (!TryRetrieveTableNameFor(name, out var tableName)) tableName = AddTable(name, typeof(TValue));
				store = CreateObjectStore<TKey, TValue>(tableName);
				_dictionaries.Add(name, store);
			}

			if (!(store is IDictionaryObjectStore<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                store.ObjectType.FullName));

			return target;
		}

		private IInternalObjectStore CreateObjectStore<TKey, TValue>(string tableName)
		{
			var keySerializer = GetSerializer<TKey>();
			var valueSerializer = GetSerializer<TValue>();
			return new DictionaryObjectStore<TKey, TValue>(_connection,
			                                               tableName,
			                                               keySerializer,
			                                               valueSerializer);
		}

		private ISQLiteSerializer<T> GetSerializer<T>()
		{
			if (NativeSerializers.TryGetValue(typeof(T), out var serializer))
				return (ISQLiteSerializer<T>) serializer;

			return new GenericSerializer<T>(_typeModel, _typeStore);
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
				                                    "typeId INTEGER NOT NULL" +
				                                    ")", TableName);

				command.ExecuteNonQuery();
			}
		}

		private bool TryRetrieveTableNameFor(string name, out string tableName)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT tableName FROM {0} WHERE name = @name LIMIT 0,1", TableName);
				command.Parameters.AddWithValue("@name", name);
				tableName = command.ExecuteScalar() as string;
				return tableName != null;
			}
		}

		private string AddTable(string name, Type type)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (name, tableName, typeId)" +
				                                    "VALUES (@name, @tableName, @typeId)", TableName);

				var typeId = _typeStore.GetOrCreateTypeId(type);

				var tableName = CreateTableNameFor(name);

				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@tableName", tableName);
				command.Parameters.AddWithValue("@typeId", typeId);

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