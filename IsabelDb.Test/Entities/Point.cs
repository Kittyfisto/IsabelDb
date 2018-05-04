using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class Point
	{
		[DataMember] public double X;

		[DataMember] public double Y;

		#region Equality members

		private bool Equals(Point other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Point && Equals((Point) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

		#endregion
	}
}