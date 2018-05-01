using System;
using System.Data;

namespace IsabelDb
{
	internal static class SQLiteHelper
	{
		public static string GetAffinity(DbType databaseType)
		{
			switch (databaseType)
			{
				case DbType.String:
					return "STRING";

				case DbType.Byte:
				case DbType.SByte:
				case DbType.UInt16:
				case DbType.Int16:
				case DbType.Int32:
				case DbType.UInt32:
				case DbType.Int64:
					return "INTEGER";

				case DbType.Single:
				case DbType.Double:
					return "REAL";

				case DbType.Binary:
					return "BLOB";

				default:
					throw new NotImplementedException(string.Format("Type '{0}' is not implemented", databaseType));
			}
		}
	}
}