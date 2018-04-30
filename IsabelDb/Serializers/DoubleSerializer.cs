using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class DoubleSerializer
		: ISQLiteSerializer<double>
	{
		#region Implementation of ISQLiteSerializer

		public DbType DatabaseType => DbType.Double;

		#endregion

		#region Implementation of ISQLiteSerializer<double>

		public object Serialize(double value)
		{
			return value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out double value)
		{
			value = reader.GetDouble(valueOrdinal);
			return true;
		}

		#endregion
	}
}