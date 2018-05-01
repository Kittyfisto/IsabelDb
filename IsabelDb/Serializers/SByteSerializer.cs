using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class SByteSerializer
		: ISQLiteSerializer<sbyte>
	{
		public DbType DatabaseType => DbType.SByte;

		public object Serialize(sbyte value)
		{
			return value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out sbyte value)
		{
			value = (sbyte)reader.GetInt16(valueOrdinal);
			return true;
		}
	}
}