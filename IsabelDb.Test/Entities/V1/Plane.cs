using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V1
{
	[DataContract(Name = "Plane", Namespace = "IsabelDb.Test.Entities")]
	public class Plane
		: Thing
	{
	}
}