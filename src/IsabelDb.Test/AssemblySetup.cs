using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[SetUpFixture]
	[Parallelizable(ParallelScope.Fixtures)]
	public sealed class AssemblySetup
	{
		public static string AssemblyDirectory
		{
			get
			{
				var location = Assembly.GetExecutingAssembly().Location;
				var folder = Path.GetDirectoryName(location);
                return folder;
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