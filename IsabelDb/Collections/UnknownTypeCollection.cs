using System;
using System.Collections;

namespace IsabelDb.Collections
{
	/// <summary>
	/// A placeholder value for a collection who's type is unknown.
	/// </summary>
	internal sealed class UnknownTypeCollection
		: IInternalCollection
	{
		private readonly Type _valueType;
		private readonly Type _keyType;
		private readonly string _name;

		public UnknownTypeCollection(string name, string tableName, int? keyTypeId, Type keyType, int valueTypeId, Type valueType)
		{
			_name = name;
			_keyType = keyType;
			_valueType = valueType;
		}

		#region Implementation of IReadOnlyCollection

		public string Name => _name;

		public CollectionType Type => CollectionType.Unknown;

		public IEnumerable GetAllValues()
		{
			throw new NotImplementedException();
		}

		public long Count()
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation of IInternalCollection

		public Type KeyType => _keyType;

		public Type ValueType => _valueType;

		public string ValueTypeName => throw new NotImplementedException();

		public string KeyTypeName => throw new NotImplementedException();

		#endregion
	}
}