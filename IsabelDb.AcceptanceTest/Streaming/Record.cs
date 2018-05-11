using System.Runtime.Serialization;

namespace IsabelDb.AcceptanceTest.Streaming
{
	[DataContract]
	public sealed class Record
		: IRecord
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public int PseudoId { get; set; }

		[DataMember]
		public double[] MeasurementValues { get; set; }
	}
}