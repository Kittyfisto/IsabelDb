using System;
using System.Collections.Generic;
using FluentAssertions;
using IsabelDb.TypeModels.Surrogates;
using NUnit.Framework;

namespace IsabelDb.Test.TypeModels.Surrogates
{
	[TestFixture]
	public sealed class VersionSurrogateTest
	{
		public static IEnumerable<int> Integers => new[]
		{
			0,
			1,
			int.MaxValue
		};

		[Test]
		public void TestRoundtrip([ValueSource(nameof(Integers))] int major,
		                          [ValueSource(nameof(Integers))] int minor)
		{
			var version = new Version(major, minor);
			var actualVersion = (Version) (VersionSurrogate) version;
			actualVersion.Should().NotBeNull();
			actualVersion.Major.Should().Be(major);
			actualVersion.Minor.Should().Be(minor);
			actualVersion.Build.Should().Be(version.Build);
			actualVersion.Revision.Should().Be(version.Revision);
		}

		[Test]
		public void TestRoundtrip([ValueSource(nameof(Integers))] int major,
		                          [ValueSource(nameof(Integers))] int minor,
		                          [ValueSource(nameof(Integers))] int build)
		{
			var version = new Version(major, minor, build);
			var actualVersion = (Version) (VersionSurrogate) version;
			actualVersion.Should().NotBeNull();
			actualVersion.Major.Should().Be(major);
			actualVersion.Minor.Should().Be(minor);
			actualVersion.Build.Should().Be(build);
			actualVersion.Revision.Should().Be(version.Revision);
		}

		[Test]
		public void TestRoundtrip([ValueSource(nameof(Integers))] int major,
		                          [ValueSource(nameof(Integers))] int minor,
		                          [ValueSource(nameof(Integers))] int build,
		                          [ValueSource(nameof(Integers))] int revision)
		{
			var version = new Version(major, minor, build, revision);
			var actualVersion = (Version) (VersionSurrogate) version;
			actualVersion.Should().NotBeNull();
			actualVersion.Major.Should().Be(major);
			actualVersion.Minor.Should().Be(minor);
			actualVersion.Build.Should().Be(build);
			actualVersion.Revision.Should().Be(revision);
		}
	}
}
