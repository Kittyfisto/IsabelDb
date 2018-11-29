using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public class SomeClass
	{
		[DataMember]
		public string Value { get; set; }
	}
}