using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace IsabelDb
{
	internal sealed class TypeStore
	{
		private const string TableName = "isabel_types";

		private readonly SQLiteConnection _connection;
		private readonly ITypeResolver _typeResolver;
		private readonly Dictionary<Type, int> _typesToId;
		private readonly Dictionary<int, Type> _idToTypes;
		private int _nextId;

		public TypeStore(SQLiteConnection connection,
		                 ITypeResolver typeResolver)
		{
			_connection = connection;
			_typeResolver = typeResolver;

			ReadTypes(connection, typeResolver, out _typesToId, out _idToTypes);
			if (_idToTypes.Count > 0)
			{
				_nextId = _idToTypes.Keys.Max() + 1;
			}
			else
			{
				_nextId = 1;
			}
		}

		public Type GetTypeFromTypeId(int typeId)
		{
			_idToTypes.TryGetValue(typeId, out var type);
			return type;
		}

		public int GetOrCreateTypeId(Type type)
		{
			int id;
			if (!_typesToId.TryGetValue(type, out id))
			{
				id = ++_nextId;
				_idToTypes.Add(id, type);
				_typesToId.Add(type, id);

				using (var command = _connection.CreateCommand())
				{
					command.CommandText = string.Format("INSERT INTO {0} (typename, id) VALUES (@typename, @id)", TableName);
					var typename = command.Parameters.Add("@typename", DbType.String);
					var idParameter = command.Parameters.Add("@id", DbType.Int32);

					typename.Value = _typeResolver.GetName(type);
					idParameter.Value = id;
					command.ExecuteNonQuery();
				}
			}

			return id;
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
				                      "id INTEGER NOT NULL," +
				                      "typename TEXT NOT NULL" +
				                      ")", TableName);
				command.ExecuteNonQuery();
			}
		}

		private static void ReadTypes(SQLiteConnection connection,
		                              ITypeResolver typeResolver,
		                              out Dictionary<Type, int> typesToId,
		                              out Dictionary<int, Type> idToTypes)
		{
			typesToId = new Dictionary<Type, int>();
			idToTypes = new Dictionary<int, Type>();

			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT typename, id FROM {0}", TableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var typeName = reader.GetString(0);
						var id = reader.GetInt32(1);

						var type = typeResolver.Resolve(typeName);
						typesToId.Add(type, id);
						idToTypes.Add(id, type);
					}
				}
			}
		}
	}
}