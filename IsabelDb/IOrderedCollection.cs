using System;

namespace IsabelDb
{
	/// <summary>
	///     This collection maps values to specific keys. Contrary to a <see cref="IDictionary{TKey,TValue}" />,
	///     its keys need to implement <see cref="IComparable{T}" /> allowing range queries.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IOrderedCollection<TKey, TValue>
		: IDictionary<TKey, TValue>
		where TKey : IComparable<TKey>
	{
	}
}