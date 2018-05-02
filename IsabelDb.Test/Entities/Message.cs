using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract(Name = "Message", Namespace = "IsabelDb.Test.Entities")]
	public sealed class Message
	{
		[DataMember(Order = 1)]
		public object Value { get; set; }
	}
}
