﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace IsabelDb
{
	internal sealed class IsabelDb
		: IDatabase
	{
		private readonly SQLiteConnection _connection;
		private readonly bool _disposeConnection;
		private readonly ObjectStores _objectStores;

		internal IsabelDb(SQLiteConnection connection, IEnumerable<Type> supportedTypes,
			bool disposeConnection,
			bool isReadOnly)
		{
			_connection = connection;
			_disposeConnection = disposeConnection;

			_objectStores = new ObjectStores(connection, supportedTypes.ToList(), isReadOnly);
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
			return new Transaction(_connection.BeginTransaction());
		}

		public IEnumerable<ICollection> Collections => _objectStores.Collections;

		IEnumerable<IReadOnlyCollection> IReadOnlyDatabase.Collections => Collections;

		IReadOnlyBag<T> IReadOnlyDatabase.GetBag<T>(string name)
		{
			return GetBag<T>(name);
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

		public void Remove(ICollection collection)
		{
			_objectStores.Drop(collection);
		}

		public void Remove(string collectionName)
		{
			var collection = Collections.FirstOrDefault(x => string.Equals(x.Name, collectionName));
			if (collection != null)
			{
				Remove(collection);
			}
		}

		/// <inheritdoc />
		public IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name)
		{
			return _objectStores.GetDictionary<TKey, TValue>(name);
		}

		public IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name)
		{
			return _objectStores.GetMultiValueDictionary<TKey, TValue>(name);
		}

		public IPoint2DCollection<T> GetPoint2DCollection<T>(string name)
		{
			return _objectStores.GetPoint2DCollection<T>(name);
		}

		public IIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>
		{
			return _objectStores.GetIntervalCollection<TKey, TValue>(name);
		}

		public IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>
		{
			return _objectStores.GetOrderedCollection<TKey, TValue>(name);
		}

		public IBag<T> GetBag<T>(string name)
		{
			return _objectStores.GetBag<T>(name);
		}

		public IQueue<T> GetQueue<T>(string name)
		{
			return _objectStores.GetQueue<T>(name);
		}
	}
}
