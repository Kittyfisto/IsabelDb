using System;
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
	///     - Once you've added a key of a particular type to the database, you may *never* modify the key type (i.e. adding/removing
	///     fields/properties is a no go)
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
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="key"/> is null.</exception>
		void Put(TKey key, TValue value);

		/// <summary>
		///     Adds or replaces values with the given keys with these new values.
		/// </summary>
		/// <param name="values"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="values"/> is null.</exception>
		/// <exception cref="ArgumentException">In case any key in <paramref name="values" /> is null</exception>
		void PutMany(IEnumerable<KeyValuePair<TKey, TValue>> values);

		/// <summary>
		///    Adds the given value with the given key.
		///    Does nothing if there already exists a value with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		bool PutIfNotExists(TKey key, TValue value);

		/// <summary>
		///     Moves the value from <paramref name="oldKey"/> to <paramref name="newKey"/>.
		///     If there already exists a value for <paramref name="newKey"/>, then it is overwritten.
		///     If <paramref name="oldKey"/> does not exist, then nothing is changed.
		/// </summary>
		/// <remarks>
		///     This method behaves almost identical to the following methods called in that order:
		///     - <see cref="IReadOnlyDictionary{TKey,TValue}.TryGet"/>
		///     - <see cref="Remove"/>
		///     - <see cref="Put"/>
		///     With the exception that TryGet will return false the value cannot be deserialized whereas
		///     this method will simply move data without attempting a deserialization / serialization roundtrip.
		/// </remarks>
		/// <param name="oldKey"></param>
		/// <param name="newKey"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="oldKey"/> is null.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="newKey"/> is null.</exception>
		void Move(TKey oldKey, TKey newKey);

		/// <summary>
		///     Removes the value with the given key.
		///     Does nothing if the key is not part of this dictionary.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>True when there was a value with the given <paramref name="key"/> in this dictionary, false otherwise</returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="key"/> is null.</exception>
		bool Remove(TKey key);

		/// <summary>
		///     Removes the values with the given keys.
		/// </summary>
		/// <param name="keys"></param>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="keys"/> is null.</exception>
		/// <exception cref="ArgumentException">In case any value of <paramref name="keys"/> is null.</exception>
		void RemoveMany(IEnumerable<TKey> keys);
	}
}