using System;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections
{
	public abstract class AbstractCollectionTest<TCollection>
		where TCollection : ICollection<string>
	{
		protected static readonly Type[] NoCustomTypes = new Type[0];

		[Test]
		public void TestGetSameCollectionTwice()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection1 = GetCollection(db, "Values1");
				var collection2 = GetCollection(db, "Values1");
				var collection3 = GetCollection(db, "Values2");

				collection1.Should().BeSameAs(collection2);
				collection1.Should().NotBeSameAs(collection3);
			}
		}

		[Test]
		public void TestGetAllValuesAgain()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "SomeTable");
				PutMany(collection, "foo", "bar");

				collection = GetCollection(db, "SomeTable");
				collection.GetAllValues().Should().Equal("foo", "bar");
			}
		}

		[Test]
		[Description("Verifies that collection names are case sensitive")]
		public void TestGetDictionaryCaseSensitiveNames()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var a = GetCollection(db, "A");
				var b = GetCollection(db, "a");
				a.Should().NotBe(b);

				Put(a, "42");
				Put(b, "50");

				a.GetAllValues().Should().Equal("42");
				b.GetAllValues().Should().Equal("50");
			}
		}

		[Test]
		[Description("Verifies that clearing an empty collection is allowed")]
		public void TestClearEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "Foo");
				collection.Count().Should().Be(1);
				collection.GetAllValues().Should().HaveCount(1);

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearManyItems()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "A");
				Put(collection, "B");
				Put(collection, "C");
				collection.Count().Should().Be(3);
				collection.GetAllValues().Should().HaveCount(3);

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestCountEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				collection.Count().Should().Be(0);
			}
		}

		[Test]
		public void TestGetAllValuesEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllValuesOneValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "Monty");
				collection.GetAllValues().Should().Equal("Monty");
			}
		}
		
		[Test]
		public void TestGetAllValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "Monty");
				Put(collection, "A");
				Put(collection, "B");
				collection.GetAllValues().Should().Equal("Monty", "A", "B");
			}
		}

		[Test]
		public void TestCountOne()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "Monty Python");
			}
		}

		protected abstract TCollection GetCollection(Database db, string name);
		protected abstract void Put(TCollection collection, string value);
		protected abstract void PutMany(TCollection collection, params string[] values);
	}
}