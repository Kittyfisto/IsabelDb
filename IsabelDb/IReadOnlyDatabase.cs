using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the database, but nothing can be modified.
	///     Use <see cref="IDatabase" /> if you need to add/remove/modify collections, values, etc...
	/// </remarks>
	public interface IReadOnlyDatabase
		: IDisposable
	{
		/// <summary>
		///     Provides access to the list of collections in this database.
		/// </summary>
		IEnumerable<IReadOnlyCollection> Collections { get; }

		/// <summary>
		///      Returns a "bag" collection which has previously been created with
		///      <see cref="IDatabase.GetBag{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IReadOnlyBag<T> GetBag<T>(string name);

		/// <summary>
		///     Returns an object store in which each object is identified by a key of the given type
		///     <typeparamref name="TKey" />.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IReadOnlyDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name);

		/// <summary>
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		IReadOnlyMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Gets or creates a collection with the given name.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		IReadOnlyIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;

		/// <summary>
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		IReadOnlyOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;
	}
}