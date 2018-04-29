using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int64Serializer
		: ISQLiteSerializer<long>
	{
		public DbType DatabaseType => DbType.Int64;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(long value, out int typeId)
		{
			typeId = -1;
			return value;
		}

		public long Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			return reader.GetInt64(valueOrdinal);
		}
	}
}