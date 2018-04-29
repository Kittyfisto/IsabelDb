using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Text;
using IsabelDb.Serializers;

namespace IsabelDb.Stores
{
	internal sealed class DictionaryObjectStore<TKey, TValue>
		: IDictionaryObjectStore<TKey, TValue>
			, IInternalObjectStore
	{
		private readonly SQLiteConnection _connection;

		#region Queries

		private readonly string _countQuery;
		private readonly string _deleteAllQuery;
		private readonly string _deleteQuery;
		private readonly string _getAllQuery;
		private readonly string _getManyQuery;
		private readonly string _getQuery;
		private readonly string _putQuery;

		#endregion

		#region Table

		private readonly string _table;
		private readonly int _keyOrdinal;
		private readonly int _keyTypeOrdinal;
		private readonly int _valueOrdinal;
		private readonly int _valueTypeOrdinal;

		#endregion

		private readonly ISQLiteSerializer<TKey> _keySerializer;
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

			CreateObjectTableIfNecessary(out _keyOrdinal,
			                             out _keyTypeOrdinal,
			                             out _valueOrdinal,
			                             out _valueTypeOrdinal,
			                             out _table);

			_getAllQuery = CreateGetAll(_tableName, _keySerializer, _valueSerializer);
			_getManyQuery = CreateGetMany(_tableName, _keySerializer, _valueSerializer);
			_getQuery = CreateGet(_tableName, _keySerializer, _valueSerializer);
			_countQuery = string.Format("SELECT COUNT(*) FROM {0}", _tableName);
			_deleteAllQuery = string.Format("DELETE FROM {0}", _tableName);
			_deleteQuery = string.Format("DELETE FROM {0} WHERE key = @key", _tableName);
			_putQuery = PutQuery(_tableName, _keySerializer, _valueSerializer);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return _table;
		}

		#endregion

		public IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
		{
			using (var command = CreateCommand(_getAllQuery))
			{
				return Get(command);
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
						{
							var value = ReadValue(reader);
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
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key, out var typeId));
				if (_keySerializer.StorePerValueTypeInformation)
				{
					command.Parameters.AddWithValue("@keyType", typeId);
				}

				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
						return default(TValue);

					return ReadValue(reader);
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
			{
				using (var command = CreateCommand(_putQuery))
				{
					var keyParameter = command.Parameters.Add("@key", _keySerializer.DatabaseType);
					SQLiteParameter keyTypeIdParameter = null;
					if (_keySerializer.StorePerValueTypeInformation)
						keyTypeIdParameter = command.Parameters.Add("@keyType", DbType.Int32);

					var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);
					SQLiteParameter valueTypeIdParameter = null;
					if (_valueSerializer.StorePerValueTypeInformation)
						valueTypeIdParameter = command.Parameters.Add("@valueType", DbType.Int32);

					foreach (var pair in values)
					{
						keyParameter.Value = _keySerializer.Serialize(pair.Key, out var keyTypeId);
						if (_keySerializer.StorePerValueTypeInformation)
							keyTypeIdParameter.Value = keyTypeId;

						valueParameter.Value = _valueSerializer.Serialize(pair.Value, out var valueTypeId);
						if (_valueSerializer.StorePerValueTypeInformation)
							valueTypeIdParameter.Value = valueTypeId;

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
				using (var command = CreateCommand(_deleteQuery))
				{
					command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key, out var typeId));
					if (_keySerializer.StorePerValueTypeInformation)
					{
						command.Parameters.AddWithValue("@keyType", typeId);
					}

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

		private TValue ReadValue(SQLiteDataReader reader)
		{
			return _valueSerializer.Deserialize(reader, _valueOrdinal, _valueTypeOrdinal);
		}

		private void InsertOrReplace(TKey key, TValue value)
		{
			using (var transaction = BeginTransaction())
			{
				using (var command = CreateCommand(_putQuery))
				{
					var serializedKey = _keySerializer.Serialize(key, out var keyTypeId);
					command.Parameters.AddWithValue("@key", serializedKey);
					if (_keySerializer.StorePerValueTypeInformation)
					{
						command.Parameters.AddWithValue("@keyType", keyTypeId);
					}

					var serializedValue = _valueSerializer.Serialize(value, out var valueTypeId);
					command.Parameters.AddWithValue("@value", serializedValue);
					if (_valueSerializer.StorePerValueTypeInformation)
					{
						command.Parameters.AddWithValue("@valueType", valueTypeId);
					}

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
					var key = _keySerializer.Deserialize(reader, 0, _keyTypeOrdinal);
					var value = _valueSerializer.Deserialize(reader, _valueOrdinal, _valueTypeOrdinal);
					ret.Add(new KeyValuePair<TKey, TValue>(key, value));
				}

				return ret;
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

		private void CreateObjectTableIfNecessary(out int keyOrdinal,
		                                          out int keyTypeOrdinal,
		                                          out int valueOrdinal,
		                                          out int valueTypeOrdinal,
		                                          out string table)
		{
			using (var command = _connection.CreateCommand())
			{
				var builder = new StringBuilder();
				builder.AppendFormat("CREATE TABLE IF NOT EXISTS {0} (", _tableName);

				keyOrdinal = 0;
				builder.AppendFormat("key {0} PRIMARY KEY NOT NULL", GetAffinity(_keySerializer.DatabaseType));
				if (_keySerializer.StorePerValueTypeInformation)
				{
					builder.Append(",keyType INTEGER NOT NULL");
					keyTypeOrdinal = 1;
					valueOrdinal = 2;
				}
				else
				{
					keyTypeOrdinal = -1;
					valueOrdinal = 1;
				}

				builder.AppendFormat(",value {0} NOT NULL", GetAffinity(_valueSerializer.DatabaseType));
				if (_valueSerializer.StorePerValueTypeInformation)
				{
					builder.Append(",valueType INTEGER NOT NULL");
					valueTypeOrdinal = valueOrdinal + 1;
				}
				else
				{
					valueTypeOrdinal = -1;
				}

				builder.Append(")");
				command.CommandText = table = builder.ToString();
				command.ExecuteNonQuery();
			}
		}

		private static string PutQuery(string tableName,
		                                           ISQLiteSerializer keySerializer,
		                                           ISQLiteSerializer valueSerializer)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("INSERT OR REPLACE INTO {0} ", tableName);
			builder.Append("(key");
			if (keySerializer.StorePerValueTypeInformation)
				builder.Append(", keyType");
			builder.Append(", value");
			if (valueSerializer.StorePerValueTypeInformation)
				builder.Append(", valueType");
			builder.Append(") VALUES (");
			builder.Append("@key");
			if (keySerializer.StorePerValueTypeInformation)
				builder.Append(", @keyType");
			builder.Append(", @value");
			if (valueSerializer.StorePerValueTypeInformation)
				builder.Append(", @valueType");
			builder.Append(")");
			return builder.ToString();
		}

		[Pure]
		private static string CreateGet(string tableName,
		                                ISQLiteSerializer<TKey> keySerializer,
		                                ISQLiteSerializer<TValue> valueSerializer)
		{
			var builder = new StringBuilder();
			builder.Append("SELECT key");
			if (keySerializer.StorePerValueTypeInformation)
				builder.Append(", keyType");
			builder.Append(", value");
			if (valueSerializer.StorePerValueTypeInformation)
				builder.Append(", valueType");
			builder.AppendFormat(" FROM {0}", tableName);
			builder.Append(" WHERE key = @key");
			return builder.ToString();
		}

		[Pure]
		private static string CreateGetMany(string tableName,
		                                    ISQLiteSerializer<TKey> keySerializer,
		                                    ISQLiteSerializer<TValue> valueSerializer)
		{
			var builder = new StringBuilder();
			builder.Append("SELECT key");
			if (keySerializer.StorePerValueTypeInformation)
				builder.Append(", keyType");
			builder.Append(", value");
			if (valueSerializer.StorePerValueTypeInformation)
				builder.Append(", valueType");
			builder.AppendFormat(" FROM {0}", tableName);
			builder.Append(" WHERE key = @key");
			return builder.ToString();
		}

		[Pure]
		private static string CreateGetAll(string tableName,
		                                   ISQLiteSerializer<TKey> keySerializer,
		                                   ISQLiteSerializer<TValue> valueSerializer)
		{
			var builder = new StringBuilder();
			builder.Append("SELECT key");
			if (keySerializer.StorePerValueTypeInformation)
				builder.Append(", keyType");
			builder.Append(", value");
			if (valueSerializer.StorePerValueTypeInformation)
				builder.Append(", valueType");
			builder.AppendFormat(" FROM {0}", tableName);
			return builder.ToString();
		}

		private static string GetAffinity(DbType databaseType)
		{
			switch (databaseType)
			{
				case DbType.String:
					return "STRING";

				case DbType.UInt16:
				case DbType.Int16:
				case DbType.Int32:
				case DbType.UInt32:
				case DbType.Int64:
					return "INTEGER";

				case DbType.Binary:
					return "BLOB";

				default:
					throw new NotImplementedException(string.Format("Type '{0}' is not implemented", databaseType));
			}
		}
	}
}