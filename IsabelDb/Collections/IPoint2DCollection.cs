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
	}
}
