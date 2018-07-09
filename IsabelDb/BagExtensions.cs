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
		public static IEnumerable<RowId> PutMany<T>(this IBag<T> that, params T[] values)
		{
			return that.PutMany((IEnumerable<T>) values);
		}
	}
}