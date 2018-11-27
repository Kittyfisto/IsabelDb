using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///   Is thrown when trying to access a collection of <see cref="IReadOnlyDatabase"/> which doesn't exist.
	/// </summary>
	public class NoSuchCollectionException
		: ArgumentException
	{
		/// <summary>
		/// Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public NoSuchCollectionException(string message)
			: base(message)
		{}
	}
}
