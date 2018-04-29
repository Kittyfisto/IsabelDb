using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

		private readonly Dictionary<string, IInternalObjectStore> _dictionaries;
		private readonly SQLiteConnection _connection;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

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
				if (!TryRetrieveTableNameFor(name, out var tableName))
				{
					tableName = AddTable(name, typeof(TValue));
				}
				store = CreateObjectStore<TKey, TValue>(tableName);
				_dictionaries.Add(name, store);
			}

			if (!(store is IDictionaryObjectStore<TKey, TValue> target))
				throw new ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
				                                          name,
				                                          store.ObjectType.FullName));

			return target;
		}

		private IInternalObjectStore CreateObjectStore<TKey, TValue>(string tableName)
		{
			var keyType = typeof(TKey);

			if (keyType == typeof(string))
				return new StringKeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			if (keyType == typeof(Int16))
				return new Int16KeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			if (keyType == typeof(UInt16))
				return new UInt16KeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			if (keyType == typeof(Int32))
				return new Int32KeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			if (keyType == typeof(UInt32))
				return new UInt32KeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			if (keyType == typeof(Int64))
				return new Int64KeyObjectStore<TValue>(_connection, _typeModel, _typeStore, tableName);

			throw new NotImplementedException();
			//return new GenericKeyObjectStore<TKey,TValue>(_connection, _typeModel, _typeStore, tableName);
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