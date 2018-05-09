﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface ICollection<out TValue>
		: ICollection
	{
		/// <summary>
		///     Returns the list of all values.
		/// </summary>
		/// <returns></returns>
		[Pure]
		IEnumerable<TValue> GetAllValues();
	}

	/// <summary>
	/// </summary>
	public interface ICollection
	{
		/// <summary>
		///     Removes all objects from this store.
		/// </summary>
		void Clear();

		/// <summary>
		///     Counts the number of objects in this store.
		/// </summary>
		/// <remarks>
		///     TODO: Do we really need this method? There is an outstanding problem in which
		///     we have an inconsistency between Count() and GetAll().Count() when some
		///     types fail to be resolved.
		/// </remarks>
		long Count();
	}
}