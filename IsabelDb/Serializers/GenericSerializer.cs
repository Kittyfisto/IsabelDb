using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class GenericSerializer<T>
		: ISQLiteSerializer<T>
	{
		private readonly Serializer _serializer;

		public GenericSerializer(Serializer serializer)
		{
			_serializer = serializer;
		}

		public DbType DatabaseType => DbType.Binary;

		public object Serialize(T value)
		{
			return _serializer.Serialize(value);
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out T value)
		{
			var serializedValue = (byte[]) reader.GetValue(valueOrdinal);
			var deserializedValue = _serializer.Deserialize(serializedValue);
			if (deserializedValue == null)
			{
				value = default(T);
				return false;
			}

			value = (T) deserializedValue;
			return true;
		}
	}
}