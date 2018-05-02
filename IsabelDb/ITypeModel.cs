using System;

namespace IsabelDb
{
	internal interface ITypeModel
	{
		int GetTypeId(Type type);
		Type GetType(int typeId);
		ProtoBuf.Meta.TypeModel Serializer { get; }
	}
}