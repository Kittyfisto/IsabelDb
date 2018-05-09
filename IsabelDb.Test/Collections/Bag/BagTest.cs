using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Bag
{
	[TestFixture]
	public sealed class BagTest
		: AbstractCollectionTest<IBag<string>>
	{
		protected override IBag<string> GetCollection(Database db, string name)
		{
			return db.GetBag<string>(name);
		}

		protected override void Put(IBag<string> collection, string value)
		{
			collection.Put(value);
		}

		protected override void PutMany(IBag<string> collection, params string[] values)
		{
			collection.PutMany(values);
		}

		[Test]
		public void TestClearOne()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<string>("foo");
				bag.Put("Hello, World!");
				bag.Count().Should().Be(1);

				bag.Clear();
				bag.Count().Should().Be(0);
				bag.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearMany()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<int>("foo");
				bag.PutMany(Enumerable.Range(0, 1000));
				bag.Count().Should().Be(1000);

				bag.Clear();
				bag.Count().Should().Be(0);
				bag.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutSameValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<int>("my favourite numbers");
				bag.Put(42);
				bag.Put(100);
				bag.Put(42);

				bag.GetAllValues().Should().BeEquivalentTo(42, 100, 42);
			}
		}

		[Test]
		public void TestPutManyValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<int>("foo");
				bag.PutMany(1, 2, 3, 4);
				bag.Count().Should().Be(4);
				bag.GetAllValues().Should().BeEquivalentTo(1, 2, 3, 4);
			}
		}
	}
}
