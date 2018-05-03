using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract(Name = "Dog", Namespace = "IsabelDb.Test.Entities")]
	public class Dog
		: Animal
	{
		[DataMember]
		public string EyeColor;

		[DataMember]
		public string FurColor { get; set; }
	}
}