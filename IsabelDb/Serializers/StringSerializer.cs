using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class StringSerializer
		: ISQLiteSerializer<string>
	{
		public DbType DatabaseType => DbType.String;

		public object Serialize(string value)
		{
			return value;
		}

		public string Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			return reader.GetString(valueOrdinal);
		}
	}
}