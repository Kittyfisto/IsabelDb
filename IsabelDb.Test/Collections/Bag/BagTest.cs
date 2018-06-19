using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Bag
{
	[TestFixture]
	public sealed class BagTest
		: AbstractCollectionTest<IBag<string>>
	{
		private RowId _lastRowId;

		protected override CollectionType CollectionType => CollectionType.Bag;

		protected override IBag<string> GetCollection(IDatabase db, string name)
		{
			return db.GetBag<string>(name);
		}

		protected override void Put(IBag<string> collection, string value)
		{
			_lastRowId = collection.Put(value);
		}

		protected override void PutMany(IBag<string> collection, params string[] values)
		{
			_lastRowId = collection.PutMany(values).Last();
		}

		protected override void RemoveLastPutValue(IBag<string> collection)
		{
			collection.Remove(_lastRowId);
		}

		[Test]
		[Description("Verifies that the database refuses to return an IBag object for bags who's value type cannot be resolved")]
		public void TestUnresolvableBagType()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					var bag = db.GetBag<CustomKey>("Keys");
					bag.Put(new CustomKey {A = 1});
					bag.Put(new CustomKey {B = 2});
				}

				using (var db = CreateDatabase(connection))
				{
					new Action(() => db.GetBag<CustomKey>("Keys"))
						.Should().Throw<TypeResolveException>()
						.WithMessage("A Bag named 'Keys' already exists but it's value type could not be resolved: If your intent is to re-use this existing collection, then you need to add 'IsabelDb.Test.Entities.CustomKey' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!");

					var collection = db.Collections.First();
					collection.KeyType.Should().BeNull("Because bags don't have keys");
					collection.KeyTypeName.Should().BeNull("Because bags don't have keys");
					collection.ValueType.Should().BeNull("because the value type couldn't be resolved");
					collection.ValueTypeName.Should().Be("IsabelDb.Test.Entities.CustomKey");
				}
			}
		}

		[Test]
		public void TestGetGetBagDifferentValueTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetBag<string>("Names");
				new Action(() => db.GetBag<int>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The Bag 'Names' uses values of type 'System.String' which does not match the requested value type 'System.Int32': If your intent was to create a new Bag then you have to pick a new name!");
			}
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

		[Test]
		public void TestGetEmptyRange()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<string>("foo");
				bag.Put("A");
				bag.Put("B");
				bag.Put("C");
				bag.Put("D");
				bag.Put("E");

				var min = new RowId(42);
				var max = new RowId(100);
				bag.GetValues(Interval.Create(min, max)).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetRange()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<string>("foo");
				bag.Put("A");
				var min = bag.Put("B");
				bag.Put("C");
				var max = bag.Put("D");
				bag.Put("E");

				bag.GetValues(Interval.Create(min, max)).Should().Equal("B", "C", "D");
			}
		}

		[Test]
		public void StressTest()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<int>("foo");
				const int count = 10000;
				var values = Enumerable.Range(0, count).ToList();
				bag.PutMany(values);

				bag.Count().Should().Be(count);
				bag.GetAllValues().Should().Equal(values);
			}
		}

		[Test]
		public void TestGetValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<double>("Values");
				var e = bag.Put(Math.E);
				var pi = bag.Put(Math.PI);

				bag.GetValue(e).Should().Be(Math.E);
				bag.GetValue(pi).Should().Be(Math.PI);
			}
		}

		[Test]
		public void TestGetManyValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<double>("Values");
				var e = bag.Put(Math.E);
				var pi = bag.Put(Math.PI);

				bag.GetManyValues(new[] {e, pi}).Should().Equal(Math.E, Math.PI);
			}
		}

		[Test]
		public void TestTryGetValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<double>("Values");
				var e = bag.Put(Math.E);
				var pi = bag.Put(Math.PI);

				bag.TryGetValue(pi, out var value).Should().BeTrue();
				value.Should().Be(Math.PI);
				bag.Remove(pi);
				bag.TryGetValue(pi, out value).Should().BeFalse();
			}
		}

		[Test]
		public void TestGetNonExistingValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<double>("Values");
				bag.Put(Math.E);
				new Action(() => bag.GetValue(new RowId(41421321)))
					.Should().Throw<KeyNotFoundException>();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var bag = db.GetBag<int>("foo");
				bag.Put(4);
				bag.Put(42);
				var id1 = bag.Put(1337);
				var id2 = bag.Put(9001);

				bag.GetAllValues().Should().Equal(4, 42, 1337, 9001);

				bag.Remove(id1);
				bag.GetAllValues().Should().Equal(4, 42, 9001);

				bag.Remove(id2);
				bag.GetAllValues().Should().Equal(4, 42);
			}
		}

		[Test]
		public void TestPutGetAllValuesReadOnly()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, NoCustomTypes))
				{
					var collection = db.GetBag<object>("Stuff");
					collection.Put(Math.E);
					collection.Put("Hello, World!");
					collection.Put("Architects");
					collection.Put(42);
				}

				using (var db = CreateReadOnlyDatabase(connection, NoCustomTypes))
				{
					var collection = db.GetBag<object>("Stuff");
					collection.GetAllValues().Should().Equal(Math.E, "Hello, World!", "Architects", 42);
				}
			}
		}

		[Test]
		public void TestPutGetSpecificValueReadOnly()
		{
			using (var connection = CreateConnection())
			{
				RowId id1, id2, id3, id4;

				using (var db = CreateDatabase(connection, NoCustomTypes))
				{
					var collection = db.GetBag<object>("Stuff");
					id1 = collection.Put(Math.E);
					id2 = collection.Put("Hello, World!");
					id3 = collection.Put("Architects");
					id4 = collection.Put(42);
				}

				using (var db = CreateReadOnlyDatabase(connection, NoCustomTypes))
				{
					var collection = db.GetBag<object>("Stuff");
					collection.GetValue(id1).Should().Be(Math.E);
					collection.GetValue(id2).Should().Be("Hello, World!");
					collection.GetValue(id3).Should().Be("Architects");
					collection.GetValue(id4).Should().Be(42);
				}
			}
		}
	}
}
