using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[SetUpFixture]
	public sealed class AssemblySetup
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string nativePath;
			if (Environment.Is64BitProcess)
			{
				nativePath = Path.Combine(AssemblyDirectory, "x64");
			}
			else
			{
				nativePath = Path.Combine(AssemblyDirectory, "x86");
			}
			
			var path = Environment.GetEnvironmentVariable("PATH");
			path = string.Format("{0};{1}", nativePath, path);
			Environment.SetEnvironmentVariable("PATH", path);
		}

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}
