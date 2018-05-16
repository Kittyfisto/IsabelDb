using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public abstract class AbstractDictionaryObjectStoreTest<TKey>
	{
		protected abstract IEnumerable<Type> CustomTypes { get; }
		protected abstract TKey SomeKey { get; }
		protected abstract TKey DifferentKey { get; }
		protected abstract IReadOnlyList<TKey> ManyKeys { get; }

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var keys = new HashSet<TKey>();
			foreach (var key in ManyKeys)
				if (!keys.Add(key))
					Assert.Inconclusive("Don't provide duplicate keys!");
		}

		[Test]
		public void TestMove()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "Noether's Theorem");
				values.Put(DifferentKey, "The Symmetries of Reality");
				values.Get(SomeKey).Should().Be("Noether's Theorem");
				values.Get(DifferentKey).Should().Be("The Symmetries of Reality");

				values.Move(SomeKey, DifferentKey);
				new Action(() => values.Get(SomeKey)).Should().Throw<KeyNotFoundException>();
				values.Get(DifferentKey).Should().Be("Noether's Theorem");
			}
		}

		[Test]
		public void TestGetKey()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "Hello");
				values.Put(DifferentKey, "World");

				var allValues = values.GetAll();
				allValues.Should().HaveCount(2);
				allValues.ElementAt(index: 0).Key.Should().Be(SomeKey);
				allValues.ElementAt(index: 0).Value.Should().Be("Hello");
				allValues.ElementAt(index: 1).Key.Should().Be(DifferentKey);
				allValues.ElementAt(index: 1).Value.Should().Be("World");
			}
		}

		[Test]
		public void TestGetNonExistantKey()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.TryGet(SomeKey, out var unused).Should().BeFalse();
			}
		}

		[Test]
		public void TestOverwriteValue()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, value: 42);
				values.Get(SomeKey).Should().Be(42);
				values.TryGet(SomeKey, out var value).Should().BeTrue();
				value.Should().Be(42);

				values.Put(SomeKey, value: 9001);
				values.Get(SomeKey).Should().Be(9001);
				values.TryGet(SomeKey, out value).Should().BeTrue();
				value.Should().Be(9001);
			}
		}

		[Test]
		public void TestOverwriteValueWithNull()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "A");
				values.Get(SomeKey).Should().Be("A");

				values.Put(SomeKey, value: null);
				new Action(() => values.Get(SomeKey)).Should().Throw<KeyNotFoundException>();
				values.TryGet(SomeKey, out var unused).Should().BeFalse();
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutManyKeys()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				var i = 0;
				foreach (var key in ManyKeys)
				{
					values.Put(key, i);
					++i;
				}

				i = 0;
				foreach (var key in ManyKeys)
				{
					values.Get(key).Should().Be(i);
					values.TryGet(key, out var value).Should().BeTrue();
					value.Should().Be(i);
					++i;
				}

				var actual = values.GetAll().ToDictionary(x => x.Key, x => x.Value);
				foreach (var key in ManyKeys)
				{
					actual.Should().ContainKey(key);
				}
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "A");
				values.Get(SomeKey).Should().Be("A");

				values.Remove(SomeKey);
				new Action(() => values.Get(SomeKey)).Should().Throw<KeyNotFoundException>();
				values.TryGet(SomeKey, out var unused).Should().BeFalse();
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestStoreTwoValues()
		{
			using (var db = Database.CreateInMemory(CustomTypes))
			{
				var values = db.GetDictionary<TKey, object>("Values");
				values.Put(SomeKey, "Hello");
				values.Put(DifferentKey, "World");

				values.Get(SomeKey).Should().Be("Hello");
				values.TryGet(SomeKey, out var value).Should().BeTrue();
				value.Should().Be("Hello");
				values.Get(DifferentKey).Should().Be("World");
				values.TryGet(DifferentKey, out value).Should().BeTrue();
				value.Should().Be("World");
			}
		}
	}
}