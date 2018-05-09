using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A dictionary where multiple values are associated with the same key.
	/// </summary>
	public interface IMultiValueDictionary<TKey, TValue>
		: ICollection<TValue>
	{
		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void Put(TKey key, TValue value);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		void PutMany(TKey key, IEnumerable<TValue> values);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		void PutMany(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> values);

		/// <summary>
		///     Returns all values associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEnumerable<TValue> Get(TKey key);

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
		IEnumerable<IEnumerable<TValue>> GetMany(IEnumerable<TKey> keys);

		/// <summary>
		///     Returns all values from this collection.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> GetAll();

		/// <summary>
		///     Removes the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		void RemoveAll(TKey key);
	}
}