using System.Runtime.Serialization;

namespace IsabelDb.Benchmark
{
	[DataContract]
	public class Customer
	{
		[DataMember(Order = 1)]
		public int Id { get; set; }

		[DataMember(Order = 2)]
		public string Name { get; set; }

		[DataMember(Order = 3)]
		public string[] Phones { get; set; }

		[DataMember(Order = 4)]
		public bool IsActive { get; set; }

		public override string ToString()
		{
			return string.Format("Name: {0}", Name);
		}
	}
}