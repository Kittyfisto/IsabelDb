using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     This collections maps values to intervals.
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
	/// <remarks>
	///     You can use custom data types as keys, however you should know the following:
	///     - Your implementations of  <see cref="object.GetHashCode" /> / <see cref="object.Equals(object)" /> are irrelevant
	///     to this database
	///     - Two keys are equal if their serialized byte arrays are equal
	///     - Once you've added a key of a particular type to the database, you may *never* modify the key type (i.e. adding/removing
	///     fields/properties is a no go)
	/// </remarks>
	/// <typeparam name="TKey">The type of the interval (such as int, double, DateTime) to which values are mapped</typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IIntervalCollection<TKey, TValue>
		: IReadOnlyIntervalCollection<TKey, TValue>
		, ICollection<TValue>
		where TKey : IComparable<TKey>
	{
		/// <summary>
		///     Adds a new value with the given interval to this collection.
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void Put(Interval<TKey> interval, TValue value);

		/// <summary>
		///     Adds the given values to this collection.
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void PutMany(IEnumerable<KeyValuePair<Interval<TKey>, TValue>> values);

		/// <summary>
		///     Removes all values who's interval intersects with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void Remove(TKey key);

		/// <summary>
		///     Removes all values who's interval intersects with the given interval.
		/// </summary>
		/// <param name="interval"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void Remove(Interval<TKey> interval);
	}
}