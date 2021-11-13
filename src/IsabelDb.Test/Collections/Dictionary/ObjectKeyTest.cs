using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class ObjectKeyTest
		: AbstractDictionaryObjectStoreTest<object>
	{
		protected override IEnumerable<Type> CustomTypes => new []{typeof(Version)};

		protected override object SomeKey => 42;

		protected override object DifferentKey => new Version(4, 3, 2, 1);

		protected override IReadOnlyList<object> ManyKeys => new object[]
		{
			"Just have some goddamn faith, Arthur!", 42, 6.67408e-11, new Version(1, 0, 0, 0)
		};

		[Test]
		public void TestRemove()
		{
			using (var db = Database.CreateInMemory(new []{typeof(Version)}))
			{
				var dictionary = db.GetOrCreateDictionary<object, string>("Description");
				dictionary.Put(new Version(1, 0, 0, 0), "Fresh off the shelf");
				dictionary.Put(new Version(1, 0, 0, 1), "Some bugs were to be expected");
				dictionary.Put(new Version(1, 0, 0, 20), "Next time we'll write tests first");

				dictionary.Remove(new Version(1, 0, 0, 0));
				dictionary.Count().Should().Be(2);
				dictionary.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					"Some bugs were to be expected",
					"Next time we'll write tests first"
				});

				dictionary.RemoveMany(new []{new Version(1, 0, 0, 20) });
				dictionary.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					"Some bugs were to be expected"
				});
			}
		}
	}
}
