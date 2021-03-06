﻿using System;

namespace IsabelDb
{
	/// <summary>
	///     The interface for a collection of an IsabelDb database.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface ICollection<out TValue>
		: IReadOnlyCollection<TValue>
		, ICollection
	{
	}

	/// <summary>
	///     The interface for a collection of an IsabelDb database.
	/// </summary>
	public interface ICollection
		: IReadOnlyCollection
	{
		/// <summary>
		///     Removes all objects from this store.
		/// </summary>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		void Clear();
	}
}