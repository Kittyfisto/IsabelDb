using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     This collections maps values to intervals.
	/// </summary>
	/// <typeparam name="T">The type of the interval (such as int, double, DateTime) to which values are mapped</typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IIntervalCollection<T, TValue>
		: IReadOnlyIntervalCollection<T, TValue>
		, ICollection<TValue>
		where T : IComparable<T>
	{
		/// <summary>
		///     Adds a new value with the given interval to this collection.
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void Put(Interval<T> interval, TValue value);

		/// <summary>
		///     Adds the given values to this collection.
		/// </summary>
		/// <param name="values"></param>
		void PutMany(IEnumerable<KeyValuePair<Interval<T>, TValue>> values);

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
	}
}