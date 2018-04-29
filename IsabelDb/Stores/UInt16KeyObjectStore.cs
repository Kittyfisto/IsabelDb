using System.Data;
using System.Data.SQLite;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class UInt16KeyObjectStore<TValue>
		: AbstractDictionaryObjectStore<ushort, TValue>
	{
		public UInt16KeyObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore, string tableName) :
			base(connection, typeModel, typeStore, tableName)
		{
		}

		protected override DbType KeyDatabaseType => DbType.UInt16;

		protected override string KeyColumnDefinition => "INTEGER PRIMARY KEY NOT NULL";

		protected override object SerializeKey(ushort key)
		{
			return (int)key;
		}

		protected override ushort DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			return (ushort)reader.GetInt32(ordinal);
		}
	}
}