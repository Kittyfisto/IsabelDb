using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///    A collection where a value can only appear once.
	///    Two values are identical if their serialized byte arrays are identical.
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
		/// <exception cref="ArgumentNullException">In case <paramref name="value" /> is null</exception>
		bool Add(T value);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="ArgumentNullException">In case <paramref name="values" /> is null</exception>
		/// <exception cref="ArgumentException">In case any value in <paramref name="values" /> is null</exception>
		void AddMany(IEnumerable<T> values);

		/// <summary>
		///     Removes the given value from this collection.
		///     Does nothing if the value isn't part of this collection.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if the value was part of this collection, false otherwise</returns>
		/// <exception cref="ArgumentNullException">In case <paramref name="value" /> is null</exception>
		bool Remove(T value);
	}
}