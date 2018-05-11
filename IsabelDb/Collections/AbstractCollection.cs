using System;
using System.Collections;
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
		private readonly bool _isReadOnly;
		private readonly string _name;

		protected AbstractCollection(SQLiteConnection connection,
		                             string name,
		                             string tableName,
		                             ISQLiteSerializer<TValue> valueSerializer,
		                             bool isReadOnly)
		{
			_connection = connection;
			_name = name;
			_tableName = tableName;
			_valueSerializer = valueSerializer;
			_isReadOnly = isReadOnly;
		}

		#region Implementation of ICollection

		public string Name => _name;

		public abstract CollectionType Type { get; }

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
			ThrowIfReadOnly();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0}", _tableName);
				command.ExecuteNonQuery();
			}
		}

		IEnumerable IReadOnlyCollection.GetAllValues()
		{
			return GetAllValues();
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

		protected void ThrowIfReadOnly()
		{
			if (_isReadOnly)
				throw new InvalidOperationException("The database has been opened read-only and therefore may not be modified");
		}

		#region Implementation of IInternalCollection

		public abstract Type KeyType { get; }

		public Type ValueType => typeof(TValue);

		public string ValueTypeName => throw new NotImplementedException();

		public string KeyTypeName => throw new NotImplementedException();

		#endregion
	}
}