using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public enum UInt64Enum : UInt64
	{
		[EnumMember]
		Minimum = UInt64.MinValue,

		[EnumMember]
		Maximum = UInt64.MaxValue
	}
}