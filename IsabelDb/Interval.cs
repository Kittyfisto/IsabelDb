using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	public static class Interval
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns></returns>
		public static Interval<T> Create<T>(T minimum, T maximum) where T : IComparable<T>
		{
			return new Interval<T>(minimum, maximum);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Interval<T> Create<T>(T value) where T : IComparable<T>
		{
			return new Interval<T>(value, value);
		}
	}

	/// <summary>
	///     Defines an interval [Minimum, Maximum].
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DataContract]
	public struct Interval<T>
		: IEquatable<Interval<T>> where T : IComparable<T>
	{
		#region Equality members

		/// <inheritdoc />
		public bool Equals(Interval<T> other)
		{
			return EqualityComparer<T>.Default.Equals(Minimum, other.Minimum) &&
			       EqualityComparer<T>.Default.Equals(Maximum, other.Maximum);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is Interval<T> && Equals((Interval<T>) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T>.Default.GetHashCode(Minimum) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Maximum);
			}
		}

		/// <summary>
		///     Compares two intervals for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Interval<T> left, Interval<T> right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares two intervals for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Interval<T> left, Interval<T> right)
		{
			return !left.Equals(right);
		}

		#endregion

		/// <summary>
		/// </summary>
		[DataMember] public T Minimum;

		/// <summary>
		/// </summary>
		[DataMember] public T Maximum;

		/// <summary>
		/// </summary>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		public Interval(T minimum, T maximum)
		{
			if (minimum.CompareTo(maximum) > 0)
			{
				Minimum = maximum;
				Maximum = minimum;
			}
			else
			{
				Minimum = minimum;
				Maximum = maximum;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Pure]
		public bool Contains(T value)
		{
			if (Minimum.CompareTo(value) > 0)
				return false;

			if (Maximum.CompareTo(value) < 0)
				return false;

			return true;
		}

		#region Overrides of ValueType

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("[{0}, {1}]", Minimum, Maximum);
		}

		#endregion
	}
}