namespace IsabelDb.Collections
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface IPoint2DCollection<TValue>
		: IReadOnlyPoint2DCollection<TValue>
		, IMultiValueDictionary<Point2D, TValue>
	{
		/// <summary>
		///     Removes all values who's keys are within the given rectangle.
		/// </summary>
		/// <param name="rectangle"></param>
		void RemoveMany(Rectangle2D rectangle);
	}
}
