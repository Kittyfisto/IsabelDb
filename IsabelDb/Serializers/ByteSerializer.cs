using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class ByteSerializer
		: ISQLiteSerializer<byte>
	{
		public DbType DatabaseType => DbType.Byte;

		public object Serialize(byte value)
		{
			return value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out byte value)
		{
			value = reader.GetByte(valueOrdinal);
			return true;
		}
	}
}