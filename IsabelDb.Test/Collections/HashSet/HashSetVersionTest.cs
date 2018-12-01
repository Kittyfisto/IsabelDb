using System;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.HashSet
{
	[TestFixture]
	public sealed class HashSetVersionTest
	{
		[Test]
		public void TestAddSameVersion()
		{
			using (var db = Database.CreateInMemory(new[] {typeof(Version)}))
			{
				var hashSet = db.GetHashSet<Version>("dwadw");
				hashSet.Add(new Version(1, 2, 3, 4)).Should().BeTrue();
				hashSet.Add(new Version(1, 2, 3, 4)).Should().BeFalse();

				hashSet.GetAllValues().Should().Equal(new object[] {new Version(1, 2, 3, 4)});
			}
		}

		[Test]
		public void TestAddDifferentVersions()
		{
			using (var db = Database.CreateInMemory(new[] {typeof(Version)}))
			{
				var hashSet = db.GetHashSet<Version>("dwadw");
				hashSet.Add(new Version(45, 0)).Should().BeTrue();
				hashSet.Add(new Version(45, 33)).Should().BeTrue();

				hashSet.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					new Version(45, 0),
					new Version(45, 33)
				});
			}
		}
	}
}
