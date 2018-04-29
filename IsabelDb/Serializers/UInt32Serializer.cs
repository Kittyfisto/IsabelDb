using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class UInt32Serializer
		: ISQLiteSerializer<uint>
	{
		public DbType DatabaseType => DbType.UInt32;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(uint value, out int typeId)
		{
			typeId = -1;
			return (long)value;
		}

		public uint Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			var rawValue = reader.GetInt64(valueOrdinal);
			return unchecked((uint) rawValue);
		}
	}
}