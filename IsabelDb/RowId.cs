using System;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	///    Holds the id of a specific row in a specific collection.
	/// </summary>
	[DataContract]
	public struct RowId
		: IEquatable<RowId>
		, IComparable<RowId>
	{
		/// <summary>
		///     The numeric value of this key: Two keys with the same value are equal.
		/// </summary>
		[DataMember]
		public long Value { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public RowId(long value)
		{
			Value = value;
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(RowId other)
		{
			return Value == other.Value;
		}

		/// <inheritdoc />
		public int CompareTo(RowId other)
		{
			return Value.CompareTo(other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is RowId && Equals((RowId) obj);
		}

		#region Overrides of ValueType

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("#{0}", Value);
		}

		#endregion

		#region Equality members

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion

		/// <summary>
		///     Compares two values for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(RowId left, RowId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(RowId left, RowId right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}