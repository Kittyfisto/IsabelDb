using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb
{
	internal sealed class ObjectStore
		: IObjectStore
	{
		private readonly SQLiteConnection _connection;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

		public ObjectStore(SQLiteConnection connection, TypeModel typeModel, TypeStore typeStore)
		{
			_connection = connection;
			_typeModel = typeModel;
			_typeStore = typeStore;
		}

		public IEnumerable<KeyValuePair<string, object>> GetAll()
		{
			return GetAll<object>();
		}

		public IEnumerable<KeyValuePair<string, T>> GetAll<T>()
		{
			using (var command = CreateCommand("SELECT key, type, value FROM objects"))
			{
				return Get<T>(command);
			}
		}

		public IEnumerable<KeyValuePair<string, T>> Get<T>(IEnumerable<string> keys)
		{
			using (var command = CreateCommand("SELECT type, value FROM objects WHERE key = @key"))
			{
				var keyParameter = command.Parameters.Add("@key", DbType.String);
				var ret = new List<KeyValuePair<string, T>>();
				foreach (var key in keys)
				{
					keyParameter.Value = key;
					using (var reader = command.ExecuteReader())
					{
						if (TryReadValue<T>(reader, out var value))
						{
							ret.Add(new KeyValuePair<string, T>(key, value));
						}
					}
				}

				return ret;
			}
		}

		public IEnumerable<KeyValuePair<string, T>> Get<T>(params string[] keys)
		{
			return Get<T>((IEnumerable<string>) keys);
		}

		public IEnumerable<KeyValuePair<string, object>> Get(params string[] keys)
		{
			return Get((IEnumerable<string>) keys);
		}

		public IEnumerable<KeyValuePair<string, object>> Get(IEnumerable<string> keys)
		{
			return Get<object>(keys);
		}

		public T Get<T>(string key)
		{
			using (var command = CreateCommand("SELECT type, value FROM objects WHERE key = @key"))
			{
				command.Parameters.AddWithValue("@key", key);
				using (var reader = command.ExecuteReader())
				{
					TryReadValue<T>(reader, out var value);
					return value;
				}
			}
		}

		public object Get(string key)
		{
			return Get<object>(key);
		}

		public void Put(string key, object value)
		{
			var serializedValue = Serialize(value);
			using (var transaction = BeginTransaction())
			{
				var typeId = _typeStore.GetOrCreateTypeId(value.GetType());

				using (var command = _connection.CreateCommand())
				{
					command.CommandText = "INSERT OR REPLACE INTO objects (key, type, value) VALUES" +
					                      "(@key, @typeId, @value)";
					command.Parameters.AddWithValue("@key", key);
					command.Parameters.AddWithValue("@typeId", typeId);
					command.Parameters.AddWithValue("@value", serializedValue);

					command.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

		public void Put(IEnumerable<KeyValuePair<string, object>> values)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<KeyValuePair<string, T>> Get<T>(SQLiteCommand command)
		{
			using (var reader = command.ExecuteReader())
			{
				var ret = new List<KeyValuePair<string, T>>();
				while (reader.Read())
				{
					var key = reader.GetString(0);
					var typeId = reader.GetInt32(1);
					var type = _typeStore.GetTypeFromTypeId(typeId);
					if (type != null)
					{
						var serializedValue = (byte[]) reader.GetValue(2);
						var value = Deserialize(type, serializedValue);

						if (value is T desiredValue)
						{
							ret.Add(new KeyValuePair<string, T>(key, desiredValue));
						}
					}
				}
				return ret;
			}
		}

		private bool TryReadValue<T>(SQLiteDataReader reader, out T value)
		{
			if (!reader.Read())
			{
				value = default(T);
				return false;
			}

			var typeId = reader.GetInt32(0);
			var type = _typeStore.GetTypeFromTypeId(typeId);
			if (type == null)
			{
				value = default(T);
				return false;
			}

			var serializedValue = (byte[]) reader.GetValue(1);
			var deserializedValue = Deserialize(type, serializedValue);
			if (!(deserializedValue is T))
			{
				value = default(T);
				return false;
			}

			value = (T) deserializedValue;
			return true;
		}

		private SQLiteCommand CreateCommand(string text)
		{
			var command = _connection.CreateCommand();
			command.CommandText = text;
			return command;
		}

		private SQLiteTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		private byte[] Serialize(object value)
		{
			using (var stream = new MemoryStream())
			{
				_typeModel.Serialize(stream, value);
				return stream.ToArray();
			}
		}

		private object Deserialize(Type type, byte[] serializedValue)
		{
			using (var stream = new MemoryStream(serializedValue))
			{
				var value = _typeModel.Deserialize(stream, null, type);
				return value;
			}
		}
	}
}