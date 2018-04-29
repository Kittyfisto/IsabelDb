using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb
{
	internal sealed class DictionaryObjectStore<T>
		: IDictionaryObjectStore<T>
	{
		private readonly SQLiteConnection _connection;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;
		private readonly string _tableName;

		public DictionaryObjectStore(SQLiteConnection connection,
		                   TypeModel typeModel,
		                   TypeStore typeStore,
		                   string tableName)
		{
			_connection = connection;
			_typeModel = typeModel;
			_typeStore = typeStore;
			_tableName = tableName;

			CreateObjectTableIfNecessary();
		}

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS {0} (" +
				                                    "key TEXT PRIMARY KEY NOT NULL," +
				                                    "type INTEGER NOT NULL," +
				                                    "value BLOB NOT NULL" +
				                                    ")", _tableName);
				command.ExecuteNonQuery();
			}
		}

		public IEnumerable<KeyValuePair<string, T>> GetAll()
		{
			using (var command = CreateCommand("SELECT key, type, value FROM {0}"))
			{
				return Get(command);
			}
		}

		public IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys)
		{
			using (var command = CreateCommand("SELECT type, value FROM {0} WHERE key = @key"))
			{
				var keyParameter = command.Parameters.Add("@key", DbType.String);
				var ret = new List<KeyValuePair<string, T>>();
				foreach (var key in keys)
				{
					keyParameter.Value = key;
					using (var reader = command.ExecuteReader())
					{
						if (TryReadValue(reader, out var value))
						{
							ret.Add(new KeyValuePair<string, T>(key, value));
						}
					}
				}

				return ret;
			}
		}

		public IEnumerable<KeyValuePair<string, T>> Get(params string[] keys)
		{
			return Get((IEnumerable<string>) keys);
		}

		public T Get(string key)
		{
			using (var command = CreateCommand("SELECT type, value FROM {0} WHERE key = @key"))
			{
				command.Parameters.AddWithValue("@key", key);
				using (var reader = command.ExecuteReader())
				{
					TryReadValue(reader, out var value);
					return value;
				}
			}
		}

		public void Put(string key, T value)
		{
			if (value == null)
			{
				Remove(key);
			}
			else
			{
				InsertOrReplace(key, value);
			}
		}

		public void Put(IEnumerable<KeyValuePair<string, T>> values)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("INSERT OR REPLACE INTO {0} (key, type, value) VALUES" +
				                                   "(@key, @typeId, @value)"))
				{
					var keyParameter = command.Parameters.Add("@key", DbType.String);
					var typeIdParameter = command.Parameters.Add("@typeId", DbType.Int32);
					var valueParameter = command.Parameters.Add("@value", DbType.Binary);

					foreach (var pair in values)
					{
						var value = pair.Value;
						var type = value.GetType();
						var typeId = _typeStore.GetOrCreateTypeId(type);

						keyParameter.Value = pair.Key;
						typeIdParameter.Value = typeId;
						valueParameter.Value = Serialize(value);
						command.ExecuteNonQuery();
					}

					transaction.Commit();
				}
			}
		}

		public void Remove(string key)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("DELETE FROM {0} WHERE key = @key"))
				{
					command.Parameters.AddWithValue("@key", key);
					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		public void Clear()
		{
			using (var command = CreateCommand("DELETE FROM {0}"))
			{
				command.ExecuteNonQuery();
			}
		}

		public int Count()
		{
			using (var command = CreateCommand("SELECT COUNT(*) FROM {0}"))
			{
				var count = Convert.ToInt32(command.ExecuteScalar());
				return count;
			}
		}

		private void InsertOrReplace(string key, object value)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("INSERT OR REPLACE INTO {0} (key, type, value) VALUES" +
				                                   "(@key, @typeId, @value)"))
				{
					var typeId = _typeStore.GetOrCreateTypeId(value.GetType());
					var serializedValue = Serialize(value);
					command.Parameters.AddWithValue("@key", key);
					command.Parameters.AddWithValue("@typeId", typeId);
					command.Parameters.AddWithValue("@value", serializedValue);
					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		private IEnumerable<KeyValuePair<string, T>> Get(SQLiteCommand command)
		{
			using (var reader = command.ExecuteReader())
			{
				var ret = new List<KeyValuePair<string, T>>();
				while (reader.Read())
				{
					var key = reader.GetString(0);
					var typeId = reader.GetInt32(1);
					var type = _typeStore.GetTypeFromTypeId(typeId);
					if (type != null)
					{
						var serializedValue = (byte[]) reader.GetValue(2);
						var value = Deserialize(type, serializedValue);

						if (value is T desiredValue)
						{
							ret.Add(new KeyValuePair<string, T>(key, desiredValue));
						}
					}
				}
				return ret;
			}
		}

		private bool TryReadValue(SQLiteDataReader reader, out T value)
		{
			if (!reader.Read())
			{
				value = default(T);
				return false;
			}

			var typeId = reader.GetInt32(0);
			var type = _typeStore.GetTypeFromTypeId(typeId);
			if (type == null)
			{
				value = default(T);
				return false;
			}

			var serializedValue = (byte[]) reader.GetValue(1);
			var deserializedValue = Deserialize(type, serializedValue);
			if (!(deserializedValue is T))
			{
				value = default(T);
				return false;
			}

			value = (T) deserializedValue;
			return true;
		}

		private SQLiteCommand CreateCommand(string text)
		{
			var command = _connection.CreateCommand();
			command.CommandText = string.Format(text, _tableName);
			return command;
		}

		private SQLiteTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		private byte[] Serialize(object value)
		{
			using (var stream = new MemoryStream())
			{
				_typeModel.Serialize(stream, value);
				return stream.ToArray();
			}
		}

		private object Deserialize(Type type, byte[] serializedValue)
		{
			using (var stream = new MemoryStream(serializedValue))
			{
				var value = _typeModel.Deserialize(stream, null, type);
				return value;
			}
		}
	}
}