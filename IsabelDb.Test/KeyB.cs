using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class KeyB
		: IPolymorphicCustomKey
	{
		#region Equality members

		private bool Equals(KeyB other)
		{
			return string.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is KeyB && Equals((KeyB) obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		#endregion

		[DataMember]
		public string Value { get; set; }
	}
}