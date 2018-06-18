using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	[DataContract]
	public struct Point2D
		: IEquatable<Point2D>
	{
		static Point2D()
		{
			Zero = new Point2D();
		}

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		[Pure]
		public static double SquaredDistance(Point2D lhs, Point2D rhs)
		{
			var x = lhs.X - rhs.X;
			var y = lhs.Y - rhs.Y;
			return x * x + y * y;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns></returns>
		[Pure]
		public static double Distance(Point2D lhs, Point2D rhs)
		{
			return Math.Sqrt(SquaredDistance(lhs, rhs));
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly Point2D Zero;
	}
}