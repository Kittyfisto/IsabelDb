using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int32Serializer
		: ISQLiteSerializer<int>
	{
		public DbType DatabaseType => DbType.Int32;

		public object Serialize(int value)
		{
			return value;
		}

		public int Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			return reader.GetInt32(valueOrdinal);
		}
	}
}