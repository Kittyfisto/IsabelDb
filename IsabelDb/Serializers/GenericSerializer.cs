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
			var typeId = _typeId ?? _typeStore.GetOrCreateTypeId(value.GetType());
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(typeId);
				_typeModel.Serialize(stream, value);
				return stream.ToArray();
			}
		}

		public T Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			var serializedValue = (byte[]) reader.GetValue(valueOrdinal);
			using (var stream = new MemoryStream(serializedValue))
			using (var tmp = new BinaryReader(stream))
			{
				var typeId = tmp.ReadInt32();
				var type = _typeStore.GetTypeFromTypeId(typeId);
				var value = _typeModel.Deserialize(stream, value: null, type: type);
				return (T) value;
			}
		}
	}
}