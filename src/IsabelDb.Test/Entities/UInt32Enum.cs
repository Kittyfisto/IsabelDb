using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public enum UInt32Enum : UInt32
	{
		[EnumMember] Minimum = UInt32.MinValue,

		[EnumMember] Maximum = UInt32.MaxValue
	}
}