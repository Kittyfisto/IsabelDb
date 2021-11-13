using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V2
{
	[DataContract(Namespace = "IsabelDb.Test.Entities", Name = "Motherboard")]
	public sealed class Motherboard
	{
		[DataMember]
		public List<V1.Cpu> Cpus { get; set; }
	}
}
