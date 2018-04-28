using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class Address
	{
		[DataMember(Order = 2)] public string City;

		[DataMember(Order = 1)] public string Country;

		[DataMember(Order = 4)] public int Number;

		[DataMember(Order = 3)] public string Street;

		private bool Equals(Address other)
		{
			return string.Equals(Country, other.Country) && string.Equals(City, other.City) &&
			       string.Equals(Street, other.Street) && Number == other.Number;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Address && Equals((Address) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Country != null ? Country.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (City != null ? City.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Street != null ? Street.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Number;
				return hashCode;
			}
		}
	}
}