using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	public interface IDatabase
		: IReadOnlyDatabase
	{
		/// <summary>
		///    Begins a new transaction.
		///    Changes made via mutating methods such as <see cref="IBag{T}.Put"/>,
		///    <see cref="IQueue{T}.Enqueue"/> or <see cref="IDictionary{TKey,TValue}.Remove"/>
		///    are not externally visible until <see cref="ITransaction.Commit"/> is called.
		///    If the transaction is disposed of before <see cref="ITransaction.Commit"/> is called,
		///    then ALL changes made while the transaction was active are rolled back.
		/// </summary>
		/// <remarks>
		///    Transactions may only be used when A SINGLE thread writes to the database.
		///    As soon as there are multiple writers, using transactions is strongly discouraged and
		///    will lead to mutating methods throwing <see cref="SQLiteException"/>s and NOT
		///    storing their data.
		/// </remarks>
		/// <returns></returns>
		ITransaction BeginTransaction();

		/// <summary>
		///     Provides access to the list of collections in this database.
		/// </summary>
		new IEnumerable<ICollection> Collections { get; }

		/// <summary>
		///      Gets or creates a bag with the given name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a bag but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetBag&lt;string&gt;() and is now retrieved using GetBag&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IBag<T> GetBag<T>(string name);

		/// <summary>
		///     Gets or creates a queue with the given name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a queue but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetQueue&lt;string&gt;() and is now retrieved using GetQueue&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IQueue<T> GetQueue<T>(string name);

		/// <summary>
		///     Gets or creates a dictionary with the given name.
		///     There can only be one value per key.
		///     Two keys are identical if their serialized byte array is identical.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a dictionary but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetDictionary&lt;int, string&gt;() and is now retrieved using GetDictionary&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Gets or creates a multi value dictionary with the given name.
		///     There can be multiple values per key.
		///     Two keys are identical if their serialized byte array is identical.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a multi value dictionary but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetDictionary&lt;int, string&gt;() and is now retrieved using GetDictionary&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Gets or creates an interval collection with the given name.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not an interval collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetIntervalCollection&lt;int, string&gt;() and is now retrieved using GetIntervalCollection&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;

		/// <summary>
		///     Gets or creates an ordered collection with the given name.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not an ordered collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetOrderedCollection&lt;int, string&gt;() and is now retrieved using GetOrderedCollection&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name) where TKey : IComparable<TKey>;

		/// <summary>
		///     Gets or creates a 2d point collection with the given name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a point2d collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetBag&lt;string&gt;() and is now retrieved using GetBag&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		new IPoint2DCollection<T> GetPoint2DCollection<T>(string name);

		/// <summary>
		///     Drops the given collection from this database.
		///     Doesn't do anything when this collection isn't part of this database.
		/// </summary>
		/// <remarks>
		///     THIS OPERATION IMMEDIATELY REMOVES ALL OBJECTS FROM THE GIVEN COLLECTION FROM STORAGE.
		///     THIS OPERATION IS NOT REVERSIBLE.
		///     USE WITH CARE.
		/// </remarks>
		/// <param name="collection"></param>
		void Remove(ICollection collection);

		/// <summary>
		///     Drops the collection with the given name from this database.
		///     Doesn't do anything when this collection isn't part of this database.
		/// </summary>
		/// <remarks>
		///     THIS OPERATION IMMEDIATELY REMOVES ALL OBJECTS FROM THE GIVEN COLLECTION FROM STORAGE.
		///     THIS OPERATION IS NOT REVERSIBLE.
		///     USE WITH CARE.
		/// </remarks>
		/// <param name="collectionName"></param>
		void Remove(string collectionName);

		/// <summary>
		///    Rebuilds the database file, repacking it into a minimal amount of disk space.
		/// </summary>
		void Compact();
	}
}