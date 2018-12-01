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
		/// 
		/// </summary>
		/// <param name="value"></param>
		void Enqueue(T value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		void EnqueueMany(IEnumerable<T> values);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryDequeue(out T value);
	}
}
