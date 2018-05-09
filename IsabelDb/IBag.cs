using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A collection which holds a list of values: Values can be added or the entire collection
	///     can be cleared and the same value can be added many times.
	/// </summary>
	/// <remarks>
	///     <see cref="Put" />, <see cref="PutMany(T[])" /> and <see cref="ICollection.Clear" />
	///     only return once the data has been written to the disk / removed from it.
	/// </remarks>
	/// <remarks>
	///     Every method is atomic.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface IBag<T>
		: ICollection<T>
	{
		/// <summary>
		///     Adds a new value to this bag.
		/// </summary>
		/// <param name="value"></param>
		void Put(T value);

		/// <summary>
		///     Adds the given values to this bag.
		/// </summary>
		/// <param name="values"></param>
		void PutMany(IEnumerable<T> values);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		void PutMany(params T[] values);
	}
}