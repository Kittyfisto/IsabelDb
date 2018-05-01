using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     This exception is thrown when a type stored in a database could not be resolved.
	/// </summary>
	public class TypeResolveException
		: Exception
	{
		/// <summary>
		/// </summary>
		public TypeResolveException()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="message"></param>
		public TypeResolveException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TypeResolveException(string message, Exception innerException)
		{
		}
	}
}