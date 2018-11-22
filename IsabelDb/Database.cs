using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using IsabelDb.TypeModels;

namespace IsabelDb
{
	/// <summary>
	///     Factory to create an <see cref="IDatabase" />.
	/// </summary>
	public static class Database
	{
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
			var connection = new SQLiteConnection("Data Source=:memory:");
			try
			{
				connection.Open();
				CreateTables(connection);
				return new IsabelDb(connection, supportedTypes, true, false);
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
			if (!File.Exists(databasePath)) SQLiteConnection.CreateFile(databasePath);

			var connectionString = CreateConnectionString(databasePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				CreateTablesIfNecessary(connection);
				return new IsabelDb(connection, supportedTypes, true, false);
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
				EnsureTableSchema(connection);
				return new IsabelDb(connection, supportedTypes, true, isReadOnly);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		private static void CreateTablesIfNecessary(SQLiteConnection connection)
		{
			var hasTypesTable = TypeModel.DoesTableExist(connection);
			var hasStoresTable = ObjectStores.DoesTableExist(connection);
			if (hasTypesTable && hasStoresTable)
				return;
			if (hasTypesTable != hasStoresTable)
				throw new NotImplementedException("Something something incompatible");

			CreateTables(connection);
		}

		internal static void CreateTables(SQLiteConnection connection)
		{
			TypeModel.CreateTable(connection);
			ObjectStores.CreateTable(connection);
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
			if (!TypeModel.DoesTableExist(connection))
				throw new NotImplementedException();
			if (!ObjectStores.DoesTableExist(connection))
				throw new NotImplementedException();
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