using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class CustomKey
	{
		[DataMember(Order = 1)] public int A;
		[DataMember(Order = 2)] public int B;
		[DataMember(Order = 3)] public int C;
		[DataMember(Order = 4)] public int D;

		private bool Equals(CustomKey other)
		{
			return A == other.A && B == other.B && C == other.C && D == other.D;
		}

		/// <summary>
		///     This method is implemented to compare two keys within tests.
		///     <see cref="object.Equals(object)" /> is NOT used by IsabelDb.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is CustomKey && Equals((CustomKey) obj);
		}

		/// <summary>
		///     This method is implemented to compare two keys within tests.
		///     <see cref="object.GetHashCode()" /> is NOT used by IsabelDb.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = A;
				hashCode = (hashCode * 397) ^ B;
				hashCode = (hashCode * 397) ^ C;
				hashCode = (hashCode * 397) ^ D;
				return hashCode;
			}
		}
	}
}