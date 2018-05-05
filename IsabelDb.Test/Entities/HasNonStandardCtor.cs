using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class HasCtor
	{
		public HasCtor()
		{
			StringValue = "Hello";
			IntValue = 42;
			DoubleValue = Math.E;
		}

		[DataMember]
		public string StringValue { get; set; }

		[DataMember]
		public int IntValue { get; set; }

		[DataMember]
		public double DoubleValue { get; set; }
	}
}