using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract(Name = "Dog", Namespace = "IsabelDb.Test.Entities")]
	public class Dog
		: Animal
	{
	}
}