using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public abstract class AbstractIsabelDbKeyTest<TKey>
	{
		protected abstract TKey SomeKey { get; }
		protected abstract TKey DifferentKey { get; }

		[Test]
		public void TestGetNonExistantKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Get(SomeKey).Should().BeNull();
			}
		}

		[Test]
		public void TestOverwriteValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, 42);
				values.Get(SomeKey).Should().Be(42);

				values.Put(SomeKey, 9001);
				values.Get(SomeKey).Should().Be(9001);
			}
		}

		[Test]
		public void TestStoreTwoValues()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "Hello");
				values.Put(DifferentKey, "World");

				values.Get(SomeKey).Should().Be("Hello");
				values.Get(DifferentKey).Should().Be("World");
			}
		}

		[Test]
		public void TestGetKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "Hello");
				values.Put(DifferentKey, "World");

				var allValues = values.GetAll();
				allValues.Should().HaveCount(2);
				allValues.ElementAt(0).Key.Should().Be(SomeKey);
				allValues.ElementAt(0).Value.Should().Be("Hello");
				allValues.ElementAt(1).Key.Should().Be(DifferentKey);
				allValues.ElementAt(1).Value.Should().Be("World");
			}
		}
	}
}
