using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using ProtoBuf.Meta;

namespace IsabelDb.Serializers
{
	internal sealed class GenericSerializer<T>
		: ISQLiteSerializer<T>
	{
		private readonly int? _typeId;
		private readonly TypeModel _typeModel;
		private readonly TypeStore _typeStore;

		public GenericSerializer(TypeModel typeModel, TypeStore typeStore)
		{
			_typeModel = typeModel;
			_typeStore = typeStore;
			var type = typeof(T);
			if (type.IsSealed)
				_typeId = typeStore.GetOrCreateTypeId(type);

			// We want to perform the warum now so that we don't hit
			// an extreme outlier when actually serializing the value
			// for the first time!
			Warmup();
		}

		/// <summary>
		/// Warms-up the <see cref="TypeModel"/> to serialize values of the given type.
		/// </summary>
		private void Warmup()
		{
			var type = typeof(T);
			if (type.IsValueType)
			{
				Roundtrip(default(T));
			}
			else if (!type.IsAbstract && type != typeof(object))
			{
				Roundtrip(Activator.CreateInstance<T>());
			}
		}

		private void Roundtrip(T value)
		{
			TryDeserialize((byte[]) Serialize(value), out var unused);
		}

		public DbType DatabaseType => DbType.Binary;

		public object Serialize(T value)
		{
			// We might have cached the type id for this type if it's sealed.
			var typeId = _typeId ?? _typeStore.GetOrCreateTypeId(value.GetType());
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(typeId);
				var sw = Stopwatch.StartNew();
				_typeModel.Serialize(stream, value);
				var elapsed = sw.ElapsedMilliseconds;
				return stream.ToArray();
			}
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out T value)
		{
			var serializedValue = (byte[]) reader.GetValue(valueOrdinal);
			return TryDeserialize(serializedValue, out value);
		}

		private bool TryDeserialize(byte[] serializedValue, out T value)
		{
			using (var stream = new MemoryStream(serializedValue))
			using (var tmp = new BinaryReader(stream))
			{
				var typeId = tmp.ReadInt32();
				var type = _typeStore.GetTypeFromTypeId(typeId);
				if (type == null)
				{
					value = default(T);
					return false;
				}

				value = (T) _typeModel.Deserialize(stream, value: null, type: type);
				return true;
			}
		}
	}
}