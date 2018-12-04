using System;

namespace IsabelDb
{
	/// <summary>
	///     Represents a transaction on the database.
	///     Values will not be persisted on-disk until <see cref="Commit" />
	///     is called.
	/// </summary>
	/// <remarks>
	///     There can only ever exist one transaction per <see cref="IDatabase"/> and process.
	///     This means that using transactions and multiple parallel writing threads is generally not
	///     possible and will cause mutating methods to throw <see cref="InvalidOperationException"/>s!
	/// </remarks>
	public interface ITransaction
		: IDisposable
	{
		/// <summary>
		///     Rolls back the transaction.
		/// </summary>
		/// <remarks>
		///     A transaction is rolled back automatically if <see cref="IDisposable.Dispose" /> is called without
		///     ever having called <see cref="Commit" /> on it. This method nevertheless exists for those few
		///     occasions where 'transaction.Rollback();' is more readable than not calling rollback in the first place.
		/// </remarks>
		void Rollback();

		/// <summary>
		/// </summary>
		void Commit();
	}
}