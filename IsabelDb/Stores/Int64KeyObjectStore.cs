using System;
using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class Int64KeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<Int64, TValue>
	{
		public Int64KeyObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore, string tableName) :
			base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.Int64;

		protected override string KeyColumnDefinition => "INTEGER PRIMARY KEY NOT NULL";

		protected override object SerializeKey(Int64 key)
		{
			return key;
		}

		protected override Int64 DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			return reader.GetInt64(ordinal);
		}
	}
}