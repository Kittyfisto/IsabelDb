namespace IsabelDb
{
	/// <summary>
	/// </summary>
	public interface IObjectStore
	{
		/// <summary>
		///     Removes all objects from this store.
		/// </summary>
		void Clear();

		/// <summary>
		///     Counts the number of objects in this store.
		/// </summary>
		int Count();
	}
}