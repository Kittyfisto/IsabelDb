using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class ValueKeySerializer
		: ISQLiteSerializer<RowId>
	{
		public DbType DatabaseType => DbType.Int64;

		public object Serialize(RowId value)
		{
			return value.Value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out RowId value)
		{
			value = new RowId(reader.GetInt64(valueOrdinal));
			return true;
		}
	}
}
