using System;
using System.Data.SQLite;
using System.IO;
using IsabelDb.Stores;
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
		private readonly ObjectStores _objectStores;

		private IsabelDb(SQLiteConnection connection, ITypeResolver typeResolver)
		{
			_connection = connection;
			TypeModel typeModel = TypeModel.Create();

			var typeStore = new TypeStore(connection, typeResolver ?? new TypeResolver());
			_objectStores = new ObjectStores(connection, typeModel, typeStore);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_connection.Close();
			_connection.Dispose();
		}

		/// <summary>
		///     Returns an object store in which each object is identified by a key of the given type
		///     <typeparamref name="TKey" />.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IDictionaryObjectStore<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			return _objectStores.GetDictionary<TKey, TValue>(name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IBagObjectStore<T> GetBag<T>(string name)
		{
			return _objectStores.GetBag<T>(name);
		}

		/// <summary>
		///     Creates a new database which is backed by memory.
		/// </summary>
		/// <remarks>
		///     This is probably only useful for tests.
		/// </remarks>
		/// <param name="typeResolver"></param>
		/// <returns></returns>
		public static IsabelDb CreateInMemory(ITypeResolver typeResolver = null)
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			try
			{
				connection.Open();
				CreateTables(connection);
				return new IsabelDb(connection, typeResolver);
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
		/// <param name="typeResolver"></param>
		/// <returns></returns>
		public static IsabelDb OpenOrCreate(string databasePath, ITypeResolver typeResolver = null)
		{
			if (!File.Exists(databasePath)) SQLiteConnection.CreateFile(databasePath);

			var connectionString = CreateConnectionString(databasePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				CreateTablesIfNecessary(connection);
				return new IsabelDb(connection, typeResolver);
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
		/// <param name="typeResolver"></param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static IsabelDb Open(string databaseFilePath, ITypeResolver typeResolver = null)
		{
			if (!File.Exists(databaseFilePath))
				throw new FileNotFoundException("Unable to open the given database", databaseFilePath);

			var connectionString = CreateConnectionString(databaseFilePath);
			var connection = new SQLiteConnection(connectionString);
			try
			{
				connection.Open();
				EnsureTableSchema(connection);
				return new IsabelDb(connection, typeResolver);
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
			var hasStoresTable = ObjectStores.DoesTableExist(connection);
			if (hasTypesTable && hasStoresTable)
				return;
			if (hasTypesTable != hasStoresTable)
				throw new NotImplementedException("Something something incompatible");

			CreateTables(connection);
		}

		private static void CreateTables(SQLiteConnection connection)
		{
			TypeStore.CreateTable(connection);
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
			if (!TypeStore.DoesTableExist(connection))
				throw new NotImplementedException();
			if (!ObjectStores.DoesTableExist(connection))
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