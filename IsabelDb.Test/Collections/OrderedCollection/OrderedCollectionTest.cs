using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.OrderedCollection
{
	[TestFixture]
	public sealed class OrderedCollectionTest
		: AbstractCollectionTest<IOrderedCollection<int, string>>
	{
		private int _nextKey;

		[SetUp]
		public void Setup()
		{
			_nextKey = 0;
		}

		[Test]
		public void TestGetValuesInRange()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetOrderedCollection<double, string>("Values");
				collection.Put(Math.E, "E");
				collection.Put(Math.PI, "PI");

				collection.GetValuesInRange(Interval.Create(0, 3.15)).Should().Equal("E", "PI");
				collection.GetValuesInRange(Interval.Create(3.14, 3.15)).Should().Equal("PI");
				collection.GetValuesInRange(Interval.Create(-42d, 2)).Should().BeEmpty();
				collection.GetValuesInRange(Interval.Create(4, 42d)).Should().BeEmpty();
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
				collection.GetValuesInRange(Interval.Create(Math.E)).Should().Equal("E");
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

		#region Overrides of AbstractCollectionTest<IOrderedCollection<int,string>>

		protected override IOrderedCollection<int, string> GetCollection(Database db, string name)
		{
			return db.GetOrderedCollection<int, string>(name);
		}

		protected override void Put(IOrderedCollection<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _nextKey), value);
		}

		protected override void PutMany(IOrderedCollection<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, string>>(values.Length);
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, string>(Interlocked.Increment(ref _nextKey), value));
			}
			collection.PutMany(pairs);
		}

		#endregion
	}
}
