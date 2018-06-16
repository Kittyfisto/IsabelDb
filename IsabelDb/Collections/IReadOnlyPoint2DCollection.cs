using System.Collections.Generic;

namespace IsabelDb.Collections
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
		IEnumerable<Point2D> GetKeysWithin(Rectangle2D rectangle);

		/// <summary>
		///     Returns all values wit
		/// </summary>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		IEnumerable<TValue> GetValuesWithin(Rectangle2D rectangle);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<Point2D, TValue>> GetWithin(Rectangle2D rectangle);
	}
}