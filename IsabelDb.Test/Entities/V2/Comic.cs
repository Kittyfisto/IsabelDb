using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V2
{
	[DataContract(Name = "Comic", Namespace = "IsabelDb.Test.Entities")]
	public sealed class Comic
	{
		[DataMember(IsRequired = true)]
		public string Name { get; set; }

		[DataMember(IsRequired = true)]
		public string Writer { get; set; }

		[DataMember(IsRequired = false)]
		public string Artist { get; set; }
	}
}