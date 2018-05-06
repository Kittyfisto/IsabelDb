using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class NullableInt
	{
		[DataMember]
		public int? Value { get; set; }
	}
}
