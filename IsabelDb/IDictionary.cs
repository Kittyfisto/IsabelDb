using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A collection which maps values to keys: There can only ever be one value per key.
	/// </summary>
	/// <remarks>
	///     All data is persisted in the database (which usually means a file on disk).
	/// </remarks>
	/// <remarks>
	///     <see cref="Put" />, <see cref="PutMany" />, <see cref="Remove" /> and <see cref="ICollection.Clear" />
	///     only return once the data has been written to the disk / removed from it.
	/// </remarks>
	/// <remarks>
	///     Every method is atomic.
	/// </remarks>
	/// <remarks>
	///     You can use custom data types as keys, however you should know the following:
	///     - Your implementations of  <see cref="object.GetHashCode" /> / <see cref="object.Equals(object)" /> are irrelevant
	///     to this database
	///     - Two keys are equal if their serialized byte array are equal
	///     - Once you've added a key of a particular type to the database, you may *never* modify the key type (i.e. adding
	///     new fields is a no go)
	/// </remarks>
	public interface IDictionary<TKey, TValue>
		: IReadOnlyDictionary<TKey, TValue>
		, ICollection<TValue>
	{
		/// <summary>
		///     Adds or replaces the value at the given key with the new value.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(TKey key, TValue value);

		/// <summary>
		///     Adds or replaces values with the given keys with these new values.
		/// </summary>
		/// <param name="values"></param>
		void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values);

		/// <summary>
		///     Removes the value with the given key.
		///     Does nothing if the key is not part of this dictionary.
		/// </summary>
		/// <param name="key"></param>
		void Remove(TKey key);
	}
}