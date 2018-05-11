using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class IntervalCollection<T, TValue>
		: AbstractCollection<TValue>
		, IIntervalCollection<T, TValue>
		, IInternalCollection
		where T : IComparable<T>
	{
		private static readonly IReadOnlyList<Type> SupportedKeys;

		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<T> _keySerializer;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;
		private long _lastId;

		static IntervalCollection()
		{
			SupportedKeys = new List<Type>
			{
				typeof(byte),
				typeof(sbyte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double)
			};
		}

		public IntervalCollection(SQLiteConnection connection,
		                          string tableName,
		                          ISQLiteSerializer<T> keySerializer,
		                          ISQLiteSerializer<TValue> valueSerializer,
		                          bool isReadOnly)
			: base(connection, tableName, valueSerializer, isReadOnly)
		{
			_connection = connection;
			_tableName = tableName;
			_keySerializer = keySerializer;
			_valueSerializer = valueSerializer;

			CreateObjectTableIfNecessary();

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

		#region Implementation of IInternalCollection

		public Type ValueType => throw new NotImplementedException();

		#endregion

		#region Implementation of IIntervalCollection<T,TValue>

		public void Put(Interval<T> interval, TValue value)
		{
			ThrowIfReadOnly();

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
			}
		}

		public void PutMany(IEnumerable<KeyValuePair<Interval<T>, TValue>> values)
		{
			ThrowIfReadOnly();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (id, minimum, maximum, value) VALUES (@id, @minimum, @maximum, @value)",
				                                    _tableName);
				var idParameter = command.Parameters.Add("@id", DbType.Int64);
				var minimumParameter = command.Parameters.Add("@minimum", _keySerializer.DatabaseType);
				var maximumParameter = command.Parameters.Add("@maximum", _keySerializer.DatabaseType);
				var valueParameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);

				var ret = new List<ValueKey>();
				foreach (var pair in values)
				{
					var interval = pair.Key;
					var value = pair.Value;

					var id = Interlocked.Increment(ref _lastId);
					idParameter.Value = id;
					minimumParameter.Value = _keySerializer.Serialize(interval.Minimum);
					maximumParameter.Value = _keySerializer.Serialize(interval.Maximum);
					valueParameter.Value = _valueSerializer.Serialize(value);
					command.ExecuteNonQuery();

					ret.Add(new ValueKey(id));
				}

				transaction.Commit();
			}
		}

		public IEnumerable<Interval<T>> GetManyIntervals(IEnumerable<ValueKey> keys)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TValue> GetValues(T key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE minimum <= @key AND maximum >= @key",
													_tableName);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
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

		public void Remove(T key)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE minimum <= @key AND maximum >= @key", _tableName);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.ExecuteNonQuery();
			}
		}

		public void Remove(Interval<T> interval)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE NOT (@maximum < minimum OR @minimum > maximum)", _tableName);
				command.Parameters.AddWithValue("@minimum", _keySerializer.Serialize(interval.Minimum));
				command.Parameters.AddWithValue("@maximum", _keySerializer.Serialize(interval.Maximum));
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
				builder.AppendFormat("CREATE INDEX IF NOT EXISTS {0}_minimum on {0}(minimum);", _tableName);
				command.CommandText = builder.ToString();
				command.ExecuteNonQuery();
			}
		}

		public static void ThrowIfInvalidKey()
		{
			var key = typeof(T);
			if (!SupportedKeys.Contains(key))
				throw new NotSupportedException(
					string.Format(
						"The type '{0}' may not be used as a key in an interval collection! Only basic numeric types can be used for now.",
						key.FullName));
		}
	}
}