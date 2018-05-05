using System;

namespace IsabelDb
{
	internal interface IInternalCollection
		: ICollection
	{
		Type ValueType { get; }
	}
}