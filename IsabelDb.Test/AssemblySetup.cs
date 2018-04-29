using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[SetUpFixture]
	public sealed class AssemblySetup
	{
		public static string AssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;
				var uri = new UriBuilder(codeBase);
				var path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string nativePath;
			if (Environment.Is64BitProcess)
				nativePath = Path.Combine(AssemblyDirectory, "x64");
			else
				nativePath = Path.Combine(AssemblyDirectory, "x86");

			var path = Environment.GetEnvironmentVariable("PATH");
			path = string.Format("{0};{1}", nativePath, path);
			Environment.SetEnvironmentVariable("PATH", path);
		}
	}
}