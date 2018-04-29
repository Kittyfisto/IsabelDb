using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class UInt16Serializer
		: ISQLiteSerializer<ushort>
	{
		public DbType DatabaseType => DbType.UInt16;

		public object Serialize(ushort value)
		{
			return (int) value;
		}

		public ushort Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			var rawValue = reader.GetInt32(valueOrdinal);
			return unchecked((ushort) rawValue);
		}
	}
}