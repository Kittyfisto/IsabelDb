using System;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	public struct ValueKey
		: IEquatable<ValueKey>
	{
		private readonly long _value;

		/// <summary>
		///     The numeric value of this key: Two keys with the same value are equal.
		/// </summary>
		public long Value => _value;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public ValueKey(long value)
		{
			_value = value;
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(ValueKey other)
		{
			return _value == other._value;
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
			return _value.GetHashCode();
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