using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	public sealed class IsabelDb
		: IDisposable
	{
		private readonly SQLiteConnection _connection;
		private readonly Dictionary<string, IObjectStore> _objectStores;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

		private IsabelDb(SQLiteConnection connection)
		{
			_connection = connection;
			_typeModel = TypeModel.Create();

			_objectStores = new Dictionary<string, IObjectStore>();

			var typeResolver = new TypeResolver();
			_typeStore = new TypeStore(connection, typeResolver);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_connection.Close();
			_connection.Dispose();
		}

		/// <summary>
		///     Returns an object store in which each object is identified by a <see cref="string"/> key.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IDictionaryObjectStore<T> GetDictionary<T>(string name)
		{
			if (!_objectStores.TryGetValue(name, out var store))
			{
				store = new DictionaryObjectStore<T>(_connection, _typeModel, _typeStore, name);
				_objectStores.Add(name, store);
			}

			return (IDictionaryObjectStore<T>) store;
		}

		/// <summary>
		///     Returns an object store in which each object is identified by a <see cref="string"/> key.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IDictionaryObjectStore<object> GetDictionary(string name)
		{
			if (!_objectStores.TryGetValue(name, out var store))
			{
				store = new DictionaryObjectStore<object>(_connection, _typeModel, _typeStore, name);
				_objectStores.Add(name, store);
			}

			return (IDictionaryObjectStore<object>) store;
		}

		/// <summary>
		///     Creates a new database which is backed by memory.
		/// </summary>
		/// <remarks>
		///     This is probably only useful for tests.
		/// </remarks>
		/// <returns></returns>
		public static IsabelDb CreateInMemory()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			try
			{
				connection.Open();
				CreateTables(connection);
				return new IsabelDb(connection);
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
		/// <returns></returns>
		public static IsabelDb OpenOrCreate(string databasePath)
		{
			if (!File.Exists(databasePath)) SQLiteConnection.CreateFile(databasePath);

			var connectionString = CreateConnectionString(databasePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				CreateTablesIfNecessary(connection);
				return new IsabelDb(connection);
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
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static IsabelDb Open(string databaseFilePath)
		{
			if (!File.Exists(databaseFilePath))
				throw new FileNotFoundException("Unable to open the given database", databaseFilePath);

			var connectionString = CreateConnectionString(databaseFilePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				EnsureTableSchema(connection);
				return new IsabelDb(connection);
			}
			catch (Exception)
			{
				connection.Dispose();
				throw;
			}
		}

		private static void CreateTablesIfNecessary(SQLiteConnection connection)
		{
			var hasTypesTable = TypeStore.DoesTableExist(connection);
			var hasStoresTable = Stores.DoesTableExist(connection);
			if (hasTypesTable && hasStoresTable)
				return;
			if(hasTypesTable != hasStoresTable)
				throw new NotImplementedException("Something something incompatible");

			CreateTables(connection);
		}

		private static void CreateTables(SQLiteConnection connection)
		{
			TypeStore.CreateTable(connection);
			Stores.CreateTable(connection);
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
			if (!TypeStore.DoesTableExist(connection))
				throw new NotImplementedException();
			if (!Stores.DoesTableExist(connection))
				throw new NotImplementedException();
		}

		private static string CreateConnectionString(string databaseIsdb)
		{
			var builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = databaseIsdb;
			var connectionString = builder.ToString();
			return connectionString;
		}
	}
}