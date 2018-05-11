using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// </summary>
	[DataContract]
	public enum CollectionType
	{
		/// <summary>
		///     An unknown collection type.
		///     You will only ever encounter this value when you're reading a future database
		///     where IsabelDb has introduced collections which are not available in this version
		///     of IsabelDb.
		/// </summary>
		[EnumMember] Unknown,

		/// <summary>
		/// </summary>
		[EnumMember] Bag,

		/// <summary>
		/// </summary>
		[EnumMember] Dictionary,

		/// <summary>
		/// </summary>
		[EnumMember] MultiValueDictionary,

		/// <summary>
		/// </summary>
		[EnumMember] IntervalCollection,

		/// <summary>
		/// </summary>
		[EnumMember] OrderedCollection
	}
}