using System.Collections.Generic;
using System.Data.SQLite;
using IsabelDb.Collections;

namespace IsabelDb
{
	internal sealed class Transaction
		: ITransaction
	{
		private readonly IsabelDb _database;
		private readonly SQLiteTransaction _transaction;
		private readonly IReadOnlyList<IInternalCollection> _collections;

		public Transaction(IsabelDb database, SQLiteTransaction transaction, IReadOnlyList<IInternalCollection> collections)
		{
			_database = database;
			_transaction = transaction;
			_collections = collections;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_transaction.Dispose();
		}

		public void Rollback()
		{
			_transaction.Rollback();
			_database.OnRollback(_collections);
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		#endregion
	}
}
