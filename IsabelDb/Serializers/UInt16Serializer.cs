using System;
using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	internal sealed class UInt16Serializer
		: ISQLiteSerializer<ushort>
	{
		public DbType DatabaseType => DbType.UInt16;

		public bool StorePerValueTypeInformation => false;

		public object Serialize(ushort value, out int typeId)
		{
			typeId = -1;
			return (int)value;
		}

		public ushort Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeId)
		{
			var rawValue = reader.GetInt32(valueOrdinal);
			return unchecked((ushort) rawValue);
		}
	}
}