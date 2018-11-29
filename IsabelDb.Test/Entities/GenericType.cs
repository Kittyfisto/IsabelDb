using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class GenericType<T>
	{
		[DataMember]
		public T Value { get; set; }
	}
}
