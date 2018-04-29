using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class StringKeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<string, TValue>
	{
		public StringKeyObjectStore(SQLiteConnection connection,
		                            TypeModel typeModel,
		                            TypeStore typeStore,
		                            string tableName)
			: base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.String;

		protected override string KeyColumnDefinition => "STRING PRIMARY KEY NOT NULL";

		protected override object SerializeKey(string key)
		{
			return key;
		}

		protected override string DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			return reader.GetString(ordinal);
		}
	}
}