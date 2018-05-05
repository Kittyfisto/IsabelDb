using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using ProtoBuf.Meta;

namespace IsabelDb.TypeModels
{
	/// <summary>
	///     A type model which describes every serializable type to the point which allows protobuf
	///     to generate serialization / deserialization code.
	///     The type model can itself be stored in the database and then restored from it again.
	/// </summary>
	internal sealed class ProtobufTypeModel
	{
		private static readonly HashSet<Type> BuiltInTypes;

		private readonly TypeModel _typeModel;
		private readonly RuntimeTypeModel _runtimeTypeModel;
		private readonly Dictionary<Type, MetaType> _metaTypes;

		static ProtobufTypeModel()
		{
			BuiltInTypes = new HashSet<Type>
			{
				typeof(object),
				typeof(IPAddress)
			};
			foreach (var type in TypeModel.BuiltInProtobufTypes) BuiltInTypes.Add(type);
		}

		public ProtobufTypeModel(TypeModel typeModel)
		{
			_typeModel = typeModel;
			_runtimeTypeModel = ProtoBuf.Meta.TypeModel.Create();
			_metaTypes = new Dictionary<Type, MetaType>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ProtoBuf.Meta.TypeModel Compile()
		{
			foreach(var type in _typeModel)
			{
				var typeDescription = _typeModel.GetTypeDescription(type);
				Add(typeDescription);
			}
			return _runtimeTypeModel.Compile();
		}

		public void Add(TypeDescription typeDescription)
		{
			var baseType = typeDescription.BaseType;
			if (baseType != null)
			{
				Add(baseType);
			}

			// Protobuf doesn't allow us to register built-in types
			// (such as System.String), hence don't try to add those.
			var type = typeDescription.Type;
			if (!TypeModel.IsBuiltIn(type) && !_metaTypes.TryGetValue(type, out var metaType))
			{
				metaType = _runtimeTypeModel.Add(type, applyDefaultBehaviour: false);
				ConfigureMetaType(metaType, typeDescription);
				_metaTypes.Add(type, metaType);

				if (baseType != null)
					AddSubTypeTo(baseType, typeDescription);
			}
		}

		private void ConfigureMetaType(MetaType metaType, TypeDescription typeDescription)
		{
			foreach (var member in typeDescription.Fields)
			{
				var field = metaType.AddField(member.MemberId, member.Member.Name);
				field.IsRequired = true;
			}
		}

		private void AddSubTypeTo(TypeDescription baseType, TypeDescription typeDescription)
		{
			var baseMetaType = _metaTypes[baseType.Type];
			baseMetaType.AddSubType(typeDescription.TypeId, typeDescription.Type);
		}

		/// <summary>
		///     Creates a new type model from the types stored in the given database *and* the
		///     given specified types. Types in the database which are not part of the given list
		///     are simply ignored. Types in the given list which are not part of the database are
		///     added to it.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="supportedTypes"></param>
		/// <returns></returns>
		public static CompiledTypeModel Create(SQLiteConnection connection,
		                                       IEnumerable<Type> supportedTypes)
		{
			var allTypes = BuiltInTypes.Concat(supportedTypes).ToList();
			var typeRegistry = new TypeResolver(allTypes);
			var typeModel = TypeModel.Read(connection, typeRegistry);
			typeModel.Add(allTypes);
			// If we reach this point, then both type models are compatible to each other
			// and we can create a serializer for it!
			var serializer = Compile(typeModel);

			// Now that the serializer is compiled, we should also update the typemodel in the database.
			typeModel.Write(connection);

			return new CompiledTypeModel(serializer, typeModel);
		}

		public static ProtoBuf.Meta.TypeModel Compile(TypeModel typeModel)
		{
			var protobufTypeModel = new ProtobufTypeModel(typeModel);
			return protobufTypeModel.Compile();
		}
	}
}