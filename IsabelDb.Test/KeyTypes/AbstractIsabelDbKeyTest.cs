using System.Collections.Generic;
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
		protected abstract IReadOnlyList<TKey> ManyKeys { get; }

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var keys = new HashSet<TKey>();
			foreach (var key in ManyKeys)
			{
				if (!keys.Add(key))
					Assert.Inconclusive("Don't provide duplicate keys!");
			}
		}

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
		public void TestPutManyKeys()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				int i = 0;
				foreach (var key in ManyKeys)
				{
					values.Put(key, i);
					++i;
				}

				i = 0;
				foreach (var key in ManyKeys)
				{
					values.Get(key).Should().Be(i);
					++i;
				}
			}
		}

		[Test]
		public void TestOverwriteValueWithNull()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "A");
				values.Get(SomeKey).Should().Be("A");

				values.Put(SomeKey, null);
				values.Get(SomeKey).Should().BeNull();
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "A");
				values.Get(SomeKey).Should().Be("A");

				values.Remove(SomeKey);
				values.Get(SomeKey).Should().BeNull();
				values.GetAll().Should().BeEmpty();
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
