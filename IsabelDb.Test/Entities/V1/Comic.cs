using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V1
{
	[DataContract(Name = "Comic", Namespace = "IsabelDb.Test.Entities")]
	public sealed class Comic
	{
		[DataMember(IsRequired = true)]
		public string Name { get; set; }

		[DataMember(IsRequired = true)]
		public string Writer { get; set; }
	}
}