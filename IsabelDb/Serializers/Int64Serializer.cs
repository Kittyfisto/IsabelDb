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

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out long value)
		{
			value = reader.GetInt64(valueOrdinal);
			return true;
		}
	}
}