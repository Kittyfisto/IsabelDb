using System;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	[DataContract]
	public struct ValueKey
		: IEquatable<ValueKey>
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
		public ValueKey(long value)
		{
			Value = value;
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(ValueKey other)
		{
			return Value == other.Value;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is ValueKey && Equals((ValueKey) obj);
		}

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
		public static bool operator ==(ValueKey left, ValueKey right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(ValueKey left, ValueKey right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}