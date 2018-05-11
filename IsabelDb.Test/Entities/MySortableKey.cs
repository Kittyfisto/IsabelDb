using System;
using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class MySortableKey : IComparable<MySortableKey>
	{
		public int CompareTo(MySortableKey other)
		{
			throw new NotImplementedException();
		}
	}
}