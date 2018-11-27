using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     This exception is thrown when a type stored in a database could not be resolved.
	/// </summary>
	public class TypeCouldNotBeResolvedException
		: Exception
	{
		/// <summary>
		/// </summary>
		public TypeCouldNotBeResolvedException()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="message"></param>
		public TypeCouldNotBeResolvedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TypeCouldNotBeResolvedException(string message, Exception innerException)
		{
		}
	}
}