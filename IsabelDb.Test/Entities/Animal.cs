using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract(Name = "Animal", Namespace = "IsabelDb.Test.Entities")]
	public abstract class Animal
	{
		[DataMember(Order = 1)]
		public string Name { get; set; }

		[DataMember]
		public int Age { get; set; }
	}
}