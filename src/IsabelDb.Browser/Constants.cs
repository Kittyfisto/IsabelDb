using System;
using System.IO;

namespace IsabelDb.Browser
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;
		public static readonly string LocalAppDataFolder;

		static Constants()
		{
			ApplicationTitle = "IsabelDb Browser";
			LocalAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationTitle);
		}
	}
}
