using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     This collections maps values to intervals.
	/// </summary>
	/// <typeparam name="T">The type of the interval (such as int, double, DateTime) to which values are mapped</typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IIntervalCollection<T, TValue>
		: ICollection<TValue>
		where T : IComparable<T>
	{
		/// <summary>
		///     Adds a new value with the given interval to this collection.
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		ValueKey Put(Interval<T> interval, TValue value);

		/// <summary>
		///     Adds the given values to this collection.
		/// </summary>
		/// <param name="values"></param>
		IEnumerable<ValueKey> PutMany(IEnumerable<KeyValuePair<Interval<T>, TValue>> values);

		/// <summary>
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<Interval<T>> GetManyIntervals(IEnumerable<ValueKey> keys);

		/// <summary>
		///     Returns the list of values who's intervals intersect with the given key.
		/// </summary>
		/// <remarks>
		///     Never throws, returns an empty enumeration if no interval intersects the given key.
		/// </remarks>
		/// <param name="key"></param>
		/// <returns></returns>
		[Pure]
		IEnumerable<TValue> GetValues(T key);

		/// <summary>
		///     Returns the list of values who's interval intersects with the given interval.
		/// </summary>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns></returns>
		[Pure]
		IEnumerable<TValue> GetValues(T minimum, T maximum);

		/// <summary>
		///     Returns the list of all values including their intervals.
		/// </summary>
		/// <returns></returns>
		[Pure]
		IEnumerable<KeyValuePair<Interval<T>, TValue>> GetAll();

		/// <summary>
		///     Changes the interval of the value referenced by <paramref name="key" /> to the given interval.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="interval"></param>
		void Move(ValueKey key, Interval<T> interval);

		/// <summary>
		///     Removes all values who's interval intersects with the given key.
		/// </summary>
		/// <param name="key"></param>
		void Remove(T key);

		/// <summary>
		///     Removes all values who's interval intersects with the given interval.
		/// </summary>
		/// <param name="interval"></param>
		void Remove(Interval<T> interval);

		/// <summary>
		///     Removes a value which was previously inserted via <see cref="Put" />.
		/// </summary>
		/// <param name="valueKey"></param>
		void Remove(ValueKey valueKey);
	}
}