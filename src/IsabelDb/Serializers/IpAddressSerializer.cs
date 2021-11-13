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

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out IPAddress value)
		{
			var addressBytes = (byte[]) reader.GetValue(valueOrdinal);
			value = new IPAddress(addressBytes);
			return true;
		}
	}
}