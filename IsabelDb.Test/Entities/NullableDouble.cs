using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class NullableDouble
	{
		[DataMember]
		public double? Value { get; set; }
	}
}