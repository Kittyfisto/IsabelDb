using System;
using System.Data.SQLite;
using System.IO;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for serializing / deserializing object graphs.
	/// </summary>
	internal sealed class Serializer
	{
		private readonly ITypeModel _typeModel;
		private readonly ProtoBuf.Meta.TypeModel _protobufSerializer;

		public Serializer(ITypeModel typeModel)
		{
			_typeModel = typeModel;
			_protobufSerializer = _typeModel.Serializer;
		}

		public byte[] Serialize(object value)
		{
			// We might have cached the type id for this type if it's sealed.
			var type = value.GetType();
			var typeId = _typeModel.GetTypeId(type);

			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(typeId);
				try
				{
					_protobufSerializer.Serialize(stream, value);
				}
				catch (InvalidOperationException e)
				{
					throw new ArgumentException(string.Format("Unable to serialize value: {0}",
					                                          e.Message));
				}

				return stream.ToArray();
			}
		}

		public object Deserialize(byte[] serializedValue)
		{
			using (var stream = new MemoryStream(serializedValue))
			using (var tmp = new BinaryReader(stream))
			{
				var typeId = tmp.ReadInt32();
				var type = _typeModel.GetType(typeId);
				if (type == null)
				{
					return null;
				}

				return _protobufSerializer.Deserialize(stream, null, type);
			}
		}

		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return IsabelDb.TableExists(connection, TypeModel.TypeTableName);
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "id INTEGER NOT NULL," +
				                                    "typename TEXT NOT NULL" +
				                                    ")", TypeModel.TypeTableName);
				command.ExecuteNonQuery();
			}
		}
	}
}