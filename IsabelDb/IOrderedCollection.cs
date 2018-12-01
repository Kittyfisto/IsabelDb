using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     This collection maps values to specific keys. Contrary to a <see cref="IDictionary{TKey,TValue}" />,
	///     its keys need to implement <see cref="IComparable{T}" /> allowing range queries.
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
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IOrderedCollection<TKey, TValue>
		: IReadOnlyOrderedCollection<TKey, TValue>
		, ICollection<TValue>
		where TKey : IComparable<TKey>
	{
		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(TKey key, TValue value);

		/// <summary>
		/// </summary>
		/// <param name="pairs"></param>
		void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> pairs);

		/// <summary>
		///     Removes all values in the given range.
		/// </summary>
		/// <param name="interval"></param>
		void RemoveRange(Interval<TKey> interval);
	}
}