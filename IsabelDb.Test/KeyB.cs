using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class KeyB
		: IPolymorphicCustomKey
	{
		[DataMember(Order = 1)]
		public string Value { get; set; }

		#region Equality members

		private bool Equals(KeyB other)
		{
			return string.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is KeyB && Equals((KeyB) obj);
		}

		public override int GetHashCode()
		{
			return Value != null ? Value.GetHashCode() : 0;
		}

		#endregion
	}
}