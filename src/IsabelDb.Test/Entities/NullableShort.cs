using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class NullableShort
	{
		[DataMember]
		public short? Value { get; set; }
	}
}