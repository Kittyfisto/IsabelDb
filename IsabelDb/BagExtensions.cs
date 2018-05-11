using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	public static class BagExtensions
	{
		/// <summary>
		/// </summary>
		/// <param name="that"></param>
		/// <param name="values"></param>
		public static void PutMany<T>(this IBag<T> that, params T[] values)
		{
			that.PutMany((IEnumerable<T>) values);
		}
	}
}