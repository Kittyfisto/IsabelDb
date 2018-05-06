using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class IntArray
	{
		[DataMember]
		public int[] Values { get; set; }
	}
}