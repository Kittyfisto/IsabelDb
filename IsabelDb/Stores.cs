using System.Collections.Generic;
using System.Data.SQLite;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for providing access to various (user) created stores.
	///     Ensures type safety when opening a database again, etc...
	/// </summary>
	internal sealed class Stores
	{
		private const string TableName = "isabel_stores";

		private readonly Dictionary<string, IObjectStore> _dictionaryStores;
		private SQLiteConnection _connection;

		public Stores(SQLiteConnection connection)
		{
			_connection = connection;
			_dictionaryStores = new Dictionary<string, IObjectStore>();
		}

		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return IsabelDb.TableExists(connection, TableName);
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "name STRING NOT NULL," +
				                                    "typeId INTEGER NOT NULL" +
				                                    ")", TableName);

				command.ExecuteNonQuery();
			}
		}
	}
}