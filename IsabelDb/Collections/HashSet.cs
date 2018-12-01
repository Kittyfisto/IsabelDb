using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class HashSet<T>
		: AbstractCollection<T>
		, IHashSet<T>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<T> _serializer;

		public HashSet(SQLiteConnection connection, string name, string tableName, ISQLiteSerializer<T> serializer, bool isReadOnly)
			: base(connection, name, tableName, serializer, isReadOnly)
		{
			_connection = connection;
			_tableName = tableName;
			_serializer = serializer;

			CreateTableIfNecessary(connection, serializer, tableName);
		}
		
		public override string ToString()
		{
			return string.Format("HashSet<{0}>(\"{1}\")", ValueType.FullName, Name);
		}

		#region Overrides of AbstractCollection

		public override CollectionType Type => CollectionType.HashSet;

		public override Type KeyType => null;

		public override string KeyTypeName => null;

		#endregion

		#region Implementation of IHashSet<T>

		public bool Add(T value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT OR IGNORE INTO {0} (value) VALUES (@value)", _tableName);
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				int numRowsAffected = command.ExecuteNonQuery();
				return numRowsAffected > 0;
			}
		}

		public void AddMany(IEnumerable<T> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (values == null)
				throw new ArgumentNullException(nameof(values));

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT OR IGNORE INTO {0} (value) VALUES (@value)", _tableName);
				var valueParameter = command.Parameters.Add("@value", _serializer.DatabaseType);
				foreach (var value in values)
				{
					if (value == null)
						throw new ArgumentException("Null values cannot be added to this collection");

					valueParameter.Value = _serializer.Serialize(value);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		public bool Contains(T value)
		{
			ThrowIfDropped();

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT(*) FROM {0} WHERE value = @value", _tableName);
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				var count = Convert.ToInt64(command.ExecuteScalar());
				return count > 0;
			}
		}

		public bool Remove(T value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE value = @value", _tableName);
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				var numRowsRemoved = Convert.ToInt64(command.ExecuteNonQuery());
				return numRowsRemoved > 0;
			}
		}

		#endregion

		private void CreateTableIfNecessary(SQLiteConnection connection, ISQLiteSerializer<T> serializer, string tableName)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText =
					string.Format("CREATE TABLE IF NOT EXISTS {0} (value {1} PRIMARY KEY NOT NULL)",
					              tableName,
					              SQLiteHelper.GetAffinity(serializer.DatabaseType));

				command.ExecuteNonQuery();
			}
		}
	}
}
