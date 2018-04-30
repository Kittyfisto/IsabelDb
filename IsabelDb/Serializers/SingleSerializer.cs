using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class SingleSerializer
		: ISQLiteSerializer<float>
	{
		#region Implementation of ISQLiteSerializer

		public DbType DatabaseType => DbType.Single;

		#endregion

		#region Implementation of ISQLiteSerializer<float>

		public object Serialize(float value)
		{
			return value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out float value)
		{
			value = reader.GetFloat(valueOrdinal);
			return true;
		}

		#endregion
	}
}
