using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb
{
	public sealed class IsabelDb
		: IDisposable
	{
		sealed class TypeResolver
			: ITypeResolver
		{
			public Type Resolve(string typeName)
			{
				return Type.GetType(typeName);
			}

			public string GetName(Type type)
			{
				return type.AssemblyQualifiedName;
			}
		}

		private readonly SQLiteConnection _connection;
		private readonly TypeModel _typeModel;
		private readonly Dictionary<string, ObjectStore> _objectStores;
		private readonly TypeStore _typeStore;

		private IsabelDb(SQLiteConnection connection)
		{
			_connection = connection;
			_typeModel = TypeModel.Create();

			_objectStores = new Dictionary<string, ObjectStore>();

			var typeResolver = new TypeResolver();
			_typeStore = new TypeStore(connection, typeResolver);
		}

		public void Dispose()
		{
			_connection.Close();
			_connection.Dispose();
		}

		public static IsabelDb OpenOrCreate(string databaseIsdb)
		{
			if (!File.Exists(databaseIsdb))
			{
				SQLiteConnection.CreateFile(databaseIsdb);
			}

			var connectionString = CreateConnectionString(databaseIsdb);
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

		public IObjectStore this[string name]
		{
			get
			{
				ObjectStore store;
				if (!_objectStores.TryGetValue(name, out store))
				{
					store = new ObjectStore(_connection, _typeModel, _typeStore);
					_objectStores.Add(name, store);
				}

				return store;
			}
		}

		private static void CreateTablesIfNecessary(SQLiteConnection connection)
		{
			bool hasObjectsTable = TableExists(connection, "objects");
			bool hasTypesTable = TableExists(connection, "types");
			if (hasObjectsTable && hasTypesTable)
				return;
			if (hasTypesTable != hasObjectsTable)
				throw new NotImplementedException("Incompatible db");

			using (var command = connection.CreateCommand())
			{
				command.CommandText = "CREATE TABLE objects (" +
									  "key TEXT PRIMARY KEY NOT NULL," +
				                      "type INTEGER NOT NULL," +
				                      "value BLOB NOT NULL"+
				                      ")";
				command.ExecuteNonQuery();
			}

			using (var command = connection.CreateCommand())
			{
				command.CommandText = "CREATE TABLE types (" +
				                      "id INTEGER NOT NULL," +
				                      "typename TEXT NOT NULL" +
				                      ")";
				command.ExecuteNonQuery();
			}
		}

		private static bool TableExists(SQLiteConnection connection, string tableName)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{0}' LIMIT 0,1", tableName);
				var ret = command.ExecuteScalar();
				return Convert.ToInt32(ret) == 1;
			}
		}

		private static void EnsureTableSchema(SQLiteConnection connection)
		{
			if (!TableExists(connection, "objects"))
				throw new NotImplementedException();
			if (!TableExists(connection, "types"))
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