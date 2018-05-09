using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.IntervalCollection
{
	[TestFixture]
	public sealed class IntervalCollectionTest
	{
		private static readonly Type[] NoCustomTypes = new Type[0];

		[Test]
		public void TestGetEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.GetAll().Should().BeEmpty();
				values.Clear();
				values.GetAll().Should().BeEmpty();
			}
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
		public void TestGetAllValuesEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(10, 20), "Foo");
				var all = values.GetAll().ToList();
				all.Should().HaveCount(1);
				all[0].Key.Should().Be(Interval.Create(10, 20));
				all[0].Value.Should().Be("Foo");
			}
		}

		[Test]
		public void TestGetAllValuesOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(10, 20), "Foo");
				values.GetAllValues().Should().Equal("Foo");
			}
		}

		[Test]
		public void TestClearOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(10, 20), "Foo");
				values.GetAll().Should().HaveCount(1);
				values.Clear();
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(Interval.Create(1, 10), "Hello");

				for (int i = 1; i <= 10; ++i)
				{
					values.GetValues(i).Should().Equal("Hello");
				}

				values.GetValues(0).Should().BeEmpty();
				values.GetValues(11).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "Hello");

				values.GetValues(0, 11).Should().Equal("Hello");
				values.GetValues(5, 6).Should().Equal("Hello");
				values.GetValues(-1, 0).Should().BeEmpty();
				values.GetValues(11, 100).Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveNonIntersectingInterval1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "A");
				values.Put(new Interval<int>(2, 11), "B");
				values.Put(new Interval<int>(3, 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(13);
				values.GetAllValues().Should().Equal("A", "B", "C");
			}
		}

		[Test]
		public void TestRemoveNonIntersectingInterval2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "A");
				values.Put(new Interval<int>(2, 11), "B");
				values.Put(new Interval<int>(3, 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(Interval.Create(13, 14));
				values.GetAllValues().Should().Equal("A", "B", "C");
			}
		}

		[Test]
		public void TestRemoveIntersectingValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "A");
				values.Put(new Interval<int>(2, 11), "B");
				values.Put(new Interval<int>(3, 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(2);
				values.GetAllValues().Should().Equal("C");
			}
		}

		[Test]
		public void TestRemoveIntersectingValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "A");
				values.Put(new Interval<int>(2, 11), "B");
				values.Put(new Interval<int>(3, 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(Interval.Create(1, 2));
				values.GetAllValues().Should().Equal("C");
			}
		}

		[Test]
		public void TestRemoveNonExistingValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "Hello");

				values.Remove(new ValueKey(34214121));
				values.GetValues(5).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestRemoveOneSpecificValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				values.Put(new Interval<int>(1, 10), "A");
				var b= values.Put(new Interval<int>(2, 11), "B");
				values.Put(new Interval<int>(3, 12), "C");

				values.GetAllValues().Should().Equal("A", "B", "C");
				values.Remove(b);
				values.GetAllValues().Should().Equal("A", "C");
			}
		}
	}
}
