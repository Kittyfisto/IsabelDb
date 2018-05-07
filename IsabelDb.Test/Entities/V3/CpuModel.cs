using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V3
{
	[DataContract(Namespace = "IsabelDb.Test.Entities")]
	public sealed class CpuModel
	{
		[DataMember]
		public string Name { get; set; }
	}
}