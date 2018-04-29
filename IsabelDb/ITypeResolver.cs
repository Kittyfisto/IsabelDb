using System;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for resolving types from their names and vice verca.
	/// </summary>
	public interface ITypeResolver
	{
		/// <summary>
		///     Tries to resolve the type which is identified by the typename.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		Type Resolve(string typeName);

		/// <summary>
		///     Returns the typename for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		string GetName(Type type);
	}
}