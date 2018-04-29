using System.Data;
using System.Data.SQLite;

namespace IsabelDb.Serializers
{
	/// <summary>
	///     Responsible for mapping a .NET type to a type supported by SQLite
	///     and vice versa.
	/// </summary>
	internal interface ISQLiteSerializer
	{
		DbType DatabaseType { get; }
	}

	/// <summary>
	///     Responsible for mapping a .NET type to a type supported by SQLite
	///     and vice versa.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal interface ISQLiteSerializer<T>
		: ISQLiteSerializer
	{
		/// <summary>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		object Serialize(T value);

		/// <summary>
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="valueOrdinal">The ordinal of the column where the value is stored</param>
		/// <returns></returns>
		T Deserialize(SQLiteDataReader reader, int valueOrdinal);
	}
}