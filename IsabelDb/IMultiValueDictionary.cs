using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A dictionary where multiple values are associated with the same key.
	/// </summary>
	public interface IMultiValueDictionary<TKey, TValue>
		: IReadOnlyMultiValueDictionary<TKey, TValue>
		, ICollection<TValue>
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
		///     Removes the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		void RemoveAll(TKey key);
	}
}