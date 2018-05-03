using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class Numbers
	{
		[DataMember(Order = 1)]
		public Animal[] Values { get; set; }
	}
}