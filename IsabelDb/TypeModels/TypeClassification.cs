using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	/// <summary>
	///     Keeps track of whether the type described by a <see cref="TypeDescription" /> is a class,
	///     struct, interface or enum.
	/// </summary>
	[DataContract]
	internal enum TypeClassification
	{
		/// <summary>
		/// The type is a class (a reference type).
		/// </summary>
		Class = 0,

		/// <summary>
		/// The type is an interface (a reference type).
		/// </summary>
		Interface = 1,

		/// <summary>
		/// The type is a struct (a value type).
		/// </summary>
		Struct = 2,

		/// <summary>
		/// The type is an enum (a value type).
		/// </summary>
		Enum = 3
	}
}