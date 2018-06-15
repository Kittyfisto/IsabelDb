using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     A dictionary where multiple values are associated with the same key.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the collection.
	///     If you need to modify it, please use <see cref="IMultiValueDictionary{TKey,TValue}" />.
	/// </remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyMultiValueDictionary<TKey, TValue>
	{
		/// <summary>
		///     Returns all keys in this collection.
		/// </summary>
		/// <returns></returns>
		IEnumerable<TKey> GetAllKeys();

		/// <summary>
		///    Tests if there is a value with the given key in this collection.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Pure]
		bool ContainsKey(TKey key);

		/// <summary>
		///     Returns all values associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEnumerable<TValue> GetValues(TKey key);

		/// <summary>
		///     Returns all values with the given keys.
		///     The returned list contains one collection *per* key.
		///     If there's no value for a given key, then an empty collection is returned for that key.
		/// </summary>
		/// <example>
		///     If you query for two keys, then the returned list will contain 2 values.
		///     The first value will be the list of values for the first key, and so forth...
		/// </example>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<TValue> GetValues(IEnumerable<TKey> keys);

		/// <summary>
		///     Returns all values from this collection.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> GetAll();
	}
}