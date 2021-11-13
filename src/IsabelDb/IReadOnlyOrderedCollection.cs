using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     This collection maps values to specific keys. Contrary to a <see cref="IReadOnlyDictionary{TKey,TValue}" />,
	///     its keys need to implement <see cref="IComparable{T}" /> allowing range queries.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the collection.
	///     If you need to modify it, please use <see cref="IOrderedCollection{TKey,TValue}" />.
	/// </remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyOrderedCollection<TKey, out TValue>
		: IReadOnlyCollection<TValue>
		where TKey : IComparable<TKey>
	{
		/// <summary>
		///     Returns all values in the given range.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		[Pure]
		IEnumerable<TValue> GetValues(Interval<TKey> interval);
	}
}