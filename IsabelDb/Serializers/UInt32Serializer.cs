using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class UInt32Serializer
		: ISQLiteSerializer<uint>
	{
		public DbType DatabaseType => DbType.Int64;

		public object Serialize(uint value)
		{
			return (long) value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out uint value)
		{
			var rawValue = reader.GetInt64(valueOrdinal);
			value = unchecked((uint) rawValue);
			return true;
		}
	}
}