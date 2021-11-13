using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///     This exception is thrown when there's a mismatch between a type stored in the database
	///     and the type which the application claims which should be stored there.
	/// </summary>
	public class TypeMismatchException
		: Exception
	{
		/// <summary>
		///     Initializes this object.
		/// </summary>
		public TypeMismatchException()
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		public TypeMismatchException(string message)
			: base(message)
		{
		}

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TypeMismatchException(string message, Exception innerException)
		{
		}
	}
}