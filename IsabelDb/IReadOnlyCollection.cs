﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb
{
	/// <summary>
	///     The interface for a read-only collection of an IsabelDb database.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyCollection<out TValue>
		: IReadOnlyCollection
	{
		/// <summary>
		///     Returns the list of all values.
		/// </summary>
		/// <remarks>
		///     Values are streamed from the database on-demand as the iterator is moved.
		/// </remarks>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">When <see cref="IReadOnlyCollection.CanBeAccessed"/> is false.</exception>
		[Pure]
		new IEnumerable<TValue> GetAllValues();
	}

	/// <summary>
	///     The interface for a read-only collection of an IsabelDb database.
	/// </summary>
	public interface IReadOnlyCollection
	{
		/// <summary>
		///     Whether or not this application can access the contents of this collection.
		/// </summary>
		/// <remarks>
		///     An application is unable to access the contents if any of the following is true:
		///     - The collection uses a key and/or value type which has not been registered upon opening the database
		///     - The collection is from a future IsabelDb version and it's collection type isn't known yet
		/// </remarks>
		/// <remarks>
		///     When this property is set to false, then the following (logical) operations will still work:
		///     - Clear
		///     - Count
		///     - Remove
		/// </remarks>
		/// <remarks>
		///     When this property is set to false, then the following (logical) operations will throw:
		///     - Get
		///     - GetMany
		/// </remarks>
		bool CanBeAccessed { get; }

		/// <summary>
		///     The name of this collection.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The of this collection.
		/// </summary>
		CollectionType Type { get; }

		/// <summary>
		///     The type of key used in this collection.
		/// </summary>
		/// <remarks>
		///     Null when this collection type doesn't have keys or when the key type could not be resolved (because it wasn't provided to <see cref="Database.OpenOrCreate"/>).
		/// </remarks>
		Type KeyType { get; }

		/// <summary>
		///     The full name of the key type.
		/// </summary>
		/// <remarks>
		///     Null when this collection type doesn't have keys.
		/// </remarks>
		/// <remarks>
		///     Will be set to the type name of the key type being used, even when the type itself could not be resolved.
		/// </remarks>
		string KeyTypeName { get; }

		/// <summary>
		///     The type of value used in this collection.
		/// </summary>
		/// <remarks>
		///     Null when the value type could not be resolved (because it wasn't provided to <see cref="Database.OpenOrCreate"/>).
		/// </remarks>
		Type ValueType { get; }

		/// <summary>
		///     The full name of the value type.
		/// </summary>
		/// <remarks>
		///     Will be set to the type name of the value type being used, even when the type itself could not be resolved.
		/// </remarks>
		string ValueTypeName { get; }

		/// <summary>
		///     Returns the list of all values.
		/// </summary>
		/// <remarks>
		///     Values are streamed from the database on-demand as the iterator is moved.
		/// </remarks>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">When <see cref="CanBeAccessed"/> is false.</exception>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		[Pure]
		IEnumerable GetAllValues();

		/// <summary>
		///     Counts the number of objects in this store.
		/// </summary>
		/// <remarks>
		///     TODO: Do we really need this method? There is an outstanding problem in which
		///     we have an inconsistency between Count() and GetAll().Count() when some
		///     types fail to be resolved.
		/// </remarks>
		/// <remarks>
		///     TODO: There's a second argument as to why this method should be removed.
		///     In its current incarnation it is incredibly slow. I thought select count()
		///     was fast, but alas, it requires a full table scan (great).
		/// </remarks>
		/// <exception cref="InvalidOperationException">In case this collection has been removed from its <see cref="IDatabase"/>.</exception>
		long Count();
	}
}