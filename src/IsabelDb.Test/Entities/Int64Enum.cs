using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public enum Int64Enum : Int64
	{
		[EnumMember]
		Minimum = Int64.MinValue,

		[EnumMember]
		Maximum = Int64.MaxValue
	}
}
