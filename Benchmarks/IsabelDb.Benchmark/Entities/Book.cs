using System.Runtime.Serialization;

namespace IsabelDb.Benchmark.Entities
{
	[DataContract(Name = "Book", Namespace = "IsabelDb.Entities")]
	public sealed class Book
	{
		[DataMember(Order = 1)]
		public string Title { get; set; }

		[DataMember(Order = 2)]
		public string Author { get; set; }
	}
}
