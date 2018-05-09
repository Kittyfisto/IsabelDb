using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.MultiValueDictionary
{
	[TestFixture]
	public sealed class MultiValueDictionaryTest
		: AbstractCollectionTest<IMultiValueDictionary<int, string>>
	{
		private int _lastKey;

		[SetUp]
		public void Setup()
		{
			_lastKey = 0;
		}

		[Test]
		public void TestEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Count().Should().Be(0);
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveAllNonExistantKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");

				values.RemoveAll(2);
				values.Get(1).Should().Equal("Foo");
			}
		}

		[Test]
		public void TestRemoveAll()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(2, "Bar");

				values.RemoveAll(1);
				values.Get(1).Should().BeEmpty();
				values.Get(2).Should().Equal("Bar");
			}
		}

		[Test]
		public void TestRemoveAllManyValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(1, "Bar");
				values.Put(2, "Hello");

				values.RemoveAll(1);
				values.Get(1).Should().BeEmpty();
				values.Get(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestGetNonExistantKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Get(0).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetManyKeys()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, double>("Values");
				values.Put(1, Math.E);
				values.Put(2, Math.PI);
				values.Put(3, 1337);
				values.Put(4, 9001);

				var actualValues = values.GetMany(new[] {2, 3});
				actualValues.Should().HaveCount(2);
				actualValues.ElementAt(0).Should().Equal(Math.PI);
				actualValues.ElementAt(1).Should().Equal(1337);
			}
		}

		[Test]
		public void TestPutOneValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foobar");
			}
		}

		[Test]
		public void TestPutValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(1, "Bar");
				values.Put(2, "Hello");

				values.Get(1).Should().Equal("Foo", "Bar");
				values.Get(2).Should().Equal("Hello");
			}
		}

		[Test]
		[Description("Verifies that the same value can be added multiple times")]
		public void TestPutEqualValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "a");
				values.Put(1, "a");
				values.Put(2, "a");

				values.Get(1).Should().Equal("a", "a");
				values.Get(2).Should().Equal("a");
			}
		}

		[Test]
		public void TestPutManySameKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(1, new[] {"a", "b"});
				values.PutMany(1, new []{"c", "d"});

				values.Get(1).Should().Equal("a", "b", "c", "d");
			}
		}

		[Test]
		public void TestPutManyValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(1, new []{"Foo", "Bar"});
				values.PutMany(2, new []{"Hello"});

				values.Get(1).Should().Equal("Foo", "Bar");
				values.Get(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestPutManyValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(new []
				{
					new KeyValuePair<int, IEnumerable<string>>(1, new[]{"a", "b"}),
					new KeyValuePair<int, IEnumerable<string>>(2, new[]{"Hello"})
				});

				values.Count().Should().Be(3);
				values.Get(1).Should().Equal("a", "b");
				values.Get(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestPutManyValues3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				const int count = 10000;

				values.PutMany(1, Enumerable.Range(1, count).Select(x => x.ToString()));
				values.Count().Should().Be(count);
			}
		}

		protected override IMultiValueDictionary<int, string> GetCollection(Database db, string name)
		{
			return db.GetMultiValueDictionary<int, string>(name);
		}

		protected override void Put(IMultiValueDictionary<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _lastKey), value);
		}

		protected override void PutMany(IMultiValueDictionary<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, IEnumerable<string>>>();
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, IEnumerable<string>>(Interlocked.Increment(ref _lastKey), new []{value}));
			}
			collection.PutMany(pairs);
		}
	}
}