using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	[DataContract]
	public struct Point2D
	{
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