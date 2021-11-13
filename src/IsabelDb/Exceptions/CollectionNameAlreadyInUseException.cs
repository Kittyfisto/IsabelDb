using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///    Is thrown when trying to create a new collection with the same name as an already existing one.
	///    If the intention is to create a truly new collection, then a different name has to be chosen.
	///    If the intention is to get an existing collection, then either GetXYZ() or GetOrCreateXYZ() need to be used
	///    to retrieve a previously created collection.
	/// </summary>
	public class CollectionNameAlreadyInUseException
		: ArgumentException
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		public CollectionNameAlreadyInUseException()
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public CollectionNameAlreadyInUseException(string message)
			: base(message)
		{}
	}
}