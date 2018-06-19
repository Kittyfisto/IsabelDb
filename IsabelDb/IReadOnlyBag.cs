using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A collection which holds a list of values in their inserted order.
	///     Values can be streamed back or queried in batches.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyBag<T>
		: IReadOnlyCollection<T>
	{
		/// <summary>
		///     Returns the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		T GetValue(RowId key);

		/// <summary>
		///     Returns the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryGetValue(RowId key, out T value);

		/// <summary>
		///     Returns all values who's keys are associated with the given interval.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		IEnumerable<T> GetValues(Interval<RowId> interval);

		/// <summary>
		///     Returns all values associated with the given keys.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<T> GetManyValues(IEnumerable<RowId> keys);
	}
}