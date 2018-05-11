using System;

namespace IsabelDb
{
	internal interface IInternalCollection
		: ICollection
	{
		Type KeyType { get; }
		Type ValueType { get; }
		string KeyTypeName { get; }
		string ValueTypeName { get; }
	}
}