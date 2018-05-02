using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     This exception is thrown when a breaking change to a data contract type has been detected.
	///     This usually occurs when opening an existing database and the type model used to write
	///     that database **significantly** differs from the type model when the database is now opened.
	/// </summary>
	public class BreakingChangeException
		: Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public BreakingChangeException()
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public BreakingChangeException(string message)
			: base(message)
		{}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public BreakingChangeException(string message, Exception innerException)
			: base(message, innerException)
		{}
	}
}