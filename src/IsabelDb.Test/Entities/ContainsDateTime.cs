using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public class ContainsDateTime
	{
		[DataMember]
		public DateTime Value;
	}
}
