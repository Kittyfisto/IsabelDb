using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int16Serializer
		: ISQLiteSerializer<short>
	{
		public DbType DatabaseType => DbType.Int16;

		public object Serialize(short value)
		{
			return value;
		}

		public short Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			return reader.GetInt16(valueOrdinal);
		}
	}
}