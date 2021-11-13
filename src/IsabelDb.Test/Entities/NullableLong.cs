using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class NullableLong
	{
		[DataMember]
		public long? Value { get; set; }
	}
}