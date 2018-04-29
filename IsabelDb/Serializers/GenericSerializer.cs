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
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;
		private readonly Type _type;
		private readonly int? _typeId;

		public GenericSerializer(TypeModel typeModel, TypeStore typeStore)
		{
			_typeModel = typeModel;
			_typeStore = typeStore;
			_type = typeof(T);
			if (_type.IsSealed)
				_typeId = typeStore.GetOrCreateTypeId(_type);
		}

		public DbType DatabaseType => DbType.Binary;

		public bool StorePerValueTypeInformation => true;

		public object Serialize(T value)
		{
			using (var stream = new MemoryStream())
			{
				_typeModel.Serialize(stream, value);
				return stream.ToArray();
			}
		}

		public object Serialize(T value, out int typeId)
		{
			// We might have cached the type id for this type if it's sealed.
			typeId = _typeId ?? _typeStore.GetOrCreateTypeId(value.GetType());
			return Serialize(value);
		}

		public T Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeOrdinal)
		{
			Type type;
			if (typeOrdinal == -1)
			{
				type = _type;
			}
			else
			{
				var typeId = reader.GetInt32(typeOrdinal);
				type = _typeStore.GetTypeFromTypeId(typeId);
			}
			return Deserialize(reader, valueOrdinal, type);
		}

		private T Deserialize(SQLiteDataReader reader, int valueOrdinal, Type expectedType)
		{
			var serializedValue = (byte[]) reader.GetValue(valueOrdinal);
			using (var stream = new MemoryStream(serializedValue))
			{
				var value = _typeModel.Deserialize(stream, null, expectedType);
				return (T) value;
			}
		}
	}
}