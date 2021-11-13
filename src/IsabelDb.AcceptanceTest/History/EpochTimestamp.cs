using System;
using System.Runtime.Serialization;

namespace IsabelDb.AcceptanceTest.History
{
	[DataContract]
	public struct EpochTimestamp
		: IComparable<EpochTimestamp>
		, IComparable
	{
		public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public static readonly EpochTimestamp MinValue = new EpochTimestamp {Value = 0};
		public static readonly EpochTimestamp MaxValue = new EpochTimestamp {Value = long.MaxValue};

		#region Relational members

		public int CompareTo(EpochTimestamp other)
		{
			return Value.CompareTo(other.Value);
		}

		public int CompareTo(object obj)
		{
			if (ReferenceEquals(null, obj)) return 1;
			if (!(obj is EpochTimestamp)) throw new ArgumentException($"Object must be of type {nameof(EpochTimestamp)}");
			return CompareTo((EpochTimestamp) obj);
		}

		public static bool operator <(EpochTimestamp left, EpochTimestamp right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(EpochTimestamp left, EpochTimestamp right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(EpochTimestamp left, EpochTimestamp right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(EpochTimestamp left, EpochTimestamp right)
		{
			return left.CompareTo(right) >= 0;
		}

		#endregion

		private EpochTimestamp(DateTime value)
		{
			var tmp = value.ToUniversalTime();
			Value = (long) (tmp - Epoch).TotalSeconds;
		}

		private DateTime ToDateTime()
		{
			return Epoch + TimeSpan.FromSeconds(Value);
		}

		[DataMember]
		public long Value { get; set; }

		public static EpochTimestamp operator +(EpochTimestamp value, int seconds)
		{
			return new EpochTimestamp {Value = value.Value + seconds};
		}

		public static EpochTimestamp operator -(EpochTimestamp value, int seconds)
		{
			return new EpochTimestamp {Value = value.Value - seconds};
		}

		public static EpochTimestamp operator --(EpochTimestamp value)
		{
			return new EpochTimestamp {Value = value.Value - 1};
		}

		public static EpochTimestamp operator ++(EpochTimestamp value)
		{
			return new EpochTimestamp {Value = value.Value + 1};
		}

		public static explicit operator EpochTimestamp(DateTime value)
		{
			return new EpochTimestamp(value);
		}

		public static explicit operator DateTime (EpochTimestamp value)
		{
			return value.ToDateTime();
		}
	}
}