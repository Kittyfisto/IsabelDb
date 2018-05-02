using System;

namespace IsabelDb.TypeModel
{
	internal interface ITypeModel
	{
		int GetTypeId(Type type);
		Type GetType(int typeId);
		ProtoBuf.Meta.TypeModel Serializer { get; }
	}
}