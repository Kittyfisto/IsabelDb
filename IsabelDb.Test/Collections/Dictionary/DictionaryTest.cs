using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class DictionaryTest
		: AbstractCollectionTest<IDictionary<int, string>>
	{
		private int _lastKey;

		[SetUp]
		public void SetUp()
		{
			_lastKey = 0;
		}

		[Test]
		public void TestGet1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", "bar");
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().Be("bar");
				db.GetDictionary<string, object>("SomeTable").TryGet("foo", out var value).Should().BeTrue();
				value.Should().Be("bar");
			}
		}

		[Test]
		public void TestGetManyValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.Put(3, "C");
				collection.Put(4, "D");
				collection.GetManyValues(new[] {2, 3}).Should().Equal("B", "C");
			}
		}

		[Test]
		public void TestGetManyValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.GetManyValues(new[] {0, 1}).Should().Equal("A");
				collection.GetManyValues(new[] {2, 3}).Should().Equal("B");
			}
		}

		[Test]
		public void TestGetManyValues3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.GetManyValues(new[] {0, 3, 5}).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetDictionaryDifferentKeyTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, string>("Names");
				new Action(() => db.GetDictionary<int, string>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The Dictionary 'Names' uses keys of type 'System.String' which does not match the requested key type 'System.Int32': If your intent was to create a new Dictionary then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetDictionaryDifferentValueTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, string>("Names");
				new Action(() => db.GetDictionary<string, int>("Names"))
					.Should().Throw<TypeMismatchException>()
					.WithMessage("The Dictionary 'Names' uses values of type 'System.String' which does not match the requested value type 'System.Int32': If your intent was to create a new Dictionary then you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetNone()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").TryGet("foo", out var unused).Should().BeFalse();
				db.GetDictionary<string, object>("SomeTable").GetMany("foo", "bar").Should().BeEmpty();
			}
		}

		[Test]
		[Defect("https://github.com/Kittyfisto/IsabelDb/issues/1")]
		[Description("Verifies that collections skip un-deserializable values")]
		public void TestUnresolvableValue()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					var values = db.GetDictionary<int, object>("Foo");
					values.Put(key: 0, value: 42);
					values.Put(key: 1, value: new CustomKey {A = 42});
					values.Put(key: 2, value: "Hello, World!");
				}

				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					var values = db.GetDictionary<int, object>("Foo");
					var allValues = values.GetAll();
					allValues.Count().Should().Be(3, because: "because we're still able to resolve all types");
				}

				using (var db = CreateDatabase(connection, NoCustomTypes))
				{
					var values = db.GetDictionary<int, object>("Foo");
					var allValues = values.GetAll();
					allValues.Count().Should().Be(2, because: "because we're no longer able to resolve CustomKey");
					allValues.ElementAt(index: 0).Key.Should().Be(0);
					allValues.ElementAt(index: 0).Value.Should().Be(42);
					allValues.ElementAt(index: 1).Key.Should().Be(2);
					allValues.ElementAt(index: 1).Value.Should().Be("Hello, World!");
				}
			}
		}

		[Test]
		[Defect("https://github.com/Kittyfisto/IsabelDb/issues/1")]
		[Description("Verifies that a dictionary cannot be retrieved if it's value type is unresolved")]
		public void TestUnresolvableValueType()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					var dictionary = db.GetDictionary<int, CustomKey>("MoreKeys");
					dictionary.Put(key: 1, value: new CustomKey {C = 2});
					dictionary.Put(key: 2, value: new CustomKey {D = -2});
				}

				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					var dictionary = db.GetDictionary<int, CustomKey>("MoreKeys");
					dictionary.GetAll().Count().Should().Be(2);
				}

				using (var db = CreateDatabase(connection, NoCustomTypes))
				{
					new Action(() => db.GetDictionary<int, CustomKey>("MoreKeys"))
						.Should().Throw<TypeResolveException>()
						.WithMessage("A Dictionary named 'MoreKeys' already exists but it's value type could not be resolved: If your intent is to re-use this existing collection, then you need to add 'IsabelDb.Test.Entities.CustomKey' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!");
				}
			}
		}

		protected override CollectionType CollectionType => CollectionType.Dictionary;

		protected override IDictionary<int, string> GetCollection(IDatabase db, string name)
		{
			return db.GetDictionary<int, string>(name);
		}

		protected override void Put(IDictionary<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _lastKey), value);
		}

		protected override void PutMany(IDictionary<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, string>>(values.Length);
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, string>(Interlocked.Increment(ref _lastKey), value));
			}
			collection.PutMany(pairs);
		}

		protected override void RemoveLastPutValue(IDictionary<int, string> collection)
		{
			collection.Remove(_lastKey);
		}
	}
}
