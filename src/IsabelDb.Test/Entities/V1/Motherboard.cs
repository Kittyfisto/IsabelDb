using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V1
{
	[DataContract(Namespace = "IsabelDb.Test.Entities", Name = "Motherboard")]
	public sealed class Motherboard
	{
		[DataMember]
		public List<Cpu> Cpus { get; set; }
	}
}
