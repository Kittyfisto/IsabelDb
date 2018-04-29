using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     Represents a persistent dictionary.
	///     All operations are immediately backed by the underlying storage medium.
	/// </summary>
	public interface IDictionaryObjectStore<T>
		: IObjectStore
	{
		/// <summary>
		///     Finds all objects.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> GetAll();

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> Get(IEnumerable<string> keys);

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> Get(params string[] keys);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		T Get(string key);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(string key, T value);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		void Put(IEnumerable<KeyValuePair<string, T>> values);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		void Remove(string key);
	}
}