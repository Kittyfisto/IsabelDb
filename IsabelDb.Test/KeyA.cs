using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class KeyA
		: IPolymorphicCustomKey
	{
		[DataMember(Order = 1)]
		public string Value { get; set; }

		#region Equality members

		private bool Equals(KeyA other)
		{
			return string.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is KeyA && Equals((KeyA) obj);
		}

		public override int GetHashCode()
		{
			return Value != null ? Value.GetHashCode() : 0;
		}

		#endregion
	}
}