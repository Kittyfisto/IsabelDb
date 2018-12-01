using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyPoint2DCollection<TValue>
		: IReadOnlyMultiValueDictionary<Point2D, TValue>
	{
		/// <summary>
		///     Returns all keys within the given rectangle.
		/// </summary>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		[Pure]
		IEnumerable<Point2D> GetKeysWithin(Rectangle2D rectangle);

		/// <summary>
		///     Returns all values wit
		/// </summary>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		[Pure]
		IEnumerable<TValue> GetValuesWithin(Rectangle2D rectangle);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		[Pure]
		IEnumerable<KeyValuePair<Point2D, TValue>> GetWithin(Rectangle2D rectangle);
	}
}