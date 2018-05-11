using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class MultiValueDictionary<TKey, TValue>
		: AbstractCollection<TValue>
		, IMultiValueDictionary<TKey, TValue>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TKey> _keySerializer;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;
		private readonly string _countQuery;
		private readonly string _clearQuery;
		private readonly string _removeQuery;
		private readonly string _putQuery;
		private readonly string _getByKey;
		private readonly string _getAll;
		private long _lastId;

		public MultiValueDictionary(SQLiteConnection connection,
		                            string name,
		                            string tableName,
		                            ISQLiteSerializer<TKey> keySerializer,
		                            ISQLiteSerializer<TValue> valueSerializer,
		                            bool isReadOnly)
			: base(connection, name, tableName, valueSerializer, isReadOnly)
		{
			_connection = connection;
			_tableName = tableName;
			_keySerializer = keySerializer;
			_valueSerializer = valueSerializer;

			CreateObjectTableIfNecessary();

			_putQuery = string.Format("INSERT INTO {0} (id, key, value) VALUES (@id, @key, @value)", _tableName);
			_getAll = string.Format("SELECT key, value FROM {0}", _tableName);
			_getByKey = string.Format("SELECT value FROM {0} where key = @key", _tableName);
			_clearQuery = string.Format("DELETE FROM {0}", _tableName);
			_removeQuery = string.Format("DELETE FROM {0} WHERE key = @key", _tableName);
			_countQuery = string.Format("SELECT COUNT(*) FROM {0}", _tableName);

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT MAX(id) FROM {0}", _tableName);
				var value = command.ExecuteScalar();
				if (!Convert.IsDBNull(value))
				{
					_lastId = Convert.ToInt64(value);
				}
			}
		}

		#region Implementation of IMultiValueDictionary<TKey,TValue>

		public void Put(TKey key, TValue value)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				var id = Interlocked.Increment(ref _lastId);
				command.Parameters.AddWithValue("@id", id);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.Parameters.AddWithValue("@value", _valueSerializer.Serialize(value));
				command.ExecuteNonQuery();
			}
		}

		public void PutMany(TKey key, IEnumerable<TValue> values)
		{
			ThrowIfReadOnly();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				var idParameter = command.Parameters.Add("@id", DbType.Int64);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var value in values)
				{
					idParameter.Value = Interlocked.Increment(ref _lastId);
					valueParameter.Value = _valueSerializer.Serialize(value);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		public void PutMany(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> values)
		{
			ThrowIfReadOnly();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				var idParameter = command.Parameters.Add("@id", DbType.Int64);
				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var pair in values)
				{
					keyParameter.Value = _keySerializer.Serialize(pair.Key);

					foreach (var value in pair.Value)
					{
						idParameter.Value = Interlocked.Increment(ref _lastId);
						valueParameter.Value = _valueSerializer.Serialize(value);
						command.ExecuteNonQuery();
					}
				}

				transaction.Commit();
			}
		}

		public IEnumerable<TValue> GetValues(TKey key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByKey;
				command.Parameters.AddWithValue("key", _keySerializer.Serialize(key));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 0, out var value))
						{
							yield return value;
						}
					}
				}
			}
		}

		public IEnumerable<TValue> GetValues(IEnumerable<TKey> keys)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByKey;
				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);

				foreach (var key in keys)
				{
					keyParameter.Value = _keySerializer.Serialize(key);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (_valueSerializer.TryDeserialize(reader, 0, out var value))
							{
								yield return value;
							}
						}
					}
				}
			}
		}

		public IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> GetAll()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getAll;

				var tmp = new System.Collections.Generic.Dictionary<TKey, List<TValue>>();
				var ret = new System.Collections.Generic.Dictionary<TKey, IEnumerable<TValue>>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_keySerializer.TryDeserialize(reader, 0, out var key))
						{
							if (_valueSerializer.TryDeserialize(reader, 1, out var value))
							{
								if (!tmp.TryGetValue(key, out var values))
								{
									values = new List<TValue>();
									tmp.Add(key, values);
									ret.Add(key, values);
								}

								values.Add(value);
							}
						}
					}
				}

				return ret;
			}
		}

		public void RemoveAll(TKey key)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeQuery;
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region Implementation of IInternalCollection

		public override CollectionType Type => CollectionType.MultiValueDictionary;

		public override Type KeyType => typeof(TKey);

		#endregion

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = 
					string.Format("CREATE TABLE IF NOT EXISTS {0} (id INTEGER PRIMARY KEY NOT NULL, " +
					              "key {1} NOT NULL, " +
					              "value {2} NOT NULL);" +
					              "CREATE INDEX IF NOT EXISTS {0}_keys ON {0}(key)",
					              _tableName,
					              SQLiteHelper.GetAffinity(_keySerializer.DatabaseType),
					              SQLiteHelper.GetAffinity(_valueSerializer.DatabaseType));

				command.ExecuteNonQuery();
			}
		}
	}
}
