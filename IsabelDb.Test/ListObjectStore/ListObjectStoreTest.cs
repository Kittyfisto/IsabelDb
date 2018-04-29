using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.ListObjectStore
{
	public sealed class ListObjectStoreTest
	{
		[Test]
		public void TestClearEmpty()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var bag = db.GetBag<string>("foo");
				new Action(() => bag.Clear()).Should().NotThrow();
			}
		}

		[Test]
		public void TestClearOne()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var bag = db.GetBag<string>("foo");
				bag.Put("Hello, World!");
				bag.Count().Should().Be(1);

				bag.Clear();
				bag.Count().Should().Be(0);
				bag.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearMany()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var bag = db.GetBag<int>("foo");
				bag.PutMany(Enumerable.Range(0, 1000));
				bag.Count().Should().Be(1000);

				bag.Clear();
				bag.Count().Should().Be(0);
				bag.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutSameValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var bag = db.GetBag<int>("my favourite numbers");
				bag.Put(42);
				bag.Put(100);
				bag.Put(42);

				bag.GetAll().Should().BeEquivalentTo(42, 100, 42);
			}
		}

		[Test]
		public void TestPutManyValues()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var bag = db.GetBag<int>("foo");
				bag.PutMany(1, 2, 3, 4);
				bag.Count().Should().Be(4);
				bag.GetAll().Should().BeEquivalentTo(1, 2, 3, 4);
			}
		}
	}
}
