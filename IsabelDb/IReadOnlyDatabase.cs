using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     A key-value store for objects.
	/// </summary>
	/// <remarks>
	///     This interface only allows values to be retrieved from the database, but nothing can be modified.
	///     Use <see cref="IDatabase" /> if you need to add/remove/modify collections, values, etc...
	/// </remarks>
	public interface IReadOnlyDatabase
		: IDisposable
	{
		/// <summary>
		///     Provides access to the list of collections in this database.
		/// </summary>
		IEnumerable<IReadOnlyCollection> Collections { get; }

		/// <summary>
		///      Returns a readonly bag which has previously been created with <see cref="IDatabase.GetOrCreateBag{T}"/> or <see cref="IDatabase.CreateBag{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a bag but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetBag&lt;string&gt;() and is now retrieved using GetBag&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyBag<T> GetBag<T>(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		IReadOnlyHashSet<T> GetHashSet<T>(string name);

		/// <summary>
		///     Returns a readonly queue which has previously been created with <see cref="IDatabase.GetOrCreateQueue{T}"/> or <see cref="IDatabase.CreateQueue{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a queue but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetQueue&lt;string&gt;() and is now retrieved using GetQueue&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyQueue<T> GetQueue<T>(string name);

		/// <summary>
		///     Returns a readonly dictionary which has previously been created with <see cref="IDatabase.GetOrCreateDictionary{TKey,TValue}"/> or <see cref="IDatabase.CreateDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a dictionary but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetDictionary&lt;int, string&gt;() and is now retrieved using GetDictionary&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Returns a readonly multi value dictionary which has previously been created with <see cref="IDatabase.GetOrCreateMultiValueDictionary{TKey,TValue}"/> or <see cref="IDatabase.CreateMultiValueDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a multi value dictionary but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetDictionary&lt;int, string&gt;() and is now retrieved using GetDictionary&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name);

		/// <summary>
		///     Returns a readonly interval collection which has previously been created with <see cref="IDatabase.GetOrCreateIntervalCollection{TKey,TValue}"/> or <see cref="IDatabase.CreateIntervalCollection{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not an interval collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetIntervalCollection&lt;int, string&gt;() and is now retrieved using GetIntervalCollection&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;

		/// <summary>
		///     Returns a readonly ordered collection which has previously been created with <see cref="IDatabase.GetOrCreateOrderedCollection{TKey,TValue}"/> or <see cref="IDatabase.CreateOrderedCollection{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="TKey"/> or <typeparamref name="TValue"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not an ordered collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.KeyType"/> or <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="TKey"/> or <typeparamref name="TValue"/>. For example the collection may have been created using GetOrderedCollection&lt;int, string&gt;() and is now retrieved using GetOrderedCollection&lt;int, object&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name)
			where TKey : IComparable<TKey>;

		/// <summary>
		///     Returns a readonly 2d point collection which has previously been created with <see cref="IDatabase.GetOrCreatePoint2DCollection{T}"/> or <see cref="IDatabase.CreatePoint2DCollection{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="NoSuchCollectionException">When no collection named <paramref name="name"/> exists in this database.</exception>
		/// <exception cref="TypeNotRegisteredException">When the type <typeparamref name="T"/> has not been registered with this database upon calling <see cref="Database.OpenOrCreate"/> (or similar methods).</exception>
		/// <exception cref="WrongCollectionTypeException">A collection with the name <paramref name="name"/> exists, but it's not a point2d collection but a different <see cref="CollectionType"/>.</exception>
		/// <exception cref="TypeMismatchException">A collection with the name <paramref name="name"/> exists in the database, but it is of a different <see cref="IReadOnlyCollection.ValueType"/> than the given <typeparamref name="T"/>. For example the collection may have been created using GetBag&lt;string&gt;() and is now retrieved using GetBag&lt;int&gt;()</exception>
		/// <exception cref="TypeCouldNotBeResolvedException">A collection with the name <paramref name="name"/> exists in the database, but it's <see cref="IReadOnlyCollection.ValueType"/> could not be resolved. This most often happens when a type is renamed or has since been removed.</exception>
		IReadOnlyPoint2DCollection<T> GetPoint2DCollection<T>(string name);
	}
}