using System.Collections.Generic;

namespace IsabelDb
{
	public interface IObjectStore
	{
		/// <summary>
		///     Finds all objects.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, object>> GetAll();

		/// <summary>
		///     Finds all object of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> GetAll<T>();

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> Get<T>(IEnumerable<string> keys);

		/// <summary>
		///     Finds all objects with the given keys and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, T>> Get<T>(params string[] keys);

		/// <summary>
		///     Finds all objects with the given keys.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, object>> Get(params string[] keys);

		/// <summary>
		///     Finds all objects with the given keys.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, object>> Get(IEnumerable<string> keys);

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		T Get<T>(string key);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object Get(string key);

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void Put(string key, object value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		void Put(IEnumerable<KeyValuePair<string, object>> values);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		void Put<T>(IEnumerable<KeyValuePair<string, T>> values);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		void Remove(string key);

		/// <summary>
		/// 
		/// </summary>
		int Count();
	}
}