using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal abstract class AbstractCollection<TValue>
		: ICollection<TValue>
			, IInternalCollection
	{
		private readonly SQLiteConnection _connection;
		private readonly bool _isReadOnly;
		private readonly string _name;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;
		private bool _dropped;

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

		protected void ThrowIfReadOnly()
		{
			if (_isReadOnly)
				throw new InvalidOperationException("The database has been opened read-only and therefore may not be modified");
		}

		protected void ThrowIfDropped()
		{
			if (_dropped)
				throw new InvalidOperationException("This collection has been removed from the database and may no longer be modified");
		}

		#region Implementation of ICollection

		public string Name => _name;

		public abstract CollectionType Type { get; }

		public IEnumerable<TValue> GetAllValues()
		{
			if (_dropped)
				return Enumerable.Empty<TValue>();

			return GetAllValuesInternal();
		}

		public void Clear()
		{
			ThrowIfReadOnly();

			if (_dropped)
				return;

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
			if (_dropped)
				return 0;

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT (*) FROM {0}", _tableName);
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		#endregion

		#region Implementation of IInternalCollection

		public abstract Type KeyType { get; }

		public Type ValueType => typeof(TValue);

		public string ValueTypeName => throw new NotImplementedException();

		public string KeyTypeName => throw new NotImplementedException();

		#endregion

		#region Implementation of IInternalCollection

		public string TableName => _tableName;

		public void MarkAsDropped()
		{
			_dropped = true;
		}

		#endregion

		private IEnumerable<TValue> GetAllValuesInternal()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT value FROM {0}", _tableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
						if (_valueSerializer.TryDeserialize(reader, valueOrdinal: 0, value: out var value))
							yield return value;
				}
			}
		}
	}
}