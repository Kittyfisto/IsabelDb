using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public struct SomeStruct
	{
		[DataMember]
		public string Value { get; set; }
	}
}
