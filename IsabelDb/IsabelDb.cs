using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using IsabelDb.Collections;

namespace IsabelDb
{
	internal sealed class IsabelDb
		: IDatabase
	{
		private readonly SQLiteConnection _connection;
		private readonly bool _disposeConnection;
		private readonly string _fileName;
		private readonly CollectionsTable _collections;
		private readonly VariablesTable _variables;

		internal IsabelDb(SQLiteConnection connection,
		                  string fileName,
		                  IEnumerable<Type> supportedTypes,
		                  bool disposeConnection,
		                  bool isReadOnly)
		{
			_connection = connection;
			_fileName = fileName;
			_disposeConnection = disposeConnection;

			_variables = new VariablesTable(connection);
			_collections = new CollectionsTable(connection, supportedTypes.ToList(), isReadOnly);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposeConnection)
			{
				_connection.Close();
				_connection.Dispose();
			}
		}

		public ITransaction BeginTransaction()
		{
			// We pass a COPY of the current list of collections to the transaction so that we may restore
			// collections upon a rollback which have been removed from within the transaction.
			return new Transaction(this, _connection.BeginTransaction(), _collections.Collections.ToList());
		}

		public IEnumerable<ICollection> Collections => _collections.Collections;

		IEnumerable<IReadOnlyCollection> IReadOnlyDatabase.Collections => Collections;

		public IBag<T> CreateBag<T>(string name)
		{
			return _collections.GetBag<T>(name, Mode.Create);
		}

		public IBag<T> GetBag<T>(string name)
		{
			return _collections.GetBag<T>(name, Mode.Get);
		}

		public IHashSet<T> GetHashSet<T>(string name)
		{
			return _collections.GetHashSet<T>(name, Mode.Get);
		}

		public IHashSet<T> CreateHashSet<T>(string name)
		{
			return _collections.GetHashSet<T>(name, Mode.Create);
		}

		public IQueue<T> GetQueue<T>(string name)
		{
			return _collections.GetQueue<T>(name, Mode.Get);
		}

		public IQueue<T> CreateQueue<T>(string name)
		{
			return _collections.GetQueue<T>(name, Mode.Create);
		}

		public IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			return _collections.GetDictionary<TKey, TValue>(name, Mode.Get);
		}

		public IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(string name)
		{
			return _collections.GetDictionary<TKey, TValue>(name, Mode.Create);
		}

		public IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name)
		{
			return _collections.GetMultiValueDictionary<TKey, TValue>(name, Mode.Get);
		}

		public IMultiValueDictionary<TKey, TValue> CreateMultiValueDictionary<TKey, TValue>(string name)
		{
			return _collections.GetMultiValueDictionary<TKey, TValue>(name, Mode.Create);
		}

		public IIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>
		{
			return _collections.GetIntervalCollection<TKey, TValue>(name, Mode.Get);
		}

		public IIntervalCollection<TKey, TValue> CreateIntervalCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>
		{
			return _collections.GetIntervalCollection<TKey, TValue>(name, Mode.Create);
		}

		public IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>
		{
			return _collections.GetOrderedCollection<TKey, TValue>(name, Mode.Get);
		}

		public IOrderedCollection<TKey, TValue> CreateOrderedCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>
		{
			return _collections.GetOrderedCollection<TKey, TValue>(name, Mode.Create);
		}

		public IPoint2DCollection<T> GetPoint2DCollection<T>(string name)
		{
			return _collections.GetPoint2DCollection<T>(name, Mode.Get);
		}

		public IPoint2DCollection<T> CreatePoint2DCollection<T>(string name)
		{
			return _collections.GetPoint2DCollection<T>(name, Mode.Create);
		}

		public IHashSet<T> GetOrCreateHashSet<T>(string name)
		{
			return _collections.GetHashSet<T>(name, Mode.Get | Mode.Create);
		}

		#region IReadOnlyDatabase

		IReadOnlyBag<T> IReadOnlyDatabase.GetBag<T>(string name)
		{
			return GetBag<T>(name);
		}

		IReadOnlyHashSet<T> IReadOnlyDatabase.GetHashSet<T>(string name)
		{
			return GetHashSet<T>(name);
		}

		IReadOnlyQueue<T> IReadOnlyDatabase.GetQueue<T>(string name)
		{
			return GetQueue<T>(name);
		}

		IReadOnlyDictionary<TKey, TValue> IReadOnlyDatabase.GetDictionary<TKey, TValue>(string name)
		{
			return GetDictionary<TKey, TValue>(name);
		}

		IReadOnlyMultiValueDictionary<TKey, TValue> IReadOnlyDatabase.GetMultiValueDictionary<TKey, TValue>(string name)
		{
			return GetMultiValueDictionary<TKey, TValue>(name);
		}

		IReadOnlyIntervalCollection<TKey, TValue> IReadOnlyDatabase.GetIntervalCollection<TKey, TValue>(string name)
		{
			return GetIntervalCollection<TKey, TValue>(name);
		}

		IReadOnlyOrderedCollection<TKey, TValue> IReadOnlyDatabase.GetOrderedCollection<TKey, TValue>(string name)
		{
			return GetOrderedCollection<TKey, TValue>(name);
		}

		IReadOnlyPoint2DCollection<T> IReadOnlyDatabase.GetPoint2DCollection<T>(string name)
		{
			return GetPoint2DCollection<T>(name);
		}

		#endregion

		public void Remove(ICollection collection)
		{
			_collections.Drop(collection);
		}

		public void Remove(string collectionName)
		{
			var collection = Collections.FirstOrDefault(x => string.Equals(x.Name, collectionName));
			if (collection != null) Remove(collection);
		}

		public void Compact()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = "VACUUM";
				command.ExecuteNonQuery();
			}
		}

		/// <inheritdoc />
		public IDictionary<TKey, TValue> GetOrCreateDictionary<TKey, TValue>(string name)
		{
			return _collections.GetDictionary<TKey, TValue>(name, Mode.Get | Mode.Create);
		}

		public IMultiValueDictionary<TKey, TValue> GetOrCreateMultiValueDictionary<TKey, TValue>(string name)
		{
			return _collections.GetMultiValueDictionary<TKey, TValue>(name, Mode.Get | Mode.Create);
		}

		public IPoint2DCollection<T> GetOrCreatePoint2DCollection<T>(string name)
		{
			return _collections.GetPoint2DCollection<T>(name, Mode.Get | Mode.Create);
		}

		public IIntervalCollection<TKey, TValue> GetOrCreateIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>
		{
			return _collections.GetIntervalCollection<TKey, TValue>(name, Mode.Get | Mode.Create);
		}

		public IOrderedCollection<TKey, TValue> GetOrCreateOrderedCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>
		{
			return _collections.GetOrderedCollection<TKey, TValue>(name, Mode.Get | Mode.Create);
		}

		public IBag<T> GetOrCreateBag<T>(string name)
		{
			return _collections.GetBag<T>(name, Mode.Get | Mode.Create);
		}

		public IQueue<T> GetOrCreateQueue<T>(string name)
		{
			return _collections.GetQueue<T>(name, Mode.Get | Mode.Create);
		}

		#region Overrides of Object

		public override string ToString()
		{
			if (_fileName != null)
				return string.Format("IsabelDb: File '{0}' ({1} collection(s))",
				                     _fileName,
				                     _collections.Collections.Count()
				                    );

			return string.Format("IsabelDb: In memory ({0} collection(s))",
			                     _collections.Collections.Count());
		}

		#endregion

		public void OnRollback(IReadOnlyList<IInternalCollection> collectionsBeforeTransaction)
		{
			_collections.OnRollback(collectionsBeforeTransaction);
		}
	}
}