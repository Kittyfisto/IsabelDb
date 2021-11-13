using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V3
{
	[DataContract(Namespace = "IsabelDb.Test.Entities")]
	public sealed class Cpu
	{
		/// <summary>
		/// Changes to the datatype are not allowed
		/// </summary>
		[DataMember]
		public CpuModel Model { get; set; }
	}
}
