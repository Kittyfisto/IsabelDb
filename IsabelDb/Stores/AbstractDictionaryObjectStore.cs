using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal abstract class AbstractDictionaryObjectStore<TKey, TValue>
		: IDictionaryObjectStore<TKey, TValue>
			, IInternalObjectStore
	{
		private readonly SQLiteConnection _connection;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;
		private readonly string _tableName;

		protected AbstractDictionaryObjectStore(SQLiteConnection connection,
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

		protected abstract object SerializeKey(TKey key);
		protected abstract TKey DeserializeKey(SQLiteDataReader reader, int ordinal);
		protected abstract DbType KeyDatabaseType { get; }
		protected abstract string KeyColumnDefinition { get; }

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS {0} (" +
				                                    "key {1}," +
				                                    "type INTEGER NOT NULL," +
				                                    "value BLOB NOT NULL" +
				                                    ")",
				                                    _tableName,
				                                    KeyColumnDefinition);
				command.ExecuteNonQuery();
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
		{
			using (var command = CreateCommand("SELECT key, type, value FROM {0}"))
			{
				return Get(command);
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> Get(IEnumerable<TKey> keys)
		{
			using (var command = CreateCommand("SELECT type, value FROM {0} WHERE key = @key"))
			{
				var keyParameter = command.Parameters.Add("@key", DbType.String);
				var ret = new List<KeyValuePair<TKey, TValue>>();
				foreach (var key in keys)
				{
					keyParameter.Value = key;
					using (var reader = command.ExecuteReader())
					{
						if (TryReadValue(reader, out var value))
						{
							ret.Add(new KeyValuePair<TKey, TValue>(key, value));
						}
					}
				}

				return ret;
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> Get(params TKey[] keys)
		{
			return Get((IEnumerable<TKey>) keys);
		}

		public TValue Get(TKey key)
		{
			using (var command = CreateCommand("SELECT type, value FROM {0} WHERE key = @key"))
			{
				command.Parameters.AddWithValue("@key", SerializeKey(key));
				using (var reader = command.ExecuteReader())
				{
					TryReadValue(reader, out var value);
					return value;
				}
			}
		}

		public void Put(TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (value == null)
			{
				Remove(key);
			}
			else
			{
				InsertOrReplace(key, value);
			}
		}

		public void Put(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("INSERT OR REPLACE INTO {0} (key, type, value) VALUES" +
				                                   "(@key, @typeId, @value)"))
				{
					var keyParameter = command.Parameters.Add("@key", KeyDatabaseType);
					var typeIdParameter = command.Parameters.Add("@typeId", DbType.Int32);
					var valueParameter = command.Parameters.Add("@value", DbType.Binary);

					foreach (var pair in values)
					{
						var value = pair.Value;
						var type = value.GetType();
						var typeId = _typeStore.GetOrCreateTypeId(type);

						keyParameter.Value = SerializeKey(pair.Key);
						typeIdParameter.Value = typeId;
						valueParameter.Value = Serialize(value);
						command.ExecuteNonQuery();
					}

					transaction.Commit();
				}
			}
		}

		public void Remove(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("DELETE FROM {0} WHERE key = @key"))
				{
					command.Parameters.AddWithValue("@key", SerializeKey(key));
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

		private void InsertOrReplace(TKey key, TValue value)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand("INSERT OR REPLACE INTO {0} (key, type, value) VALUES" +
				                                   "(@key, @typeId, @value)"))
				{
					var typeId = _typeStore.GetOrCreateTypeId(value.GetType());
					var serializedValue = Serialize(value);
					command.Parameters.AddWithValue("@key", SerializeKey(key));
					command.Parameters.AddWithValue("@typeId", typeId);
					command.Parameters.AddWithValue("@value", serializedValue);
					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		private IEnumerable<KeyValuePair<TKey, TValue>> Get(SQLiteCommand command)
		{
			using (var reader = command.ExecuteReader())
			{
				var ret = new List<KeyValuePair<TKey, TValue>>();
				while (reader.Read())
				{
					var key = DeserializeKey(reader, 0);
					var typeId = reader.GetInt32(1);
					var type = _typeStore.GetTypeFromTypeId(typeId);
					if (type != null)
					{
						var serializedValue = (byte[]) reader.GetValue(2);
						var value = Deserialize(type, serializedValue);

						if (value is TValue desiredValue)
						{
							ret.Add(new KeyValuePair<TKey, TValue>(key, desiredValue));
						}
					}
				}
				return ret;
			}
		}

		private bool TryReadValue(SQLiteDataReader reader, out TValue value)
		{
			if (!reader.Read())
			{
				value = default(TValue);
				return false;
			}

			var typeId = reader.GetInt32(0);
			var type = _typeStore.GetTypeFromTypeId(typeId);
			if (type == null)
			{
				value = default(TValue);
				return false;
			}

			var serializedValue = (byte[]) reader.GetValue(1);
			var deserializedValue = Deserialize(type, serializedValue);
			if (!(deserializedValue is TValue))
			{
				value = default(TValue);
				return false;
			}

			value = (TValue) deserializedValue;
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

		private byte[] Serialize(TValue value)
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

		public Type ObjectType => typeof(TValue);
	}
}