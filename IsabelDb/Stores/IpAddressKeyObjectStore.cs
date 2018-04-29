using System.Data;
using System.Data.SQLite;
using System.Net;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class IpAddressKeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<IPAddress, TValue>
	{
		public IpAddressKeyObjectStore(SQLiteConnection connection,
		                            TypeModel typeModel,
		                            TypeStore typeStore,
		                            string tableName)
			: base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.Binary;

		protected override string KeyColumnDefinition => "BLOB PRIMARY KEY NOT NULL";

		protected override object SerializeKey(IPAddress key)
		{
			return key.GetAddressBytes();
		}

		protected override IPAddress DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			var addressBytes = (byte[]) reader.GetValue(ordinal);
			return new IPAddress(addressBytes);
		}
	}
}
