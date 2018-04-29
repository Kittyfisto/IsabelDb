using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Stores
{
	internal sealed class BagObjectStore<T>
		: IBagObjectStore<T>
			, IInternalObjectStore
	{
		private readonly SQLiteConnection _connection;
		private readonly string _put;
		private readonly ISQLiteSerializer<T> _serializer;
		private readonly string _table;
		private readonly string _count;
		private readonly string _getAll;
		private readonly string _clear;

		public BagObjectStore(SQLiteConnection connection,
		                      ISQLiteSerializer<T> serializer,
		                      string tableName)
		{
			_connection = connection;
			_serializer = serializer;
			CreateTableIfNecessary(connection, serializer, tableName, out _table);

			_clear = string.Format("DELETE FROM {0}", tableName);
			_getAll = string.Format("SELECT value FROM {0}", tableName);
			_put = string.Format("INSERT INTO {0} (value) VALUES (@value)", tableName);
			_count = string.Format("SELECT COUNT(*) FROM {0}", tableName);
		}

		#region Implementation of IInternalObjectStore

		public Type ObjectType => throw new NotImplementedException();

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

		#region Implementation of IListObjectStore<T>

		public IEnumerable<T> GetAll()
		{
			using (var command = CreateCommand(_getAll))
			{
				var ret = new List<T>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var value = _serializer.Deserialize(reader, 0);
						ret.Add(value);
					}
				}
				return ret;
			}
		}

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

		public void Clear()
		{
			using (var command = CreateCommand(_clear))
			{
				command.ExecuteNonQuery();
			}
		}

		public int Count()
		{
			using (var command = CreateCommand(_count))
			{
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		#endregion

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
	}
}