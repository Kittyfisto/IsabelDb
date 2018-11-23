using System.Runtime.Serialization;

namespace IsabelDb.Browser.Test
{
	[DataContract]
	public class DataObject
	{
		[DataMember]
		public string Property { get; set; }

		[DataMember]
		public object Value;
	}
}