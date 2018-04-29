using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class Int16KeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<short, TValue>
	{
		public Int16KeyObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore, string tableName) :
			base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.Int16;

		protected override string KeyColumnDefinition => "INTEGER PRIMARY KEY NOT NULL";

		protected override object SerializeKey(short key)
		{
			return key;
		}

		protected override short DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			return reader.GetInt16(ordinal);
		}
	}
}