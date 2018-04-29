using System;

namespace IsabelDb
{
	internal interface IInternalObjectStore
		: IObjectStore
	{
		Type ObjectType { get; }
	}
}