using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class Int32KeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<int, TValue>
	{
		public Int32KeyObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore, string tableName) :
			base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.Int32;

		protected override string KeyColumnDefinition => "INTEGER PRIMARY KEY NOT NULL";

		protected override object SerializeKey(int key)
		{
			return key;
		}

		protected override int DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			return reader.GetInt32(ordinal);
		}
	}
}