using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public class ContainsGuid
	{
		[DataMember]
		public Guid Value;
	}
}