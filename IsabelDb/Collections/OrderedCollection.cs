using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class OrderedCollection<TKey, TValue>
		: AbstractCollection<TValue>
		, IOrderedCollection<TKey, TValue>
		where TKey : IComparable<TKey>
	{
		private readonly SQLiteConnection _connection;
		private readonly ISQLiteSerializer<TKey> _keySerializer;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;

		private static readonly IReadOnlyList<Type> SupportedKeyTypes;

		static OrderedCollection()
		{
			SupportedKeyTypes = new List<Type>
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

		public OrderedCollection(SQLiteConnection connection,
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
		}

		#region Implementation of IOrderedCollection

		public void Put(TKey key, TValue value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (key, value) VALUES (@key, @value)",
				                                    _tableName);
				command.Parameters.AddWithValue("@key", _keySerializer.Serialize(key));
				command.Parameters.AddWithValue("@value", _valueSerializer.Serialize(value));

				command.ExecuteNonQuery();
			}
		}

		public void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (key, value) VALUES (@key, @value)",
				                                    _tableName);
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

		public IEnumerable<TValue> GetValues(Interval<TKey> interval)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE key >= @minimum AND key <= @maximum", _tableName);
				command.Parameters.AddWithValue("@minimum", _keySerializer.Serialize(interval.Minimum));
				command.Parameters.AddWithValue("@maximum", _keySerializer.Serialize(interval.Maximum));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 0, out var value))
							yield return value;
					}
				}
			}
		}

		public void RemoveRange(Interval<TKey> interval)
		{
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE key >= @minimum AND key <= @maximum", _tableName);
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
				builder.AppendFormat("key {0} PRIMARY KEY NOT NULL, ", SQLiteHelper.GetAffinity(_keySerializer.DatabaseType));
				builder.AppendFormat("value {0} NOT NULL", SQLiteHelper.GetAffinity(_valueSerializer.DatabaseType));
				builder.Append(")");
				command.CommandText = builder.ToString();
				command.ExecuteNonQuery();
			}
		}

		public static void ThrowIfUnsupportedKeyType()
		{
			var type = typeof(TKey);
			if (!SupportedKeyTypes.Contains(type))
				throw new NotSupportedException(string.Format("The type '{0}' may not be used as a key in an ordered collection! Only basic numeric types can be used for now.",
					type.FullName));
		}

		#region Overrides of AbstractCollection<TValue>

		public override CollectionType Type => CollectionType.OrderedCollection;

		public override Type KeyType => typeof(TKey);

		#endregion
	}
}