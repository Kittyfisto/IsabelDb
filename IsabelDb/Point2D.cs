using System;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	[DataContract]
	public struct Point2D
		: IEquatable<Point2D>
	{
		#region Equality members

		/// <inheritdoc />
		public bool Equals(Point2D other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is Point2D && Equals((Point2D) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Point2D left, Point2D right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Point2D left, Point2D right)
		{
			return !left.Equals(right);
		}

		#endregion

		/// <summary>
		/// </summary>
		[DataMember] public double X;

		/// <summary>
		/// </summary>
		[DataMember] public double Y;

		/// <summary>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public Point2D(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}", X, Y);
		}
	}
}