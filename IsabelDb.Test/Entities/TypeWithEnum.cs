using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class TypeWithEnum
	{
		[DataMember]
		public SomeEnum Value { get; set; }
	}
}
