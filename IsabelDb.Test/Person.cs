using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class Person
	{
		private bool Equals(Person other)
		{
			return Id == other.Id && string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Person && Equals((Person) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Id * 397) ^ (Name != null ? Name.GetHashCode() : 0);
			}
		}

		[DataMember(Order = 1)]
		public int Id { get; set; }

		[DataMember(Order = 2)]
		public string Name { get; set; }
	}
}