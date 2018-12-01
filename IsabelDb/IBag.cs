using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A collection which holds a list of values in their inserted order.
	///     Values can be streamed back or queried in batches.
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
	/// <remarks>
	///     <see cref="Put" />, <see cref="PutMany" /> and <see cref="ICollection.Clear" />
	///     only return once the data has been written to the disk / removed from it.
	/// </remarks>
	/// <remarks>
	///     Every method is atomic.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface IBag<T>
		: IReadOnlyBag<T>
		, ICollection<T>
	{
		/// <summary>
		///     Adds a new value to this bag.
		/// </summary>
		/// <param name="value"></param>
		RowId Put(T value);

		/// <summary>
		///     Adds the given values to this bag.
		/// </summary>
		/// <param name="values"></param>
		IEnumerable<RowId> PutMany(IEnumerable<T> values);

		/// <summary>
		///     Removes the value associated with the given key.
		/// </summary>
		/// <param name="key"></param>
		void Remove(RowId key);
	}
}