using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.MultiValueDictionaryStore
{
	[TestFixture]
	public sealed class MultiValueDictionaryTest
	{
		private static readonly Type[] NoCustomTypes = new Type[0];

		[Test]
		public void TestEmpty()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Count().Should().Be(0);
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearEmpty()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Count().Should().Be(0);
				values.Clear();
				values.Count().Should().Be(0);
			}
		}

		[Test]
		public void TestClear()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(1, "Bar");
				values.Put(2, "Hello, World!");
				values.Count().Should().Be(3);
			}
		}

		[Test]
		public void TestGetNonExistantKey()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Get(0).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetManyKeys()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foobar");
			}
		}

		[Test]
		public void TestPutValues()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				const int count = 10000;

				values.PutMany(1, Enumerable.Range(1, count).Select(x => x.ToString()));
				values.Count().Should().Be(count);
			}
		}
	}
}