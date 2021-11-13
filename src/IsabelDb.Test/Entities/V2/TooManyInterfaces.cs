using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities.V2
{
	[DataContract(Name = "TooManyInterfaces", Namespace = "IsabelDb.Test.Entities")]
	public sealed class TooManyInterfaces
		: ISerializableType2
		, IPolymorphicCustomKey
	{
	}
}