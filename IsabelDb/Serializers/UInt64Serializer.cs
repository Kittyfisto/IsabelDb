using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;

namespace IsabelDb.Serializers
{
	internal sealed class UInt64Serializer
		: ISQLiteSerializer<ulong>
	{
		/// <summary>
		/// </summary>
		/// <remarks>
		///     We're storing <see cref="long" /> values internally because SQLite doesn't
		///     support ulong.
		/// </remarks>
		public DbType DatabaseType => DbType.Int64;

		public object Serialize(ulong value)
		{
			return ToDatabase(value);
		}

		public bool TryDeserialize(SQLiteDataReader reader, int valueOrdinal, out ulong value)
		{
			var mapped = reader.GetInt64(valueOrdinal);
			value = FromDatabase(mapped);
			return true;
		}

		[Pure]
		public static long ToDatabase(ulong value)
		{
			// SQLite doesn't support uint64, therefore we have to map these
			// values to int64. We just need to be careful to find a mapping
			// which preserves the requirements we have for numbers in this database:
			// if a != b then a' != b'
			// if a > b then a' > b'
			// where a and b are uint64 values and a' b' are their mapped int64 values.
			// The easiest way to preserve those is to subtract a constant from their ulong counterparts
			var mapped = unchecked((long) (value - long.MaxValue - 1));
			return mapped;
		}

		[Pure]
		public static ulong FromDatabase(long mapped)
		{
			var value = unchecked((ulong) (mapped + long.MaxValue + 1));
			return value;
		}
	}
}