using System;
using System.Data;
using System.Data.SQLite;
using System.Net;

namespace IsabelDb.Serializers
{
	internal sealed class IpAddressSerializer
		: ISQLiteSerializer<IPAddress>
	{
		public DbType DatabaseType => DbType.Binary;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(IPAddress value, out int typeId)
		{
			typeId = -1;
			return value.GetAddressBytes();
		}

		public IPAddress Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			var addressBytes = (byte[]) reader.GetValue(valueOrdinal);
			return new IPAddress(addressBytes);
		}
	}
}