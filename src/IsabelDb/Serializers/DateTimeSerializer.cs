using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class DateTimeSerializer
		: ISQLiteSerializer<DateTime>
	{
		#region Implementation of ISQLiteSerializer

		public DbType DatabaseType => DbType.Int64;

		#endregion

		#region Implementation of ISQLiteSerializer<DateTime>

		public object Serialize(DateTime value)
		{
			return value.ToFileTimeUtc();
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out DateTime value)
		{
			var fileTimeUtc = reader.GetInt64(valueOrdinal);
			value = DateTime.FromFileTimeUtc(fileTimeUtc);
			return true;
		}

		#endregion
	}
}