using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class IntList
	{
		[DataMember]
		public List<int> Values { get; set; }
	}
}
