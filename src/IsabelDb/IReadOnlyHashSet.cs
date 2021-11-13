using System;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///    A collection where a value can only appear once.
	///    Two values are identical if their serialized byte arrays are identical.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyHashSet<T>
		: IReadOnlyCollection<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if this collection contains the given value, false otherwise</returns>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		/// <exception cref="ArgumentNullException">In case <paramref name="value"/> is null</exception>
		[Pure]
		bool Contains(T value);
	}
}
