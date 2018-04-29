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
			// Any sane person would expect that if a column is defined
			// as STRING then reader.GetString() returns the string we stored.
			// But obviously this would be WAY too easy to work with, so the sqlite
			// team ensured that life remains spicy. If you call GetString() on a field
			// which happens to store a numeric string value, such as "2" then GetString()
			// obviously throws an InvalidCastException, because why wouldn't it.
			return (string)reader.GetValue(valueOrdinal);
		}
	}
}