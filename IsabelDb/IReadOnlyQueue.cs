namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyQueue<T>
		: IReadOnlyCollection<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool TryPeek(out T value);
	}
}