using System;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using log4net;

namespace IsabelDb
{
	/// <summary>
	///     Provides access to the 'isabel_variables' table.
	/// </summary>
	internal sealed class VariablesTable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const string TableName = "isabel_variables";
		private const string IsabelAssemblyVersionKey = "initial_assembly_version";
		public const string IsabelSchemaVersionKey = "schema_version";

		private readonly SQLiteConnection _connection;
		private readonly int _schemaVersion;
		private readonly Version _assemblyVersion;

		public VariablesTable(SQLiteConnection connection)
		{
			_connection = connection;
			_assemblyVersion = Version.Parse(Read(IsabelAssemblyVersionKey));
			_schemaVersion = int.Parse(Read(IsabelSchemaVersionKey), NumberStyles.Integer, CultureInfo.InvariantCulture);

			Log.DebugFormat("Initial assembly version: {0}", _assemblyVersion);
			Log.DebugFormat("Schema version: {0}", _schemaVersion);

			if (_schemaVersion < Constants.DatabaseSchemaVersion)
				throw new IncompatibleDatabaseSchemaException(string.Format("The database was created with an earlier version of IsabelDb (Schema version: {0}) and its schema is incompatible to this version.", _schemaVersion));
			if (_schemaVersion > Constants.DatabaseSchemaVersion)
				throw new IncompatibleDatabaseSchemaException(string.Format("The database was created with a newer version of IsabelDb (Schema version: {0}) and its schema is incompatible to this version.", _schemaVersion));
		}

		/// <summary>
		///     The version of the IsabelDb assembly with which the database was initially created.
		/// </summary>
		public Version AssemblyVersion => _assemblyVersion;

		/// <summary>
		///     The version of the current database schema.
		/// </summary>
		public int SchemaVersion => _schemaVersion;

		private string Read(string key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0} WHERE key = @key", TableName);
				command.Parameters.AddWithValue("@key", key);
				return command.ExecuteScalar()?.ToString();
			}
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var transaction = connection.BeginTransaction())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = string.Format("CREATE TABLE {0} (" +
					                                    "key string PRIMARY KEY NOT NULL," +
					                                    "value TEXT NOT NULL" +
					                                    ")", TableName);
					command.ExecuteNonQuery();
				}

				Add(connection, IsabelAssemblyVersionKey, Constants.AssemblyVersion);
				Add(connection, IsabelSchemaVersionKey, Constants.DatabaseSchemaVersion);

				transaction.Commit();
			}
		}

		[Pure]
		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return Database.TableExists(connection, TableName);
		}

		private static void Add(SQLiteConnection connection, string key, object value)
		{
			Add(connection, key, value?.ToString());
		}

		private static void Add(SQLiteConnection connection, string key, string value)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (key, value) VALUES (@key, @value)", TableName);
				command.Parameters.AddWithValue("@key", key);
				command.Parameters.AddWithValue("@value", value);
				command.ExecuteNonQuery();
			}
		}
	}
}