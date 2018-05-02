using System;
using System.Collections.Generic;

namespace IsabelDb
{
	internal sealed class CompiledTypeModel
		: ITypeModel
	{
		private readonly ProtoBuf.Meta.TypeModel _protobufTypeModel;
		private readonly IReadOnlyDictionary<Type, int> _typeToId;
		private readonly IReadOnlyDictionary<int, Type> _typeIdToType;

		public CompiledTypeModel(ProtoBuf.Meta.TypeModel protobufTypeModel, IReadOnlyDictionary<Type, int> typeToId, IReadOnlyDictionary<int, Type> typeIdToType)
		{
			_protobufTypeModel = protobufTypeModel;
			_typeToId = typeToId;
			_typeIdToType = typeIdToType;
		}

		#region Implementation of ITypeModel

		public int GetTypeId(Type type)
		{
			if (!_typeToId.TryGetValue(type, out var typeId))
				throw new ArgumentException(string.Format("The type '{0}' has not been registered!", type));

			return typeId;
		}

		public Type GetType(int typeId)
		{
			if (!_typeIdToType.TryGetValue(typeId, out var type))
				return null;

			return type;
		}

		public bool IsRegistered(Type type)
		{
			return _typeToId.ContainsKey(type);
		}

		public ProtoBuf.Meta.TypeModel Serializer => _protobufTypeModel;

		#endregion
	}
}