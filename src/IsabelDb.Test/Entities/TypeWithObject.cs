using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class TypeWithObject
	{
		[DataMember]
		public object Value { get; set; }
	}
}
