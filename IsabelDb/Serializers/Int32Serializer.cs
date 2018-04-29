using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class Int32Serializer
		: ISQLiteSerializer<int>
	{
		public DbType DatabaseType => DbType.Int32;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(int value, out int typeId)
		{
			typeId = -1;
			return value;
		}

		public int Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			return reader.GetInt32(valueOrdinal);
		}
	}
}
