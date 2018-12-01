using System;
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
			if (value == null)
				return null;

			return _serializer.Serialize(value);
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out T value)
		{
			var tmp = reader.GetValue(valueOrdinal);
			if (Convert.IsDBNull(tmp))
			{
				value = default(T);
				return true;
			}

			var serializedValue = (byte[]) tmp;
			var deserializedValue = Deserialize(serializedValue);
			if (deserializedValue == null)
			{
				value = default(T);
				return false;
			}

			value = (T) deserializedValue;
			return true;
		}

		public object Deserialize(byte[] serializedValue)
		{
			return _serializer.Deserialize(serializedValue);
		}
	}
}