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
		private readonly string _fileName;
		private readonly CollectionsTable _objectStores;
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
			_objectStores = new CollectionsTable(connection, supportedTypes.ToList(), isReadOnly);
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

		public IHashSet<T> GetHashSet<T>(string name)
		{
			return _objectStores.GetHashSet<T>(name);
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

		public void Remove(ICollection collection)
		{
			_objectStores.Drop(collection);
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

		public IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>
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

		#region Overrides of Object

		public override string ToString()
		{
			if (_fileName != null)
				return string.Format("IsabelDb: File '{0}' ({1} collection(s))",
				                     _fileName,
				                     _objectStores.Collections.Count()
				                    );

			return string.Format("IsabelDb: In memory ({0} collection(s))",
			                     _objectStores.Collections.Count());
		}

		#endregion
	}
}