using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class NullableFloat
	{
		[DataMember]
		public float? Value { get; set; }
	}
}