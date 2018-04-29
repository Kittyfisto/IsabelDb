using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb.Stores
{
	internal sealed class GenericKeyObjectStore<TKey, TValue>
		: AbstractDictionaryObjectStore<TKey, TValue>
	{
		private readonly TypeModel _typeModel;
		private readonly Type _keyType;

		public GenericKeyObjectStore(SQLiteConnection connection,
		                             TypeModel typeModel,
		                             TypeStore typeStore,
		                             string tableName)
			: base(connection, typeModel, typeStore, tableName)
		{
			_typeModel = typeModel;
			_keyType = typeof(TKey);
		}

		protected override object SerializeKey(TKey key)
		{
			using (var stream = new MemoryStream())
			{
				_typeModel.Serialize(stream, key);
				//stream.Position = 0;
				var serializedKey = stream.ToArray();
				return serializedKey;
			}
		}

		protected override TKey DeserializeKey(SQLiteDataReader reader, int ordinal)
		{
			var serializedKey = (byte[]) reader.GetValue(ordinal);
			using (var stream = new MemoryStream(serializedKey))
			{
				var key = (TKey) _typeModel.Deserialize(stream, null, _keyType);
				return key;
			}
		}

		protected override DbType KeyDatabaseType => DbType.Binary;

		protected override string KeyColumnDefinition => "BLOB PRIMARY KEY NOT NULL";
	}
}