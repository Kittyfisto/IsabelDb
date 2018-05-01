using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class ByteArraySerializer
		: ISQLiteSerializer<byte[]>
	{
		#region Implementation of ISQLiteSerializer

		public DbType DatabaseType => DbType.Binary;

		#endregion

		#region Implementation of ISQLiteSerializer<byte[]>

		public object Serialize(byte[] value)
		{
			return value;
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out byte[] value)
		{
			value = (byte[]) reader.GetValue(valueOrdinal);
			return true;
		}

		#endregion
	}
}
