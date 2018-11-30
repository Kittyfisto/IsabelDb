using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	/// </summary>
	public class IncompatibleDatabaseSchemaException
		: Exception
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		public IncompatibleDatabaseSchemaException()
		{}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public IncompatibleDatabaseSchemaException(string message)
			: base(message)
		{}
	}
}