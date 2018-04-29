using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class StringSerializer
		: ISQLiteSerializer<string>
	{
		public DbType DatabaseType => DbType.String;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(string value, out int typeId)
		{
			typeId = -1;
			return value;
		}

		public string Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			return reader.GetString(valueOrdinal);
		}
	}
}