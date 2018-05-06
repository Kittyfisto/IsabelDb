using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class ByteArray
	{
		[DataMember]
		public byte[] Values { get; set; }
	}
}