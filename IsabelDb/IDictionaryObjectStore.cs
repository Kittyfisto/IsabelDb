using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     Represents a persistent dictionary.
	///     All operations are immediately backed by the underlying storage medium.
	/// </summary>
	public interface IDictionaryObjectStore<TKey, TValue>
		: IObjectStore
	{
		/// <summary>
		///     Finds all objects.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> GetAll();

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> Get(IEnumerable<TKey> keys);

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<TKey, TValue>> Get(params TKey[] keys);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		TValue Get(TKey key);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(TKey key, TValue value);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		void Put(IEnumerable<KeyValuePair<TKey, TValue>> values);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		void Remove(TKey key);
	}
}