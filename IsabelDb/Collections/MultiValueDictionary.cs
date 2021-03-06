﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
		private readonly string _removeQuery;
		private readonly string _putQuery;
		private readonly string _getByKey;
		private readonly string _existsQuery;
		private readonly string _existsRowQuery;
		private readonly string _getAll;
		private readonly string _getAllKeys;
		private readonly string _removeRowQuery;
		private readonly string _getByRowId;

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

			_existsQuery = string.Format("SELECT EXISTS(SELECT * FROM {0} WHERE key = @key)", _tableName);
			_existsRowQuery = string.Format("SELECT EXISTS(SELECT * FROM {0} WHERE rowid = @rowid)", tableName);
			_putQuery = string.Format("INSERT INTO {0} (key, value) VALUES (@key, @value)", _tableName);
			_getAllKeys = string.Format("SELECT key FROM {0}", _tableName);
			_getAll = string.Format("SELECT key, value FROM {0}", _tableName);
			_getByKey = string.Format("SELECT value FROM {0} where key = @key", _tableName);
			_getByRowId = string.Format("SELECT value FROM {0} WHERE rowid = @rowid", tableName);
			_removeRowQuery = string.Format("DELETE FROM {0} WHERE rowid = @rowid", _tableName);
			_removeQuery = string.Format("DELETE FROM {0} WHERE key = @key", _tableName);
		}

		#region Implementation of IMultiValueDictionary<TKey,TValue>

		public RowId Put(TKey key, TValue value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (key == null)
				throw new ArgumentNullException(nameof(key));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.Parameters.AddWithValue("@value", _valueSerializer.Serialize(value));
				command.ExecuteNonQuery();

				var id = _connection.LastInsertRowId;
				return new RowId(id);
			}
		}

		public IReadOnlyList<RowId> PutMany(TKey key, IEnumerable<TValue> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var value in values)
				{
					valueParameter.Value = _valueSerializer.Serialize(value);
					command.ExecuteNonQuery();

					var id = _connection.LastInsertRowId;
					ids.Add(new RowId(id));
				}

				transaction.Commit();
			}

			return ids;
		}

		public IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (values == null)
				throw new ArgumentNullException(nameof(values));

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var pair in values)
				{
					if (pair.Key == null)
						throw new ArgumentException("Keys must not be null.");

					keyParameter.Value = _keySerializer.Serialize(pair.Key);

					foreach (var value in pair.Value)
					{
						valueParameter.Value = _valueSerializer.Serialize(value);
						command.ExecuteNonQuery();

						var id = _connection.LastInsertRowId;
						ids.Add(new RowId(id));
					}
				}

				transaction.Commit();
			}

			return ids;
		}

		public IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (values == null)
				throw new ArgumentNullException(nameof(values));

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;

				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				foreach (var pair in values)
				{
					if (pair.Key == null)
						throw new ArgumentException("Keys are not allowed to be null.");

					keyParameter.Value = _keySerializer.Serialize(pair.Key);
					valueParameter.Value = _valueSerializer.Serialize(pair.Value);
					command.ExecuteNonQuery();

					var id = _connection.LastInsertRowId;
					ids.Add(new RowId(id));
				}

				transaction.Commit();
			}

			return ids;
		}

		public void Remove(RowId row)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeRowQuery;
				command.Parameters.AddWithValue("@rowid", row.Id);
				command.ExecuteNonQuery();
			}
		}

		public void RemoveMany(IEnumerable<RowId> rows)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (rows == null)
				throw new ArgumentNullException(nameof(rows));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeRowQuery;
				var rowId = command.Parameters.Add("@rowid", DbType.Int64);
				foreach (var row in rows)
				{
					rowId.Value = row.Id;
					command.ExecuteNonQuery();
				}
			}
		}

		public IEnumerable<TKey> GetAllKeys()
		{
			ThrowIfDropped();

			return GetAllKeysInternal();
		}

		public bool ContainsKey(TKey key)
		{
			ThrowIfDropped();

			if (key == null)
				throw new ArgumentNullException(nameof(key));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _existsQuery;
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				var value = Convert.ToInt64(command.ExecuteScalar());
				return value != 0;
			}
		}

		public bool ContainsRow(RowId row)
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _existsRowQuery;
				command.Parameters.AddWithValue("@rowid", row.Id);

				var value = Convert.ToInt64(command.ExecuteScalar());
				return value != 0;
			}
		}

		public TValue GetValue(RowId row)
		{
			if (!TryGetValue(row, out var value))
				throw new KeyNotFoundException();

			return value;
		}

		public bool TryGetValue(RowId row, out TValue value)
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByRowId;
				command.Parameters.AddWithValue("@rowid", row.Id);

				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
						value = default(TValue);
						return false;
					}

					return _valueSerializer.TryDeserialize(reader, 0, out value);
				}
			}
		}

		public IEnumerable<TValue> GetValues(IEnumerable<RowId> rows)
		{
			ThrowIfDropped();
			if (rows == null)
				throw new ArgumentNullException(nameof(rows));
			return GetValuesInternal(rows);
		}

		public IEnumerable<TValue> GetValues(TKey key)
		{
			ThrowIfDropped();
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			return GetValuesInternal(key);
		}

		public IEnumerable<TValue> GetValues(IEnumerable<TKey> keys)
		{
			ThrowIfDropped();
			if (keys == null)
				throw new ArgumentNullException(nameof(keys));
			return GetValuesInternal(keys);
		}

		public IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> GetAll()
		{
			ThrowIfDropped();

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
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeQuery;
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.ExecuteNonQuery();
			}
		}

		public void RemoveMany(IEnumerable<TKey> keys)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeQuery;
				var parameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
				foreach (var key in keys)
				{
					parameter.Value = _keySerializer.Serialize(key);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		#endregion

		#region Implementation of IInternalCollection

		public override CollectionType Type => CollectionType.MultiValueDictionary;

		public override Type KeyType => typeof(TKey);

		public override string KeyTypeName => null;

		#endregion

		private IEnumerable<TValue> GetValuesInternal(TKey key)
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

		private IEnumerable<TValue> GetValuesInternal(IEnumerable<TKey> keys)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByKey;
				var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);

				foreach (var key in keys)
				{
					if (key == null)
						throw new ArgumentException("Keys are not allowed to be null.");

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

		private IEnumerable<TValue> GetValuesInternal(IEnumerable<RowId> rows)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByRowId;
				var rowId = command.Parameters.Add("@rowid", DbType.Int64);

				foreach (var row in rows)
				{
					rowId.Value = row.Id;
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							if (_valueSerializer.TryDeserialize(reader, 0, out var value))
								yield return value;
						}
					}
				}
			}
		}

		private IEnumerable<TKey> GetAllKeysInternal()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getAllKeys;

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_keySerializer.TryDeserialize(reader, 0, out var key))
							yield return key;
					}
				}
			}
		}

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = 
					string.Format("CREATE TABLE IF NOT EXISTS {0} (rowid INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
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
