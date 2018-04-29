using System.Data;
using System.Data.SQLite;
using System.Net;

namespace IsabelDb.Serializers
{
	internal sealed class IpAddressSerializer
		: ISQLiteSerializer<IPAddress>
	{
		public DbType DatabaseType => DbType.Binary;

		public object Serialize(IPAddress value)
		{
			return value.GetAddressBytes();
		}

		public IPAddress Deserialize(SQLiteDataReader reader, int valueOrdinal)
		{
			var addressBytes = (byte[]) reader.GetValue(valueOrdinal);
			return new IPAddress(addressBytes);
		}
	}
}