using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V2
{
	[DataContract(Name = "Comic", Namespace = "IsabelDb.Test.Entities")]
	public sealed class Comic
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Writer { get; set; }

		[DataMember]
		public string Artist { get; set; }
	}
}