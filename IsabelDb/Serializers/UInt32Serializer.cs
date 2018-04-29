using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class UInt32Serializer
		: ISQLiteSerializer<uint>
	{
		public DbType DatabaseType => DbType.UInt32;

		public object Serialize(uint value)
		{
			return (long) value;
		}

		public uint Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			var rawValue = reader.GetInt64(valueOrdinal);
			return unchecked((uint) rawValue);
		}
	}
}