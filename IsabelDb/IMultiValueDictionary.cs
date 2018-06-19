﻿using System.Collections.Generic;

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
		RowId Put(TKey key, TValue value);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		IReadOnlyList<RowId> PutMany(TKey key, IEnumerable<TValue> values);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> values);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values);

		/// <summary>
		///     Removes the row associated with the given id.
		/// </summary>
		/// <param name="row"></param>
		void Remove(RowId row);

		/// <summary>
		///     Removes the row associated with the given id.
		/// </summary>
		/// <param name="rows"></param>
		void RemoveMany(IEnumerable<RowId> rows);

		/// <summary>
		///     Removes the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		void RemoveAll(TKey key);

		/// <summary>
		///     Removes all values associated with the given keys.
		/// </summary>
		/// <param name="keys"></param>
		void RemoveMany(IEnumerable<TKey> keys);
	}
}