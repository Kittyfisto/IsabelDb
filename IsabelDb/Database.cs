using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using IsabelDb.TypeModels;
using log4net;

namespace IsabelDb
{
	/// <summary>
	///     Factory to create an <see cref="IDatabase" />.
	/// </summary>
	public static class Database
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     Creates a new database which is backed by memory.
		/// </summary>
		/// <remarks>
		///     This is probably only useful when writing tests to not thrash the disk.
		/// </remarks>
		/// <param name="supportedTypes">The list of custom types which are to be stored in / retrieved from the database</param>
		/// <returns></returns>
		public static IDatabase CreateInMemory(IEnumerable<Type> supportedTypes)
		{
			Log.DebugFormat("Creating in memory database...");

			var connection = new SQLiteConnection("Data Source=:memory:");
			try
			{
				connection.Open();
				CreateTables(connection);
				return new IsabelDb(connection, null, supportedTypes, true, false);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		/// <summary>
		///     Opens an existing or creates a new database at the given file path.
		/// </summary>
		/// <param name="databasePath"></param>
		/// <param name="supportedTypes">The list of custom types which are to be stored in / retrieved from the database</param>
		/// <returns></returns>
		public static IDatabase OpenOrCreate(string databasePath, IEnumerable<Type> supportedTypes)
		{
			if (!File.Exists(databasePath))
			{
				Log.DebugFormat("Creating new database '{0}'...", databasePath);

				SQLiteConnection.CreateFile(databasePath);
			}
			else
			{
				Log.DebugFormat("Opening database '{0}'...", databasePath);
			}

			var connectionString = CreateConnectionString(databasePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				CreateTablesIfNecessary(connection);
				var database = new IsabelDb(connection, databasePath, supportedTypes, true, false);

				Log.DebugFormat("Successfully openeing database '{0}'!", databasePath);

				return database;
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		/// <summary>
		///     Opens an existing database at the given path.
		///     The database can both be read from and written to.
		/// </summary>
		/// <param name="databaseFilePath"></param>
		/// <param name="supportedTypes">The list of custom types which are to be stored in / retrieved from the database</param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static IDatabase Open(string databaseFilePath, IEnumerable<Type> supportedTypes)
		{
			return Open(databaseFilePath, supportedTypes, false);
		}

		/// <summary>
		///     Opens an existing database at the given path **for reading only**.
		/// </summary>
		/// <param name="databaseFilePath"></param>
		/// <param name="supportedTypes"></param>
		public static IReadOnlyDatabase OpenRead(string databaseFilePath, IEnumerable<Type> supportedTypes)
		{
			return Open(databaseFilePath, supportedTypes, true);
		}

		private static IDatabase Open(string databaseFilePath, IEnumerable<Type> supportedTypes, bool isReadOnly)
		{
			if (!File.Exists(databaseFilePath))
				throw new FileNotFoundException("Unable to open the given database", databaseFilePath);

			var connectionString = CreateConnectionString(databaseFilePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				return Open(connection, databaseFilePath, supportedTypes, isReadOnly);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		internal static IDatabase Open(SQLiteConnection connection, string databaseFilePath, IEnumerable<Type> supportedTypes, bool isReadOnly)
		{
			EnsureTableSchema(connection);
			return new IsabelDb(connection, databaseFilePath, supportedTypes, true, isReadOnly);
		}

		private static void CreateTablesIfNecessary(SQLiteConnection connection)
		{
			var hasTypesTable = TypeModel.DoesTypeTableExist(connection);
			var hasStoresTable = CollectionsTable.DoesTableExist(connection);
			if (hasTypesTable && hasStoresTable)
				return;
			if (hasTypesTable != hasStoresTable)
				throw new NotImplementedException("Something something incompatible");

			CreateTables(connection);
		}

		internal static void CreateTables(SQLiteConnection connection)
		{
			VariablesTable.CreateTable(connection);

			TypeModel.CreateTable(connection);
			CollectionsTable.CreateTable(connection);
		}

		internal static bool TableExists(SQLiteConnection connection, string tableName)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText =
					string.Format("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{0}' LIMIT 0,1", tableName);
				var ret = command.ExecuteScalar();
				return Convert.ToInt32(ret) == 1;
			}
		}

		internal static void EnsureTableSchema(SQLiteConnection connection)
		{
			if (!VariablesTable.DoesTableExist(connection))
				throw new IncompatibleDatabaseSchemaException(string.Format("The database is missing the '{0}' table. It may have been created with an early vesion of IsabelDb or it may not even be an IsabelDb file. Are you sure the path is correct?", VariablesTable.TableName));
			if (!TypeModel.DoesTypeTableExist(connection))
				throw new IncompatibleDatabaseSchemaException(string.Format("The database is missing the '{0}' table. The table may have been deleted or this may not even be an IsabelDb file. Are you sure the path is correct?", TypeModel.TypeTableName));
			if (!CollectionsTable.DoesTableExist(connection))
				throw new IncompatibleDatabaseSchemaException(string.Format("The database is missing the '{0}' table. The table may have been deleted or this may not even be an IsabelDb file. Are you sure the path is correct?", CollectionsTable.TableName));
		}

		internal static string CreateConnectionString(string databaseIsdb)
		{
			var builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = databaseIsdb;
			var connectionString = builder.ToString();
			return connectionString;
		}
	}
}