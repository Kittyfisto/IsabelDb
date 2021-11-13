using System;

namespace IsabelDb.TypeModels
{
	internal interface ITypeModel
	{
		int GetTypeId(Type type);
		Type GetType(int typeId);
		ProtoBuf.Meta.TypeModel Serializer { get; }
	}
}