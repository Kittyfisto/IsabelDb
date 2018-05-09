using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class IntervalCollection<T, TValue>
		: IIntervalCollection<T, TValue>
		, IInternalCollection
		where T : IComparable<T>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<T> _keySerializer;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;
		private long _lastId;

		public IntervalCollection(SQLiteConnection connection,
		                          string tableName,
		                          ISQLiteSerializer<T> keySerializer,
		                          ISQLiteSerializer<TValue> valueSerializer)
		{
			_connection = connection;
			_tableName = tableName;
			_keySerializer = keySerializer;
			_valueSerializer = valueSerializer;

			CreateObjectTableIfNecessary();

			_lastId = 0;
		}

		#region Implementation of IInternalCollection

		public Type ValueType => throw new NotImplementedException();

		#endregion

		#region Implementation of ICollection

		public void Clear()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0}", _tableName);
				command.ExecuteNonQuery();
			}
		}

		public int Count()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation of IIntervalCollection<T,TValue>

		public ValueKey Put(Interval<T> interval, TValue value)
		{
			using (var command = _connection.CreateCommand())
			{
				var id = Interlocked.Increment(ref _lastId);
				command.CommandText = string.Format("INSERT INTO {0} (id, minimum, maximum, value) VALUES (@id, @minimum, @maximum, @value)",
				                                    _tableName);
				command.Parameters.AddWithValue("@id", id);
				command.Parameters.AddWithValue("@minimum", _keySerializer.Serialize(interval.Minimum));
				command.Parameters.AddWithValue("@maximum", _keySerializer.Serialize(interval.Maximum));
				command.Parameters.AddWithValue("@value", _valueSerializer.Serialize(value));

				command.ExecuteNonQuery();
				return new ValueKey(id);
			}
		}

		public IEnumerable<TValue> GetValues(T key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE minimum <= @value AND maximum >= @value",
				                                    _tableName);
				command.Parameters.AddWithValue("@value", _keySerializer.Serialize(key));
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

		public IEnumerable<TValue> GetValues(T minimum, T maximum)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE NOT (@maximum < minimum OR @minimum > maximum)",
				                                    _tableName);
				command.Parameters.AddWithValue("@minimum", _keySerializer.Serialize(minimum));
				command.Parameters.AddWithValue("@maximum", _keySerializer.Serialize(maximum));

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

		public IEnumerable<KeyValuePair<Interval<T>, TValue>> GetAll()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT minimum, maximum, value FROM {0}", _tableName);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_keySerializer.TryDeserialize(reader, 0, out var minimum) &&
						    _keySerializer.TryDeserialize(reader, 1, out var maximum) &&
						    _valueSerializer.TryDeserialize(reader, 2, out var value))
						{
							var interval = Interval.Create(minimum, maximum);
							yield return new KeyValuePair<Interval<T>, TValue>(interval, value);
						}
					}
				}
			}
		}

		public IEnumerable<TValue> GetAllValues()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0}", _tableName);

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

		public void Remove(T key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE minimum <= @key AND maximum >= @key", _tableName);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.ExecuteNonQuery();
			}
		}

		public void Remove(Interval<T> interval)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE NOT (@maximum < minimum OR @minimum > maximum)", _tableName);
				command.Parameters.AddWithValue("@minimum", _keySerializer.Serialize(interval.Minimum));
				command.Parameters.AddWithValue("@maximum", _keySerializer.Serialize(interval.Maximum));
				command.ExecuteNonQuery();
			}
		}

		public void Remove(ValueKey valueKey)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE id = @id", _tableName);
				command.Parameters.AddWithValue("@id", valueKey.Value);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				var builder = new StringBuilder();
				builder.AppendFormat("CREATE TABLE IF NOT EXISTS {0} (", _tableName);

				builder.AppendFormat("id INTEGER PRIMARY KEY NOT NULL, ");
				builder.AppendFormat("minimum {0} NOT NULL, ", SQLiteHelper.GetAffinity(_keySerializer.DatabaseType));
				builder.AppendFormat("maximum {0} NOT NULL, ", SQLiteHelper.GetAffinity(_keySerializer.DatabaseType));
				builder.AppendFormat("value {0} NOT NULL", SQLiteHelper.GetAffinity(_valueSerializer.DatabaseType));
				builder.Append(");");
				builder.AppendFormat("CREATE INDEX IF NOT EXISTS min on {0}(minimum)", _tableName);
				command.CommandText = builder.ToString();
				command.ExecuteNonQuery();
			}
		}
	}
}