using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal abstract class AbstractCollection<TValue>
		: ICollection<TValue>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;

		protected AbstractCollection(SQLiteConnection connection,
		                             string tableName,
		                             ISQLiteSerializer<TValue> valueSerializer)
		{
			_connection = connection;
			_tableName = tableName;
			_valueSerializer = valueSerializer;
		}

		#region Implementation of ICollection

		public IEnumerable<TValue> GetAllValues()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0}", _tableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 0, out var value))
							yield return value;
					}
				}
			}
		}

		public void Clear()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0}", _tableName);
				command.ExecuteNonQuery();
			}
		}

		public long Count()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT (*) FROM {0}", _tableName);
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		#endregion
	}
}