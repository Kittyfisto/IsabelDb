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

		/// <summary>
		///     When set to true, then the true type should be stored with every value
		///     (which needs to be the case for polymorphic types). If not, then
		///     type information is only stored once for the table.
		/// </summary>
		bool StorePerValueTypeInformation { get; }
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
		/// <remarks>
		///     Only needs to be implemented when <see cref="ISQLiteSerializer.StorePerValueTypeInformation" /> is true.
		/// </remarks>
		/// <param name="value"></param>
		/// <param name="typeId"></param>
		/// <returns></returns>
		object Serialize(T value, out int typeId);

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Only needs to be implemented when <see cref="ISQLiteSerializer.StorePerValueTypeInformation" /> is true.
		/// </remarks>
		/// <param name="reader"></param>
		/// <param name="valueOrdinal">The ordinal of the column where the value is stored</param>
		/// <param name="typeOrdinal">The ordinal of the column where the value's type is stored</param>
		/// <returns></returns>
		T Deserialize(SQLiteDataReader reader, int valueOrdinal, int typeOrdinal);
	}
}