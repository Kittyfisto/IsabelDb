using System;

namespace IsabelDb
{
	/// <summary>
	/// This exception is thrown when there's a mismatch between a type stored in the database
	/// and the type which the application claims which should be stored there.
	/// </summary>
	public class TypeMismatchException
		: Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public TypeMismatchException()
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public TypeMismatchException(string message)
			: base(message)
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public TypeMismatchException(string message, Exception innerException)
		{ }
	}
}
