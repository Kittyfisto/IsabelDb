using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.OrderedCollection
{
	[TestFixture]
	public sealed class OrderedCollectionTest
		: AbstractCollectionTest<IOrderedCollection<int, string>>
	{
		private int _lastKey;

		[SetUp]
		public void Setup()
		{
			_lastKey = 0;
		}

		[Test]
		public void TestToString()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					db.GetOrderedCollection<int, string>("Stuff").ToString().Should().Be("OrderedCollection<System.Int32, System.String>(\"Stuff\")");
				}
			}
		}

		[Test]
		public void TestGetGetOrderedCollectionDifferentKeyTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetOrderedCollection<int, string>("Names");
				new Action(() => db.GetOrderedCollection<uint, string>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The OrderedCollection 'Names' uses keys of type 'System.Int32' which does not match the requested key type 'System.UInt32': If your intent was to create a new OrderedCollection then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetGetOrderedCollectionDifferentValueTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetOrderedCollection<int, string>("Names");
				new Action(() => db.GetOrderedCollection<int, int>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The OrderedCollection 'Names' uses values of type 'System.String' which does not match the requested value type 'System.Int32': If your intent was to create a new OrderedCollection then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetValuesInRange()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");
				collection.Put(Math.E, "E");
				collection.Put(Math.PI, "PI");

				collection.GetValues(Interval.Create(0, 3.15)).Should().Equal("E", "PI");
				collection.GetValues(Interval.Create(3.14, 3.15)).Should().Equal("PI");
				collection.GetValues(Interval.Create(-42d, 2)).Should().BeEmpty();
				collection.GetValues(Interval.Create(4, 42d)).Should().BeEmpty();
			}
		}

		[Test]
		[Description("Verifies that Puts overwrite existing values with the same key")]
		public void TestPutSameKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");
				collection.Put(Math.PI, "1");
				collection.Put(Math.PI, "2");

				collection.GetValues(Interval.Create(Math.PI))
				          .Should().Equal(new object[] {"2"});
			}
		}

		[Test]
		public void TestPutMany()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");

				var values = new List<KeyValuePair<double, string>>();
				values.Add(new KeyValuePair<double, string>(Math.E, "E"));
				values.Add(new KeyValuePair<double, string>(Math.PI, "PI"));
				collection.PutMany(values);

				collection.Count().Should().Be(2);
				collection.GetAllValues().Should().Equal("E", "PI");
				collection.GetValues(Interval.Create(Math.E)).Should().Equal("E");
			}
		}

		[Test]
		public void TestRemoveRange1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");

				collection.Put(Math.E, "E");
				collection.Put(Math.PI, "PI");
				collection.Count().Should().Be(2);

				collection.RemoveRange(Interval.Create(-42, 2.6));
				collection.Count().Should().Be(2);
				collection.GetAllValues().Should().Equal("E", "PI");
			}
		}

		[Test]
		public void TestRemoveRange2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");

				collection.Put(Math.E, "E");
				collection.Put(Math.PI, "PI");

				collection.RemoveRange(Interval.Create(2, 3.1));
				collection.GetAllValues().Should().Equal("PI");
			}
		}

		[Test]
		public void TestCustomType()
		{
			using (var db = Database.CreateInMemory(new[] { typeof(MySortableKey) }))
			{
				new Action(() => db.GetOrderedCollection<MySortableKey, string>("Values"))
					.Should().Throw<NotSupportedException>("Custom types cannot be used as sortable keys")
					.WithMessage("The type 'IsabelDb.Test.Entities.MySortableKey' may not be used as a key in an ordered collection! Only basic numeric types can be used for now.");
			}
		}

		[Test]
		public void TestStringType()
		{
			using (var db = Database.CreateInMemory(new[] { typeof(MySortableKey) }))
			{
				new Action(() => db.GetOrderedCollection<string, string>("Values"))
					.Should().Throw<NotSupportedException>("Custom types cannot be used as sortable keys")
					.WithMessage("The type 'System.String' may not be used as a key in an ordered collection! Only basic numeric types can be used for now.");
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

		#region Overrides of AbstractCollectionTest<IOrderedCollection<int,string>>

		protected override CollectionType CollectionType => CollectionType.OrderedCollection;

		protected override IOrderedCollection<int, string> GetCollection(IDatabase db, string name)
		{
			return db.GetOrderedCollection<int, string>(name);
		}

		protected override void Put(IOrderedCollection<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _lastKey), value);
		}

		protected override void PutMany(IOrderedCollection<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, string>>(values.Length);
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, string>(Interlocked.Increment(ref _lastKey), value));
			}
			collection.PutMany(pairs);
		}

		protected override void RemoveLastPutValue(IOrderedCollection<int, string> collection)
		{
			collection.RemoveRange(Interval.Create(_lastKey, _lastKey));
		}

		#endregion

		private void TestKeyLimits<TKey>(TKey minimum, TKey maximum) where TKey : IComparable<TKey>
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<TKey, string>("Values");
				collection.Put(minimum, "Helo");
				collection.Put(maximum, "Boomer");

				collection.Count().Should().Be(2);
				collection.GetValues(Interval.Create(minimum)).Should().Equal("Helo");
				collection.GetValues(Interval.Create(maximum)).Should().Equal("Boomer");
				collection.GetValues(Interval.Create(minimum, maximum)).Should().Equal("Helo", "Boomer");
			}
		}
	}
}
