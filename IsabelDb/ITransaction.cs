using System;

namespace IsabelDb
{
	/// <summary>
	///     Represents a transaction on the database.
	///     Values will not be persisted on-disk until <see cref="Commit" />
	///     is called.
	/// </summary>
	public interface ITransaction
		: IDisposable
	{
		/// <summary>
		/// </summary>
		void Commit();
	}
}