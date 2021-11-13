using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class TypeWithEnum
	{
		[DataMember]
		public Int32Enum Value { get; set; }
	}
}
