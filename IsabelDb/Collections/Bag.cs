using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
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
		private long _lastId;

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

		public ValueKey Put(T value)
		{
			ThrowIfReadOnly();

			using (var command = CreateCommand(_put))
			{
				var id = Interlocked.Increment(ref _lastId);
				command.Parameters.AddWithValue("@id", id);
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				command.ExecuteNonQuery();
				return new ValueKey(id);
			}
		}

		public IEnumerable<ValueKey> PutMany(IEnumerable<T> values)
		{
			ThrowIfReadOnly();

			using (var transaction = _connection.BeginTransaction())
			using (var command = CreateCommand(_put))
			{
				var idParameter = command.Parameters.Add("@id", DbType.Int64);
				var valueParameter = command.Parameters.Add("@value", _serializer.DatabaseType);

				var ret = new List<ValueKey>();
				foreach (var value in values)
				{
					var id = Interlocked.Increment(ref _lastId);
					idParameter.Value = id;
					valueParameter.Value = _serializer.Serialize(value);
					command.ExecuteNonQuery();

					ret.Add(new ValueKey(id));
				}

				transaction.Commit();
				return ret;
			}
		}

		public T GetValue(ValueKey key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Value);
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

		public bool TryGetValue(ValueKey key, out T value)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Value);
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

		public IEnumerable<T> GetValues(Interval<ValueKey> interval)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id >= @minimum AND id <= @maximum", _tableName);
				command.Parameters.AddWithValue("@minimum", interval.Minimum.Value);
				command.Parameters.AddWithValue("@maximum", interval.Maximum.Value);

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

		public IEnumerable<T> GetManyValues(IEnumerable<ValueKey> keys)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE id = @id", _tableName);
				var idParameter = command.Parameters.Add("@id", DbType.Int64);

				foreach (var key in keys)
				{
					idParameter.Value = key.Value;
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

		public void Remove(ValueKey key)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", key.Value);
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}