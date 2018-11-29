using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	sealed class Queue<T>
		: AbstractCollection<T>
		, IQueue<T>
	{
		private readonly SQLiteConnection _connection;
		private readonly ISQLiteSerializer<T> _serializer;
		private readonly string _put;
		private readonly string _tryPeek;
		private readonly string _remove;

		public Queue(SQLiteConnection connection,
		             string name,
		             string tableName,
		             ISQLiteSerializer<T> serializer,
		             bool isReadOnly)
			: base(connection, name, tableName, serializer, isReadOnly)
		{
			_connection = connection;
			_serializer = serializer;

			CreateTableIfNecessary(connection, serializer, tableName);

			_put = string.Format("INSERT INTO {0} (value) VALUES (@value)", tableName);
			_tryPeek = string.Format("SELECT id, value FROM {0} LIMIT 0, 1", tableName);
			_remove = string.Format("DELETE FROM {0} WHERE id = @id", tableName);
		}

		#region Overrides of AbstractCollection

		public override CollectionType Type => CollectionType.Queue;

		public override Type KeyType => null;

		public override string KeyTypeName => null;

		#endregion

		#region Implementation of IQueue<T>

		public void Enqueue(T value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _put;
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				command.ExecuteNonQuery();
			}
		}

		public void EnqueueMany(IEnumerable<T> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _put;
				var valueParameter = command.Parameters.Add("@value", _serializer.DatabaseType);

				foreach (var value in values)
				{
					valueParameter.Value = _serializer.Serialize(value);
					command.ExecuteNonQuery();
				}
				
				transaction.Commit();
			}
		}

		public bool TryDequeue(out T value)
		{
			if (!TryPeek(out var rowId, out value))
				return false;

			Remove(rowId);
			return true;
		}

		public bool TryPeek(out T value)
		{
			return TryPeek(out var unused, out value);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("Queue<{0}>(\"{1}\")", ValueType.FullName, Name);
		}

		#endregion

		#endregion

		private void Remove(RowId rowId)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _remove;
				command.Parameters.AddWithValue("id", rowId.Id);
				command.ExecuteNonQuery();
			}
		}

		private bool TryPeek(out RowId rowId, out T value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _tryPeek;
				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
						value = default(T);
						rowId = new RowId();
						return false;
					}

					rowId = new RowId(reader.GetInt64(0));
					return _serializer.TryDeserialize(reader, 1, out value);
				}
			}
		}

		private static void CreateTableIfNecessary(SQLiteConnection connection,
		                                           ISQLiteSerializer serializer,
		                                           string tableName)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText =
					string.Format("CREATE TABLE IF NOT EXISTS {0} (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, value {1} NOT NULL)",
					              tableName,
					              SQLiteHelper.GetAffinity(serializer.DatabaseType));

				command.ExecuteNonQuery();
			}
		}
	}
}
