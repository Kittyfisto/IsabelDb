using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.IntervalCollection
{
	[TestFixture]
	public sealed class IntervalCollectionTest
		: AbstractCollectionTest<IIntervalCollection<int, string>>
	{
		protected override IIntervalCollection<int, string> GetCollection(Database db, string name)
		{
			return db.GetIntervalCollection<int, string>(name);
		}

		protected override void Put(IIntervalCollection<int, string> collection, string value)
		{
			collection.Put(Interval.Create(minimum: 1, maximum: 2), value);
		}

		protected override void PutMany(IIntervalCollection<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<Interval<int>, string>>(values.Length);
			foreach (var value in values)
			{
				var interval = Interval.Create(1, 2);
				pairs.Add(new KeyValuePair<Interval<int>, string>(interval, value));
			}
			collection.PutMany(pairs);
		}

		[Test]
		public void TestGetAllEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(minimum: 10, maximum: 20), "Foo");
				var all = values.GetAll().ToList();
				all.Should().HaveCount(expected: 1);
				all[index: 0].Key.Should().Be(Interval.Create(minimum: 10, maximum: 20));
				all[index: 0].Value.Should().Be("Foo");
			}
		}

		[Test]
		public void TestGetValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(minimum: 1, maximum: 10), "Hello");

				for (var i = 1; i <= 10; ++i) values.GetValues(i).Should().Equal("Hello");

				values.GetValues(key: 0).Should().BeEmpty();
				values.GetValues(key: 11).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "Hello");

				values.GetValues(minimum: 0, maximum: 11).Should().Equal("Hello");
				values.GetValues(minimum: 5, maximum: 6).Should().Equal("Hello");
				values.GetValues(minimum: -1, maximum: 0).Should().BeEmpty();
				values.GetValues(minimum: 11, maximum: 100).Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveIntersectingValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "A");
				values.Put(new Interval<int>(minimum: 2, maximum: 11), "B");
				values.Put(new Interval<int>(minimum: 3, maximum: 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(key: 2);
				values.GetAllValues().Should().Equal("C");
			}
		}

		[Test]
		public void TestRemoveIntersectingValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "A");
				values.Put(new Interval<int>(minimum: 2, maximum: 11), "B");
				values.Put(new Interval<int>(minimum: 3, maximum: 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(Interval.Create(minimum: 1, maximum: 2));
				values.GetAllValues().Should().Equal("C");
			}
		}

		[Test]
		public void TestRemoveNonExistingValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "Hello");

				values.Remove(new ValueKey(value: 34214121));
				values.GetValues(key: 5).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestRemoveNonIntersectingInterval1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "A");
				values.Put(new Interval<int>(minimum: 2, maximum: 11), "B");
				values.Put(new Interval<int>(minimum: 3, maximum: 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(key: 13);
				values.GetAllValues().Should().Equal("A", "B", "C");
			}
		}

		[Test]
		public void TestRemoveNonIntersectingInterval2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "A");
				values.Put(new Interval<int>(minimum: 2, maximum: 11), "B");
				values.Put(new Interval<int>(minimum: 3, maximum: 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(Interval.Create(minimum: 13, maximum: 14));
				values.GetAllValues().Should().Equal("A", "B", "C");
			}
		}

		[Test]
		public void TestRemoveOneSpecificValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(minimum: 1, maximum: 10), "A");
				var b = values.Put(new Interval<int>(minimum: 2, maximum: 11), "B");
				values.Put(new Interval<int>(minimum: 3, maximum: 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(b);
				values.GetAllValues().Should().Equal("A", "C");
			}
		}
	}
}