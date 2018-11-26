using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	public interface IDatabase
		: IReadOnlyDatabase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		ITransaction BeginTransaction();

		/// <summary>
		///     Provides access to the list of collections in this database.
		/// </summary>
		new IEnumerable<ICollection> Collections { get; }

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		new IBag<T> GetBag<T>(string name);

		/// <summary>
		///     Returns an object store in which acts as a queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		new IQueue<T> GetQueue<T>(string name);

		/// <summary>
		///     Returns an object store in which each object is identified by a key of the given type
		///     <typeparamref name="TKey" />.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		new IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name);

		/// <summary>
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		new IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Gets or creates a collection with the given name.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		new IIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;

		/// <summary>
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		new IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>;

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		new IPoint2DCollection<T> GetPoint2DCollection<T>(string name);

		/// <summary>
		///     Drops the given collection from this database.
		///     Doesn't do anything when this collection isn't part of this database.
		/// </summary>
		/// <remarks>
		///     THIS OPERATION IMMEDIATELY REMOVES ALL OBJECTS FROM THE GIVEN COLLECTION FROM STORAGE.
		///     THIS OPERATION IS NOT REVERSIBLE.
		///     USE WITH CARE.
		/// </remarks>
		/// <param name="collection"></param>
		void Remove(ICollection collection);
	}
}