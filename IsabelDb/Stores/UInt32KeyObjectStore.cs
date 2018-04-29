using System;
using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class UInt32KeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<uint, TValue>
	{
		public UInt32KeyObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore, string tableName) :
			base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.Int64;

		protected override string KeyColumnDefinition => "INTEGER PRIMARY KEY NOT NULL";

		protected override object SerializeKey(UInt32 key)
		{
			return (long)key;
		}

		protected override UInt32 DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			var rawValue = reader.GetInt64(ordinal);
			return unchecked((uint) rawValue);
		}
	}
}