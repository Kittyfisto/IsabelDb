using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using ProtoBuf.Meta;

namespace IsabelDb
{
	/// <summary>
	///     A type model which describes every serializable type to the point which allows protobuf
	///     to generate serialization / deserialization code.
	///     The type model can itself be stored in the database and then restored from it again.
	/// </summary>
	internal sealed class TypeModel
		: ITypeModel
	{
		public const string TypeTableName = "isabel_types";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly HashSet<Type> NativeProtobufTypes;
		private static readonly HashSet<Type> BuiltInTypes;

		private readonly TypeRegistry _typeRegistry;
		private readonly Dictionary<int, Type> _idToTypes;
		private readonly Dictionary<Type, MetaType> _metaTypes;
		private readonly RuntimeTypeModel _typeModel;
		private readonly Dictionary<Type, int> _typesToId;
		private int _nextId;

		static TypeModel()
		{
			// These types are known by protobuf, but protobuf throws an exception
			// if we try to add them to the type model...
			NativeProtobufTypes = new HashSet<Type>
				{
					typeof(string),
					typeof(float),
					typeof(double),
					typeof(byte),
					typeof(sbyte),
					typeof(short),
					typeof(ushort),
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(byte[])
				};

			BuiltInTypes = new HashSet<Type>
			{
				typeof(object),
				typeof(IPAddress)
			};
			foreach (var type in NativeProtobufTypes)
			{
				BuiltInTypes.Add(type);
			}
		}

		public TypeModel(TypeRegistry typeRegistry)
		{
			_typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
			_idToTypes = new Dictionary<int, Type>();
			_typesToId = new Dictionary<Type, int>();
			_metaTypes = new Dictionary<Type, MetaType>();
			_typeModel = ProtoBuf.Meta.TypeModel.Create();
			_nextId = 1;
		}

		/// <summary>
		///     Creates a new type model from the types stored in the given database.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="supportedTypes"></param>
		/// <returns></returns>
		public static CompiledTypeModel Create(SQLiteConnection connection,
		                                       IEnumerable<Type> supportedTypes)
		{
			var allTypes = BuiltInTypes.Concat(supportedTypes).ToList();
			var typeRegistry = new TypeRegistry(allTypes);
			var typeModel = new TypeModel(typeRegistry);
			typeModel.AddTypesFromDatabase(connection);
			typeModel.Add(connection, allTypes);
			return typeModel.Compile();
		}

		public CompiledTypeModel Compile()
		{
			return new CompiledTypeModel(_typeModel.Compile(),
			                             _typesToId,
			                             _idToTypes);
		}

		[Pure]
		public int GetTypeId(Type type)
		{
			if (!_typesToId.TryGetValue(type, out var id))
				throw new ArgumentException(string.Format("The type '{0}' has not been registered and thus cannot be stored",
				                                          type.FullName));

			return id;
		}

		[Pure]
		public Type GetType(int typeId)
		{
			_idToTypes.TryGetValue(typeId, out var type);
			return type;
		}

		public ProtoBuf.Meta.TypeModel Serializer => _typeModel;

		public MetaType Add(Type type, int id)
		{
			if (_idToTypes.ContainsKey(id))
				throw new NotImplementedException();

			_typesToId.Add(type, id);
			_idToTypes.Add(id, type);
			_nextId = Math.Max(_nextId, id + 1);

			// Protobuf doesn't allow us to register built-in types
			// (such as System.String), hence don't try to add those.
			if (!NativeProtobufTypes.Contains(type))
			{
				var metaType = _typeModel.Add(type, applyDefaultBehaviour: true);
				_metaTypes.Add(type, metaType);

				var typeId = _typesToId[type];
				var baseType = type.BaseType;
				if (baseType != null)
					AddSubTypeTo(baseType, type, typeId);
				return metaType;
			}

			return null;
		}

		public bool Add(Type type)
		{
			if (!_typesToId.TryGetValue(type, out var typeId))
			{
				typeId = _nextId;
				++_nextId;
				Add(type, typeId);
				return true;
			}

			return false;
		}

		private void Add(SQLiteConnection connection,
		                 IEnumerable<Type> supportedTypes)
		{
			using (var transaction = connection.BeginTransaction())
			using (var command = connection.CreateCommand())
			{
				foreach (var type in supportedTypes)
					if (Add(type))
					{
						var typeName = _typeRegistry.GetName(type);
						command.CommandText = string.Format("INSERT INTO {0} (typename, id) VALUES (@typename, @id)", TypeTableName);
						var typename = command.Parameters.Add("@typename", DbType.String);
						var idParameter = command.Parameters.Add("@id", DbType.Int32);

						typename.Value = typeName;
						idParameter.Value = _typesToId[type];
						command.ExecuteNonQuery();
					}

				transaction.Commit();
			}
		}

		private void AddSubTypeTo(Type baseType, Type type, int typeId)
		{
			Add(baseType);
			var baseMetaType = _metaTypes[baseType];
			baseMetaType.AddSubType(typeId, type);
		}

		private void AddTypesFromDatabase(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT typename, id FROM {0}", TypeTableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var typeName = reader.GetString(i: 0);
						var id = reader.GetInt32(i: 1);

						TryResolveType(typeName, id);
					}
				}
			}
		}

		private void TryResolveType(string typeName, int id)
		{
			try
			{
				var type = _typeRegistry.Resolve(typeName);
				if (type != null)
					Add(type, id);
				else
					Log.ErrorFormat("Unable to resolve '{0}' to a .NET type! Values of this type will not be readable", typeName);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Unable to resolve '{0}' to a .NET type! Values of this type will not be readable: {0}", typeName,
				                e);
			}
		}
	}
}