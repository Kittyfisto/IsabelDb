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
		/// <returns></returns>
		[Pure]
		bool Contains(T value);
	}
}
