using System.Data.SQLite;

namespace IsabelDb
{
	internal sealed class Transaction
		: ITransaction
	{
		private readonly SQLiteTransaction _transaction;

		public Transaction(SQLiteTransaction transaction)
		{
			_transaction = transaction;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_transaction.Dispose();
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		#endregion
	}
}
