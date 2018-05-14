using System;
using System.Collections;
using System.Data.SQLite;

namespace IsabelDb.Collections
{
	/// <summary>
	///     A placeholder for a collection who's key and/or value type could not be resolved.
	/// </summary>
	internal sealed class UnresolvedTypeCollection
		: AbstractCollection
	{
		private readonly CollectionType _type;
		private readonly Type _keyType;
		private readonly string _keyTypeName;
		private readonly Type _valueType;
		private readonly string _valueTypeName;

		public UnresolvedTypeCollection(SQLiteConnection connection,
		                                CollectionType type,
		                                string name,
		                                string tableName,
		                                Type keyType,
		                                string keyTypeName,
		                                Type valueType,
		                                string valueTypeName,
		                                bool isReadOnly)
			: base(connection, name, tableName, isReadOnly)
		{
			_type = type;
			_keyType = keyType;
			_keyTypeName = keyTypeName;
			_valueType = valueType;
			_valueTypeName = valueTypeName;
		}

		#region Implementation of IReadOnlyCollection

		public override CollectionType Type => _type;

		public override bool CanBeAccessed => false;

		public override string ValueTypeName => _valueTypeName;

		public override string KeyTypeName => _keyTypeName;

		public override IEnumerable GetAllValues()
		{
			string unavailableTypeName;
			if (_valueType == null)
				unavailableTypeName = _valueTypeName;
			else
				unavailableTypeName = _keyTypeName;
			var message = string.Format("This collection cannot be accessed because the type '{0}' is not available!",
			                            unavailableTypeName);
			throw new InvalidOperationException(message);
		}

		public override Type KeyType => _keyType;

		public override Type ValueType => _valueType;

		#endregion
	}
}