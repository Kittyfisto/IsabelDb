namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
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
