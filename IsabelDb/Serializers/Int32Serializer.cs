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

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out int value)
		{
			value = reader.GetInt32(valueOrdinal);
			return true;
		}
	}
}