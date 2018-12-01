using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.HashSet
{
	[TestFixture]
	public sealed class ReadOnlyHashSetTest
		: AbstractTest
	{
		[Test]
		public void TestContainsReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var hashSet = db.GetHashSet<string>("Stuff");
					hashSet.Add("Hello");
					hashSet.Add("Erased");
				}

				using (var db = CreateReadOnlyDatabase(connection))
				{
					var hashSet = db.GetHashSet<string>("Stuff");
					hashSet.Contains("Hello").Should().BeTrue();
					hashSet.Contains("Erased").Should().BeTrue();

					hashSet.Contains("hELLO").Should().BeFalse();
					hashSet.Contains("eRASED").Should().BeFalse();
				}
			}
		}
	}
}
