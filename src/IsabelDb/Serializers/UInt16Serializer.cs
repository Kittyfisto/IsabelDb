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

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out ushort value)
		{
			var rawValue = reader.GetInt32(valueOrdinal);
			value = unchecked((ushort) rawValue);
			return true;
		}
	}
}