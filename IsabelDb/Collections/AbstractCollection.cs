using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal abstract class AbstractCollection
		: IInternalCollection
	{
		private readonly SQLiteConnection _connection;
		private readonly string _name;
		private readonly string _tableName;
		private readonly bool _isReadOnly;
		private bool _isDropped;

		protected AbstractCollection(SQLiteConnection connection, string name, string tableName, bool isReadOnly)
		{
			_connection = connection;
			_name = name;
			_tableName = tableName;
			_isReadOnly = isReadOnly;
		}

		#region Implementation of ICollection

		public void Clear()
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("DELETE FROM {0}", _tableName);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region Implementation of IReadOnlyCollection

		public string Name => _name;

		public abstract CollectionType Type { get; }

		public abstract bool CanBeAccessed { get; }

		public long Count()
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT (*) FROM {0}", _tableName);
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		public abstract IEnumerable GetAllValues();

		#endregion

		#region Implementation of IInternalCollection

		public abstract Type KeyType { get; }

		public abstract Type ValueType { get; }

		public abstract string ValueTypeName { get; }

		public abstract string KeyTypeName { get; }

		public string TableName => _tableName;

		public void MarkAsDropped()
		{
			_isDropped = true;
		}

		#endregion

		protected void ThrowIfReadOnly()
		{
			if (_isReadOnly)
				throw new InvalidOperationException("The database has been opened read-only and therefore may not be modified");
		}

		protected void ThrowIfDropped()
		{
			if (_isDropped)
				throw new InvalidOperationException(string.Format("This collection (\"{0}\") has been removed from the database and may no longer be used",
				                                                  Name));
		}

		protected bool IsDropped => _isDropped;
	}

	internal abstract class AbstractCollection<TValue>
		: AbstractCollection
		, ICollection<TValue>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _name;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;

		protected AbstractCollection(SQLiteConnection connection,
		                             string name,
		                             string tableName,
		                             ISQLiteSerializer<TValue> valueSerializer,
		                             bool isReadOnly)
			: base(connection, name, tableName, isReadOnly)
		{
			_connection = connection;
			_name = name;
			_tableName = tableName;
			_valueSerializer = valueSerializer;
		}

		public override string ToString()
		{
			if (IsDropped)
				return string.Format("This collection (\"{0}\") has been removed from the database and may no longer be used", Name);

			var keyType = KeyType;
			if (keyType == null)
				return string.Format("{0}<{1}>(\"{2}\")", Type, ValueType.FullName, Name);

			return string.Format("{0}<{1}, {2}>(\"{3}\")", Type, KeyType.FullName, ValueType.FullName, Name);
		}

		#region Implementation of ICollection

		public override bool CanBeAccessed => true;
		public override Type ValueType => typeof(TValue);
		public override string ValueTypeName => null;

		IEnumerable<TValue> IReadOnlyCollection<TValue>.GetAllValues()
		{
			ThrowIfDropped();

			return GetAllValuesInternal();
		}

		public override IEnumerable GetAllValues()
		{
			if (IsDropped)
				return Enumerable.Empty<TValue>();

			return GetAllValuesInternal();
		}

		#endregion

		#region Implementation of IInternalCollection

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