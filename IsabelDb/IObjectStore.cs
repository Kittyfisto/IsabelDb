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
		/// <remarks>
		///     TODO: Do we really need this method? There is an outstanding problem in which
		///           we have an inconsistency between Count() and GetAll().Count() when some
		///           types fail to be resolved.
		/// </remarks>
		int Count();
	}
}