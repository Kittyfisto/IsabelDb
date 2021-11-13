using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A collection which behaves in a first-in, first-out manner.
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface IQueue<T>
		: ICollection<T>
		, IReadOnlyQueue<T>
	{
		/// <summary>
		///     Adds the given <paramref name="value"/> to the end of this queue.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void Enqueue(T value);

		/// <summary>
		///     Adds the given <paramref name="values"/> (in the same order as they are given) to the end of this queue.
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void EnqueueMany(IEnumerable<T> values);

		/// <summary>
		///     Tries to remove the value from the front of this queue.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if the queue wasn't empty and a value was removed, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		bool TryDequeue(out T value);
	}
}
