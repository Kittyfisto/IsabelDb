using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     Is thrown when trying to access a collection of <see cref="IReadOnlyDatabase" /> where either
	///     the value- or key type hasn't been registered with the database through <see cref="Database.OpenOrCreate" />
	///     (or similar methods).
	/// </summary>
	public class TypeNotRegisteredException
		: ArgumentException
	{
		/// <summary>
		/// Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public TypeNotRegisteredException(string message)
			: base(message)
		{}
	}
}