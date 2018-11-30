using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace IsabelDb
{
	internal static class Constants
	{
		/// <summary>
		///     The version of this assembly.
		/// </summary>
		public static readonly Version AssemblyVersion;

		/// <summary>
		///     The version of the database / schema.
		///     Only databases with the same version are compatible.
		/// </summary>
		public static readonly int DatabaseSchemaVersion;

		static Constants()
		{
			AssemblyVersion = GetAssemblyVersion();

			// INCREMENTING THIS VALUE MEANS DECLARING A BREAKING CHANGE OCCURED.
			// YOU NEVER WANT TO DO THIS EXCEPT BEFORE THE INITIAL RELEASE
			DatabaseSchemaVersion = 3;
		}

		[Pure]
		private static Version GetAssemblyVersion()
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetName().Version;
		}
	}
}