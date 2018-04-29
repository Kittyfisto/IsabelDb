using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBagObjectStore<T>
		: IObjectStore
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerable<T> GetAll();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		void Put(T value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		void PutMany(IEnumerable<T> values);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		void PutMany(params T[] values);
	}
}