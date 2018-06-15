using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.MultiValueDictionary
{
	[TestFixture]
	public sealed class MultiValueDictionaryTest
		: AbstractCollectionTest<IMultiValueDictionary<int, string>>
	{
		private int _lastKey;

		[SetUp]
		public void Setup()
		{
			_lastKey = 0;
		}

		[Test]
		public void TestGetGetMultiValueDictionaryDifferentKeyTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetMultiValueDictionary<int, string>("Names");
				new Action(() => db.GetMultiValueDictionary<uint, string>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The MultiValueDictionary 'Names' uses keys of type 'System.Int32' which does not match the requested key type 'System.UInt32': If your intent was to create a new MultiValueDictionary then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetGetMultiValueDictionaryDifferentValueTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetMultiValueDictionary<int, string>("Names");
				new Action(() => db.GetMultiValueDictionary<int, int>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The MultiValueDictionary 'Names' uses values of type 'System.String' which does not match the requested value type 'System.Int32': If your intent was to create a new MultiValueDictionary then you have to pick a new name!");
			}
		}

		[Test]
		public void TestEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Count().Should().Be(0);
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveAllNonExistantKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");

				values.RemoveAll(2);
				values.GetValues(1).Should().Equal("Foo");
			}
		}

		[Test]
		public void TestRemoveAll()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(2, "Bar");

				values.RemoveAll(1);
				values.GetValues(1).Should().BeEmpty();
				values.GetValues(2).Should().Equal("Bar");
			}
		}

		[Test]
		public void TestRemoveAllManyValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(1, "Bar");
				values.Put(2, "Hello");

				values.RemoveAll(1);
				values.GetValues(1).Should().BeEmpty();
				values.GetValues(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestGetNonExistantKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.GetValues(0).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetManyKeys()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, double>("Values");
				values.Put(1, Math.E);
				values.Put(2, Math.PI);
				values.Put(3, 1337);
				values.Put(4, 9001);

				var actualValues = values.GetValues(new[] {2, 3});
				actualValues.Should().Equal(Math.PI, 1337);
			}
		}

		[Test]
		public void TestPutOneValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foobar");
			}
		}

		[Test]
		public void TestPutValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "Foo");
				values.Put(1, "Bar");
				values.Put(2, "Hello");

				values.GetValues(1).Should().Equal("Foo", "Bar");
				values.GetValues(2).Should().Equal("Hello");
			}
		}

		[Test]
		[Description("Verifies that the same value can be added multiple times")]
		public void TestPutEqualValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.Put(1, "a");
				values.Put(1, "a");
				values.Put(2, "a");

				values.GetValues(1).Should().Equal("a", "a");
				values.GetValues(2).Should().Equal("a");
			}
		}

		[Test]
		public void TestPutManySameKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(1, new[] {"a", "b"});
				values.PutMany(1, new []{"c", "d"});

				values.GetValues(1).Should().Equal("a", "b", "c", "d");
			}
		}

		[Test]
		public void TestPutManyValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(1, new []{"Foo", "Bar"});
				values.PutMany(2, new []{"Hello"});

				values.GetValues(1).Should().Equal("Foo", "Bar");
				values.GetValues(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestPutManyValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				values.PutMany(new []
				{
					new KeyValuePair<int, IEnumerable<string>>(1, new[]{"a", "b"}),
					new KeyValuePair<int, IEnumerable<string>>(2, new[]{"Hello"})
				});

				values.Count().Should().Be(3);
				values.GetValues(1).Should().Equal("a", "b");
				values.GetValues(2).Should().Equal("Hello");
			}
		}

		[Test]
		public void TestPutManyValues3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetMultiValueDictionary<int, string>("Values");
				const int count = 10000;

				values.PutMany(1, Enumerable.Range(1, count).Select(x => x.ToString()));
				values.Count().Should().Be(count);
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
		public void TestPutMany2ReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = db.GetMultiValueDictionary<int, string>("Stuff");
					collection.Put(1, "One");
					collection.Put(1, "Two");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = db.GetMultiValueDictionary<int, string>("Stuff");
					collection.GetAllValues().Should().Equal("One", "Two");

					new Action(() => collection.PutMany(1, new []{"Three"}))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One", "Two");
				}
			}
		}

		[Test]
		public void TestRemoveAllReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = db.GetMultiValueDictionary<int, string>("Stuff");
					collection.Put(1, "One");
					collection.Put(1, "Two");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = db.GetMultiValueDictionary<int, string>("Stuff");
					collection.GetAllValues().Should().Equal("One", "Two");

					new Action(() => collection.RemoveAll(1))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One", "Two");
				}
			}
		}
		
		[Test]
		public void TestPutManyRemoved()
		{
			using (var connection = CreateConnection())
			using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
			{
				var collection = db.GetMultiValueDictionary<int, string>("Blessthefall");
				collection.Put(1, "Wishful Sinking");
				db.Remove(collection);
				new Action(() => collection.PutMany(2, new[] {"Find Yourself", "Sakura Blues"}))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("This collection has been removed from the database and may no longer be modified");
			}
		}

		[Test]
		public void TestContainsKey()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = db.GetMultiValueDictionary<int, string>("Blessthefall");
				collection.Put(1, "1");
				collection.PutMany(2, new []{"2", "3"});

				collection.ContainsKey(1).Should().BeTrue();
				collection.ContainsKey(2).Should().BeTrue();
				collection.ContainsKey(0).Should().BeFalse();
				collection.ContainsKey(3).Should().BeFalse();
			}
		}

		protected override CollectionType CollectionType => CollectionType.MultiValueDictionary;

		protected override IMultiValueDictionary<int, string> GetCollection(IDatabase db, string name)
		{
			return db.GetMultiValueDictionary<int, string>(name);
		}

		protected override void Put(IMultiValueDictionary<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _lastKey), value);
		}

		protected override void PutMany(IMultiValueDictionary<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, IEnumerable<string>>>();
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, IEnumerable<string>>(Interlocked.Increment(ref _lastKey), new []{value}));
			}
			collection.PutMany(pairs);
		}

		protected override void RemoveLastPutValue(IMultiValueDictionary<int, string> collection)
		{
			collection.RemoveAll(_lastKey);
		}

		private void TestKeyLimits<TKey>(TKey minimum, TKey maximum) where TKey : IComparable<TKey>
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetMultiValueDictionary<TKey, string>("Values");
				collection.Put(minimum, "Helo");
				collection.Put(maximum, "Boomer");

				collection.Count().Should().Be(2);
				collection.GetValues(minimum).Should().Equal(new object[]{"Helo"});
				collection.GetValues(maximum).Should().Equal(new object[]{"Boomer"});
				collection.GetValues(new[] {minimum, maximum}).Should().Equal(new object[] {"Helo", "Boomer"});
			}
		}
	}
}