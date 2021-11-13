using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public class ClassWithObject
	{
		[DataMember]
		public object Value { get; set; }
	}
}