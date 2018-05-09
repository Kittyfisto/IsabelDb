using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     This collection maps values to specific keys. Contrary to a <see cref="IDictionary{TKey,TValue}" />,
	///     its keys need to implement <see cref="IComparable{T}" /> allowing range queries.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IOrderedCollection<TKey, TValue>
		: ICollection<TValue>
		where TKey : IComparable<TKey>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(TKey key, TValue value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pairs"></param>
		void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> pairs);
	}
}