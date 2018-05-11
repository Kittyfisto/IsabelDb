using System;
using System.Collections;

namespace IsabelDb.Collections
{
	/// <summary>
	///     A placeholder for a collection who's key and/or value type could not be resolved.
	/// </summary>
	internal sealed class UnresolvedTypeCollection
		: ICollection
	{
		private readonly CollectionType _type;
		private readonly Type _keyType;
		private readonly string _keyTypeName;
		private readonly Type _valueType;
		private readonly string _valueTypeName;
		private readonly string _name;

		public UnresolvedTypeCollection(CollectionType type,
		                                string name,
		                                Type keyType,
		                                string keyTypeName,
		                                Type valueType,
		                                string valueTypeName)
		{
			_type = type;
			_name = name;
			_keyType = keyType;
			_keyTypeName = keyTypeName;
			_valueType = valueType;
			_valueTypeName = valueTypeName;
		}

		#region Implementation of IReadOnlyCollection

		public string Name => _name;

		public CollectionType Type => _type;

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

		public string ValueTypeName => _valueTypeName;

		public string KeyTypeName => _keyTypeName;

		#endregion
	}
}