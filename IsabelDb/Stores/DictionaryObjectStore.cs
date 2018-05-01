using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using IsabelDb.Serializers;

namespace IsabelDb.Stores
{
	internal sealed class DictionaryObjectStore<TKey, TValue>
		: IDictionaryObjectStore<TKey, TValue>
			, IInternalObjectStore

	{
		private readonly SQLiteConnection _connection;

		private readonly ISQLiteSerializer<TKey> _keySerializer;

		#region Table

		private readonly string _table;

		#endregion

		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;

		public DictionaryObjectStore(SQLiteConnection connection,
		                             string tableName,
		                             ISQLiteSerializer<TKey> keySerializer,
		                             ISQLiteSerializer<TValue> valueSerializer)
		{
			_connection = connection;
			_tableName = tableName;
			_keySerializer = keySerializer;
			_valueSerializer = valueSerializer;

			CreateObjectTableIfNecessary(out _table);

			_getAllQuery = string.Format("SELECT key, value FROM {0}", tableName);
			_getManyQuery = string.Format("SELECT key, value FROM {0} WHERE key = @key", tableName);
			_getQuery = string.Format("SELECT key, value FROM {0} WHERE key = @key", tableName);
			_countQuery = string.Format("SELECT COUNT(*) FROM {0}", _tableName);
			_deleteAllQuery = string.Format("DELETE FROM {0}", _tableName);
			_deleteQuery = string.Format("DELETE FROM {0} WHERE key = @key", _tableName);
			_putQuery = PutQuery(_tableName);
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
		{
			using (var command = CreateCommand(_getAllQuery))
			using (var reader = command.ExecuteReader())
			{
				var ret = new List<KeyValuePair<TKey, TValue>>();
				while (reader.Read())
					if (_keySerializer.TryDeserialize(reader, valueOrdinal: 0, value: out var key))
						if (_valueSerializer.TryDeserialize(reader, valueOrdinal: 1, value: out var value))
							ret.Add(new KeyValuePair<TKey, TValue>(key, value));

				return ret;
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetMany(IEnumerable<TKey> keys)
		{
			using (var command = CreateCommand(_getManyQuery))
			{
				var keyParameter = command.Parameters.Add("@key", DbType.String);
				var ret = new List<KeyValuePair<TKey, TValue>>();
				foreach (var key in keys)
				{
					keyParameter.Value = key;
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
							if (_valueSerializer.TryDeserialize(reader, valueOrdinal: 1, value: out var value))
							{
								ret.Add(new KeyValuePair<TKey, TValue>(key, value));
							}
					}
				}

				return ret;
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> GetMany(params TKey[] keys)
		{
			return GetMany((IEnumerable<TKey>) keys);
		}

		public TValue Get(TKey key)
		{
			using (var command = CreateCommand(_getQuery))
			{
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));

				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
						return default(TValue);

					_valueSerializer.TryDeserialize(reader, valueOrdinal: 1, value: out var value);
					return value;
				}
			}
		}

		public void Put(TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				Remove(key);
			else
				InsertOrReplace(key, value);
		}

		public void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			using (var transaction = BeginTransaction())
			using (var command = CreateCommand(_putQuery))
			{
				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var pair in values)
				{
					keyParameter.Value = _keySerializer.Serialize(pair.Key);
					valueParameter.Value = _valueSerializer.Serialize(pair.Value);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		public void Remove(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand(_deleteQuery))
				{
					command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		public void Clear()
		{
			using (var command = CreateCommand(_deleteAllQuery))
			{
				command.ExecuteNonQuery();
			}
		}

		public int Count()
		{
			using (var command = CreateCommand(_countQuery))
			{
				var count = Convert.ToInt32(command.ExecuteScalar());
				return count;
			}
		}

		public Type ObjectType => typeof(TValue);

		#region Overrides of Object

		public override string ToString()
		{
			return _table;
		}

		#endregion

		private void InsertOrReplace(TKey key, TValue value)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand(_putQuery))
				{
					var serializedKey = _keySerializer.Serialize(key);
					command.Parameters.AddWithValue("@key", serializedKey);
					var serializedValue = _valueSerializer.Serialize(value);
					command.Parameters.AddWithValue("@value", serializedValue);

					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		private SQLiteCommand CreateCommand(string text)
		{
			var command = _connection.CreateCommand();
			command.CommandText = text;
			return command;
		}

		private SQLiteTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		private void CreateObjectTableIfNecessary(out string table)
		{
			using (var command = _connection.CreateCommand())
			{
				var builder = new StringBuilder();
				builder.AppendFormat("CREATE TABLE IF NOT EXISTS {0} (", _tableName);

				builder.AppendFormat("key {0} PRIMARY KEY NOT NULL", SQLiteHelper.GetAffinity(_keySerializer.DatabaseType));
				builder.AppendFormat(",value {0} NOT NULL", SQLiteHelper.GetAffinity(_valueSerializer.DatabaseType));
				builder.Append(")");
				command.CommandText = table = builder.ToString();
				command.ExecuteNonQuery();
			}
		}

		private static string PutQuery(string tableName)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("INSERT OR REPLACE INTO {0} ", tableName);
			builder.Append("(key, value) VALUES (@key, @value)");
			return builder.ToString();
		}

		#region Queries

		private readonly string _countQuery;
		private readonly string _deleteAllQuery;
		private readonly string _deleteQuery;
		private readonly string _getAllQuery;
		private readonly string _getManyQuery;
		private readonly string _getQuery;
		private readonly string _putQuery;

		#endregion
	}
}