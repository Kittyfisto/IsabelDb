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
		[Pure]
		new IEnumerable<TValue> GetAllValues();
	}

	/// <summary>
	///     The interface for a read-only collection of an IsabelDb database.
	/// </summary>
	public interface IReadOnlyCollection
	{
		/// <summary>
		///     The name of this collection.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The of this collection.
		/// </summary>
		CollectionType Type { get; }

		/// <summary>
		///     Returns the list of all values.
		/// </summary>
		/// <remarks>
		///     Values are streamed from the database on-demand as the iterator is moved.
		/// </remarks>
		/// <returns></returns>
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
		long Count();
	}
}