namespace IsabelDb
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IHashSet<T>
		: ICollection<T>
			, IReadOnlyHashSet<T>
	{
		/// <summary>
		///     Adds the given value.
		///     If the value is already part of this collection, then nothing is done.
		/// </summary>
		/// <param name="value">The value to add to this collection</param>
		/// <returns>True when the value wasn't previously part of this collection, false otherwise</returns>
		bool Add(T value);

		/// <summary>
		///     Removes the given value from this collection.
		///     Does nothing if the value isn't part of this collection.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool Remove(T value);
	}
}