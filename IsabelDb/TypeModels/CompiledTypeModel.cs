﻿using System;
using System.Collections.Generic;

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

		public IEnumerable<TypeDescription> TypeDescriptions => _typeModel.TypeDescriptions;

		public TypeDescription GetTypeDescription(int typeId)
		{
			return _typeModel.GetTypeDescription(typeId);
		}

		public int GetTypeId(Type type)
		{
			return _typeModel.GetTypeId(type);
		}

		public Type GetType(int typeId)
		{
			return _typeModel.TryGetType(typeId);
		}

		public bool IsRegistered(Type type)
		{
			return _typeModel.IsTypeRegistered(type);
		}

		public ProtoBuf.Meta.TypeModel Serializer => _protobufTypeModel;

		#endregion
	}
}