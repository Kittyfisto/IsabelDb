using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class Bag<T>
		: AbstractCollection<T>
		, IBag<T>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _put;
		private readonly ISQLiteSerializer<T> _serializer;
		private readonly string _table;
		private readonly string _tableName;

		public Bag(SQLiteConnection connection,
		           string name,
		           string tableName,
		           ISQLiteSerializer<T> serializer,
		           bool isReadOnly)
			: base(connection, name, tableName, serializer, isReadOnly)
		{
			_connection = connection;
			_serializer = serializer;
			_tableName = tableName;

			CreateTableIfNecessary(connection, serializer, tableName, out _table);

			_put = string.Format("INSERT INTO {0} (value) VALUES (@value)", tableName);
		}

		#region Implementation of IInternalObjectStore

		public override CollectionType Type => CollectionType.Bag;

		public override Type KeyType => null;

		public override string KeyTypeName => null;

		#endregion

		#region Overrides of Object

		public override string ToString()
		{
			return _table;
		}

		#endregion

		private SQLiteCommand CreateCommand(string text)
		{
			var command = _connection.CreateCommand();
			command.CommandText = text;
			return command;
		}

		private static void CreateTableIfNecessary(SQLiteConnection connection,
		                                           ISQLiteSerializer serializer,
		                                           string tableName,
		                                           out string table)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = table =
					string.Format("CREATE TABLE IF NOT EXISTS {0} (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, value {1} NOT NULL)",
					              tableName,
					              SQLiteHelper.GetAffinity(serializer.DatabaseType));

				command.ExecuteNonQuery();
			}
		}

		#region Implementation of IListObjectStore<T>

		public RowId Put(T value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _put;
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				command.ExecuteNonQuery();
				return new RowId(_connection.LastInsertRowId);
			}
		}

		public IEnumerable<RowId> PutMany(IEnumerable<T> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var transaction = _connection.BeginTransaction())
			using (var command = CreateCommand(_put))
			{
				var valueParameter = command.Parameters.Add("@value", _serializer.DatabaseType);

				var ret = new List<RowId>();
				foreach (var value in values)
				{
					valueParameter.Value = _serializer.Serialize(value);
					command.ExecuteNonQuery();
					ret.Add(new RowId(_connection.LastInsertRowId));
				}

				transaction.Commit();
				return ret;
			}
		}

		public T GetValue(RowId key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Id);
				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
						throw new KeyNotFoundException();

					if (!_serializer.TryDeserialize(reader, 0, out var value))
						throw new NotImplementedException();

					return value;
				}
			}
		}

		public IEnumerable<KeyValuePair<RowId, T>> GetAll()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT rowid, value FROM {0}", _tableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var rowId = new RowId(reader.GetInt64(0));

						if (_serializer.TryDeserialize(reader, 1, out var value))
							yield return new KeyValuePair<RowId, T>(rowId, value);
					}
				}
			}
		}

		public bool TryGetValue(RowId key, out T value)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Id);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						if (_serializer.TryDeserialize(reader, 0, out value))
						return true;
					}
				}

				value = default(T);
				return false;
			}
		}

		public IEnumerable<T> GetValues(Interval<RowId> interval)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id >= @minimum AND id <= @maximum", _tableName);
				command.Parameters.AddWithValue("@minimum", interval.Minimum.Id);
				command.Parameters.AddWithValue("@maximum", interval.Maximum.Id);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_serializer.TryDeserialize(reader, 0, out var value))
							yield return value;
					}
				}
			}
		}

		public IEnumerable<T> GetManyValues(IEnumerable<RowId> keys)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				var idParameter = command.Parameters.Add("@id", DbType.Int64);

				foreach (var key in keys)
				{
					idParameter.Value = key.Id;
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (_serializer.TryDeserialize(reader, 0, out var value))
								yield return value;
						}
					}
				}
			}
		}

		public void Remove(RowId key)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Id);
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}