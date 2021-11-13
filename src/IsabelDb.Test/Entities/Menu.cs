using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract]
	public sealed class Menu
	{
		[DataMember]
		public bool IsVisible { get; set; }
	}
}