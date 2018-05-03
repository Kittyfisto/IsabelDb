using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class Point
	{
		[DataMember]
		public double X;

		[DataMember]
		public double Y;
	}
}