using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     This collections maps values to intervals.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the collection.
	///     If you need to modify it, please use <see cref="IIntervalCollection{TKey,TValue}" />.
	/// </remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyIntervalCollection<TKey, TValue>
		: IReadOnlyCollection<TValue>
		where TKey : IComparable<TKey>
	{
		/// <summary>
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<Interval<TKey>> GetManyIntervals(IEnumerable<RowId> keys);

		/// <summary>
		///     Returns the list of values who's intervals intersect with the given key.
		/// </summary>
		/// <remarks>
		///     Never throws, returns an empty enumeration if no interval intersects the given key.
		/// </remarks>
		/// <param name="key"></param>
		/// <returns></returns>
		[Pure]
		IEnumerable<TValue> GetValues(TKey key);

		/// <summary>
		///     Returns the list of values who's interval intersects with the given interval.
		/// </summary>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns></returns>
		[Pure]
		IEnumerable<TValue> GetValues(TKey minimum, TKey maximum);

		/// <summary>
		///     Returns the list of all values including their intervals.
		/// </summary>
		/// <returns></returns>
		[Pure]
		IEnumerable<KeyValuePair<Interval<TKey>, TValue>> GetAll();
	}
}