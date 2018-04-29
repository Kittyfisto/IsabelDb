using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int16Serializer
		: ISQLiteSerializer<short>
	{
		public DbType DatabaseType => DbType.Int16;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(short value, out int typeId)
		{
			typeId = -1;
			return value;
		}

		public short Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeOrdinal)
		{
			return reader.GetInt16(valueOrdinal);
		}
	}
}