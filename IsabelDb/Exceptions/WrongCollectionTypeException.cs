using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     Is thrown when trying to access a collection of a <see cref="IReadOnlyDatabase" /> which is not of the expected
	///     <see cref="CollectionType" />.
	///     For example, the collection may have been created using <see cref="IDatabase.GetBag{T}" /> and now it's being
	///     retrieved via <see cref="IDatabase.GetQueue{T}" />.
	/// </summary>
	public class WrongCollectionTypeException
		: ArgumentException
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public WrongCollectionTypeException(string message)
			: base(message)
		{
		}
	}
}