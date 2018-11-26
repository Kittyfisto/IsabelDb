using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
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
