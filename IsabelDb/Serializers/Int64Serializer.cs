using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int64Serializer
		: ISQLiteSerializer<long>
	{
		public DbType DatabaseType => DbType.Int64;

		public object Serialize(long value)
		{
			return value;
		}

		public long Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			return reader.GetInt64(valueOrdinal);
		}
	}
}