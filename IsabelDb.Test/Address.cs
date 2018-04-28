using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract]
	public sealed class Address
	{
		[DataMember(Order = 1)]
		public string Country;
		[DataMember(Order = 2)]
		public string City;
		[DataMember(Order = 3)]
		public string Street;
		[DataMember(Order = 4)]
		public int Number;
	}
}