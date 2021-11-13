using System;
using System.Data.SQLite;

namespace IsabelDb.Test.Collections
{
	public abstract class AbstractTest
	{
		protected SQLiteConnection CreateConnection()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			Database.CreateTables(connection);
			return connection;
		}

		protected IDatabase CreateDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, null, types, disposeConnection: false, isReadOnly: false);
		}

		protected IReadOnlyDatabase CreateReadOnlyDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, null, types, disposeConnection: false, isReadOnly: true);
		}
	}
}