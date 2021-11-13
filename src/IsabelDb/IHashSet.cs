using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///    A collection where a value can only appear once.
	///    Two values are identical if their serialized byte arrays are identical.
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
	/// <remarks>
	///     You can use custom data types as values, however you should know the following:
	///     - Your implementations of  <see cref="object.GetHashCode" /> / <see cref="object.Equals(object)" /> are irrelevant
	///     to this database
	///     - Two keys are equal if their serialized byte arrays are equal
	///     - Once you've added a key of a particular type to the database, you may *never* modify the key type (i.e. adding/removing
	///     fields/properties is a no go)
	/// </remarks>
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
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="value" /> is null</exception>
		bool Add(T value);

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="values" /> is null</exception>
		/// <exception cref="ArgumentException">In case any value in <paramref name="values" /> is null</exception>
		void AddMany(IEnumerable<T> values);

		/// <summary>
		///     Removes the given value from this collection.
		///     Does nothing if the value isn't part of this collection.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if the value was part of this collection, false otherwise</returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="value" /> is null</exception>
		bool Remove(T value);
	}
}