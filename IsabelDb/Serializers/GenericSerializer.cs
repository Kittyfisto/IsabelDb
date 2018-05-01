using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb.Serializers
{
	internal sealed class GenericSerializer<T>
		: ISQLiteSerializer<T>
	{
		private readonly int? _typeId;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

		public GenericSerializer(TypeModel typeModel, TypeStore typeStore)
		{
			_typeModel = typeModel;
			_typeStore = typeStore;
			var type = typeof(T);
			if (type.IsSealed)
				_typeId = typeStore.GetOrCreateTypeId(type);
		}

		public DbType DatabaseType => DbType.Binary;

		public object Serialize(T value)
		{
			// We might have cached the type id for this type if it's sealed.
			var type = value.GetType();
			var typeId = _typeId ?? _typeStore.GetOrCreateTypeId(type);
			if (typeId == -1)
				throw new ArgumentException(string.Format("The type '{0}' has not been registered and thus cannot be stored", type.FullName));

			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(typeId);
				try
				{
					_typeModel.Serialize(stream, value);
				}
				catch (InvalidOperationException e)
				{
					throw new ArgumentException(string.Format("Unable to serialize value: {0}",
					                                          e.Message));
				}

				return stream.ToArray();
			}
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out T value)
		{
			var serializedValue = (byte[]) reader.GetValue(valueOrdinal);
			using (var stream = new MemoryStream(serializedValue))
			using (var tmp = new BinaryReader(stream))
			{
				var typeId = tmp.ReadInt32();
				var type = _typeStore.GetTypeFromTypeId(typeId);
				if (type == null)
				{
					value = default(T);
					return false;
				}

				value = (T) _typeModel.Deserialize(stream, value: null, type: type);
				return true;
			}
		}
	}
}