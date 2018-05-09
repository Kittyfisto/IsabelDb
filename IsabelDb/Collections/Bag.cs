using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class Bag<T>
		: AbstractCollection<T>
		, IBag<T>
		, IInternalCollection
	{
		private readonly SQLiteConnection _connection;
		private readonly string _put;
		private readonly ISQLiteSerializer<T> _serializer;
		private readonly string _table;

		public Bag(SQLiteConnection connection,
		                      string tableName,
		                      ISQLiteSerializer<T> serializer)
			: base(connection, tableName, serializer)
		{
			_connection = connection;
			_serializer = serializer;
			CreateTableIfNecessary(connection, serializer, tableName, out _table);

			_put = string.Format("INSERT INTO {0} (value) VALUES (@value)", tableName);
		}

		#region Implementation of IInternalObjectStore

		public Type ValueType => typeof(T);

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

		public void Put(T value)
		{
			using (var command = CreateCommand(_put))
			{
				command.Parameters.AddWithValue("@value", _serializer.Serialize(value));
				command.ExecuteNonQuery();
			}
		}

		public void PutMany(IEnumerable<T> values)
		{
			using (var transaction = _connection.BeginTransaction())
			using (var command = CreateCommand(_put))
			{
				var parameter = command.Parameters.Add("@value", _serializer.DatabaseType);

				foreach (var value in values)
				{
					parameter.Value = _serializer.Serialize(value);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		public void PutMany(params T[] values)
		{
			PutMany((IEnumerable<T>) values);
		}

		#endregion
	}
}