﻿using IsabelDb.TypeModels;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	public sealed class Database
		: IDisposable
	{
		private readonly SQLiteConnection _connection;
		private readonly bool _disposeConnection;
		private readonly ObjectStores _objectStores;

		internal Database(SQLiteConnection connection, IEnumerable<Type> supportedTypes,
		                  bool disposeConnection = true)
		{
			_connection = connection;
			_disposeConnection = disposeConnection;

			_objectStores = new ObjectStores(connection, supportedTypes.ToList());
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposeConnection)
			{
				_connection.Close();
				_connection.Dispose();
			}
		}

		/// <summary>
		///     Returns an object store in which each object is identified by a key of the given type
		///     <typeparamref name="TKey" />.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			return _objectStores.GetDictionary<TKey, TValue>(name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name)
		{
			return _objectStores.GetMultiValueDictionary<TKey, TValue>(name);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IBag<T> GetBag<T>(string name)
		{
			return _objectStores.GetBag<T>(name);
		}

		/// <summary>
		///     Creates a new database which is backed by memory.
		/// </summary>
		/// <remarks>
		///     This is probably only useful for tests.
		/// </remarks>
		/// <param name="supportedTypes">The list of custom types which are to be stored in / retrieved from the database</param>
		/// <returns></returns>
		public static Database CreateInMemory(IEnumerable<Type> supportedTypes)
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			try
			{
				connection.Open();
				CreateTables(connection);
				return new Database(connection, supportedTypes);
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
		public static Database OpenOrCreate(string databasePath, IEnumerable<Type> supportedTypes)
		{
			if (!File.Exists(databasePath)) SQLiteConnection.CreateFile(databasePath);

			var connectionString = CreateConnectionString(databasePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				CreateTablesIfNecessary(connection);
				return new Database(connection, supportedTypes);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		/// <summary>
		///     Opens an existing database at the given path.
		/// </summary>
		/// <param name="databaseFilePath"></param>
		/// <param name="supportedTypes">The list of custom types which are to be stored in / retrieved from the database</param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static Database Open(string databaseFilePath, IEnumerable<Type> supportedTypes)
		{
			if (!File.Exists(databaseFilePath))
				throw new FileNotFoundException("Unable to open the given database", databaseFilePath);

			var connectionString = CreateConnectionString(databaseFilePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				EnsureTableSchema(connection);
				return new Database(connection, supportedTypes);
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

		private static void EnsureTableSchema(SQLiteConnection connection)
		{
			if (!TypeModel.DoesTableExist(connection))
				throw new NotImplementedException();
			if (!ObjectStores.DoesTableExist(connection))
				throw new NotImplementedException();
		}

		private static string CreateConnectionString(string databaseIsdb)
		{
			var builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = databaseIsdb;
			builder.JournalMode = SQLiteJournalModeEnum.Wal;
			var connectionString = builder.ToString();
			return connectionString;
		}
	}
}