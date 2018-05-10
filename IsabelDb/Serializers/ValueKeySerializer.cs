using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class ValueKeySerializer
		: ISQLiteSerializer<ValueKey>
	{
		public DbType DatabaseType => DbType.Int64;

		public object Serialize(ValueKey value)
		{
			return value.Value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out ValueKey value)
		{
			value = new ValueKey(reader.GetInt64(valueOrdinal));
			return true;
		}
	}
}
