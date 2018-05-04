using System;

namespace IsabelDb.TypeModels
{
	internal sealed class CompiledTypeModel
		: ITypeModel
	{
		private readonly ProtoBuf.Meta.TypeModel _protobufTypeModel;
		private readonly TypeModel _typeModel;

		public CompiledTypeModel(ProtoBuf.Meta.TypeModel protobufTypeModel, TypeModel typeModel)
		{
			_protobufTypeModel = protobufTypeModel;
			_typeModel = typeModel;
		}

		#region Implementation of ITypeModel

		public int GetTypeId(Type type)
		{
			return _typeModel.GetTypeId(type);
		}

		public Type GetType(int typeId)
		{
			return _typeModel.GetType(typeId);
		}

		public bool IsRegistered(Type type)
		{
			return _typeModel.IsTypeRegistered(type);
		}

		public ProtoBuf.Meta.TypeModel Serializer => _protobufTypeModel;

		#endregion
	}
}