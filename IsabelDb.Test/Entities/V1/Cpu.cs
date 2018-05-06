using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V1
{
	[DataContract(Namespace = "IsabelDb.Test.Entities", Name = "Cpu")]
	public sealed class Cpu
	{
		[DataMember]
		public string Model { get; set; }
	}
}