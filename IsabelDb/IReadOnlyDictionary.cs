using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     A collection which maps values to keys: There can only ever be one value per key.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the collection.
	///     If you need to modify it, please use <see cref="IDictionary{TKey,TValue}" />.
	/// </remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyDictionary<TKey, TValue>
		: IReadOnlyCollection<TValue>
	{
		/// <summary>
		///    Tests if there is a value with the given key in this collection.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Pure]
		bool ContainsKey(TKey key);

		/// <summary>
		///     Tries to retrieve the value with the given key.
		///     Returns true if the value was retrieved, false otherwise.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGet(TKey key, out TValue value);

		/// <summary>
		///     Retrieves the value with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		TValue Get(TKey key);

		/// <summary>
		///     Retrieves all values from this database.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> GetAll();

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> GetMany(IEnumerable<TKey> keys);

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> GetMany(params TKey[] keys);

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<TValue> GetManyValues(IEnumerable<TKey> keys);
	}
}