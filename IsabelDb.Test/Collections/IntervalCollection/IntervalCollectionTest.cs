using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.IntervalCollection
{
	[TestFixture]
	public sealed class IntervalCollectionTest
		: AbstractCollectionTest<IIntervalCollection<int, string>>
	{
		private int _lastId;

		protected override CollectionType CollectionType => CollectionType.IntervalCollection;

		protected override IIntervalCollection<int, string> GetCollection(IDatabase db, string name)
		{
			return db.GetIntervalCollection<int, string>(name);
		}

		protected override void Put(IIntervalCollection<int, string> collection, string value)
		{
			collection.Put(Interval.Create(Interlocked.Increment(ref _lastId)), value);
		}

		protected override void PutMany(IIntervalCollection<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<Interval<int>, string>>(values.Length);
			foreach (var value in values)
			{
				var interval = Interval.Create(Interlocked.Increment(ref _lastId));
				pairs.Add(new KeyValuePair<Interval<int>, string>(interval, value));
			}
			collection.PutMany(pairs);
		}

		protected override void RemoveLastPutValue(IIntervalCollection<int, string> collection)
		{
			collection.Remove(_lastId);
		}

		[Test]
		public void TestToString()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					db.GetIntervalCollection<int, string>("Stuff").ToString().Should().Be("IntervalCollection<System.Int32, System.String>(\"Stuff\")");
				}
			}
		}

		[Test]
		public void TestByteKey()
		{
			TestKeyLimits(byte.MinValue, byte.MaxValue);
		}

		[Test]
		public void TestSByteKey()
		{
			TestKeyLimits(sbyte.MinValue, sbyte.MaxValue);
		}

		[Test]
		public void TestShortKey()
		{
			TestKeyLimits(short.MinValue, short.MaxValue);
		}

		[Test]
		public void TestUShortKey()
		{
			TestKeyLimits(ushort.MinValue, ushort.MaxValue);
		}

		[Test]
		public void TestIntKey()
		{
			TestKeyLimits(int.MinValue, int.MaxValue);
		}

		[Test]
		public void TestUIntKey()
		{
			TestKeyLimits(uint.MinValue, uint.MaxValue);
		}

		[Test]
		public void TestLongKey()
		{
			TestKeyLimits(long.MinValue, long.MaxValue);
		}

		[Test]
		public void TestULongKey()
		{
			TestKeyLimits(ulong.MinValue, ulong.MaxValue);
		}

		[Test]
		public void TestFloatKey()
		{
			TestKeyLimits(float.MinValue, float.MaxValue);
		}

		[Test]
		public void TestDoubleKey()
		{
			TestKeyLimits(double.MinValue, double.MaxValue);
		}

		[Test]
		public void TestGetIntervalCollectionDifferentKeyTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetIntervalCollection<int, string>("Names");
				new Action(() => db.GetIntervalCollection<uint, string>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The IntervalCollection 'Names' uses keys of type 'System.Int32' which does not match the requested key type 'System.UInt32': If your intent was to create a new IntervalCollection then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetIntervalCollectionDifferentValueTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetIntervalCollection<int, string>("Names");
				new Action(() => db.GetIntervalCollection<int, int>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The IntervalCollection 'Names' uses values of type 'System.String' which does not match the requested value type 'System.Int32': If your intent was to create a new IntervalCollection then you have to pick a new name!");
			}
		}

		[Test]
		public void TestCustomType()
		{
			using (var db = Database.CreateInMemory(new []{typeof(MySortableKey) }))
			{
				new Action(() => db.GetIntervalCollection<MySortableKey, string>("Values"))
					.Should().Throw<NotSupportedException>("Custom types cannot be used as sortable keys")
					.WithMessage("The type 'IsabelDb.Test.Entities.MySortableKey' may not be used as a key in an interval collection! Only basic numeric types can be used for now.");
			}
		}

		[Test]
		public void TestStringType()
		{
			using (var db = Database.CreateInMemory(new []{typeof(MySortableKey) }))
			{
				new Action(() => db.GetIntervalCollection<string, string>("Values"))
					.Should().Throw<NotSupportedException>("Custom types cannot be used as sortable keys")
					.WithMessage("The type 'System.String' may not be used as a key in an interval collection! Only basic numeric types can be used for now.");
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
		public void TestRemoveIntervalReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = db.GetIntervalCollection<int, string>("Stuff");
					collection.Put(Interval.Create(1), "One");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = db.GetIntervalCollection<int, string>("Stuff");
					collection.GetAllValues().Should().Equal("One");

					new Action(() => collection.Remove(Interval.Create(1)))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One");
				}
			}
		}

		private void TestKeyLimits<TKey>(TKey minimum, TKey maximum) where TKey : IComparable<TKey>
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetIntervalCollection<TKey, string>("Values");
				collection.Put(Interval.Create(minimum), "Helo");
				collection.Put(Interval.Create(maximum), "Boomer");

				collection.Count().Should().Be(2);
				collection.GetValues(minimum).Should().Equal("Helo");
				collection.GetValues(maximum).Should().Equal("Boomer");
				collection.GetValues(minimum, maximum).Should().Equal("Helo", "Boomer");
			}
		}
	}
}