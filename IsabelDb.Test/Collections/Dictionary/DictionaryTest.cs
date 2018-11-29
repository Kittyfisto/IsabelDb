﻿using System;
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
		public void TestToString()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					db.GetDictionary<int, string>("Stuff").ToString().Should().Be("Dictionary<System.Int32, System.String>(\"Stuff\")");
				}
			}
		}

		[Test]
		public void TestMove1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Values");
				values.Put("b", 42);
				values.Move("a", "b");
				values.Get("b").Should().Be(42);
			}
		}

		[Test]
		public void TestMove2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Values");
				values.Put("a", 42);
				values.Get("a").Should().Be(42);

				values.Move("a", "b");
				values.Get("b").Should().Be(42);
				new Action(() => values.Get("a")).Should().Throw<KeyNotFoundException>();
			}
		}

		[Test]
		public void TestMove3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Values");
				values.Put("a", 42);
				values.Put("b", 9001);
				values.Get("a").Should().Be(42);
				values.Get("b").Should().Be(9001);

				values.Move("a", "b");
				values.Get("b").Should().Be(42);
				new Action(() => values.Get("a")).Should().Throw<KeyNotFoundException>();
			}
		}

		[Test]
		public void TestMove4()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<int, object>("Values");
				values.Put(1, "Parkway Drive");
				values.Put(2, "Wishing Well");

				values.Move(1, 1);
				values.Get(1).Should().Be("Parkway Drive");
				values.Get(2).Should().Be("Wishing Well");
			}
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
		[Description("Verifies that a dictionary cannot be retrieved if it's key type is unresolved")]
		public void TestUnresolvableKeyType()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(CustomKey)))
				{
					db.GetDictionary<CustomKey, string>("Pies");
				}

				using (var db = CreateDatabase(connection, NoCustomTypes))
				{
					new Action(() => db.GetDictionary<CustomKey, string>("Pies"))
						.Should().Throw<TypeCouldNotBeResolvedException>()
						.WithMessage("A Dictionary named 'Pies' already exists but it's key type could not be resolved: If your intent is to re-use this existing collection, then you need to add 'IsabelDb.Test.Entities.CustomKey' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!");

					var collection = db.Collections.First();
					collection.Name.Should().Be("Pies");
					collection.KeyType.Should().BeNull("because the key type couldn't be resolved");
					collection.KeyTypeName.Should().Be("IsabelDb.Test.Entities.CustomKey");
					collection.ValueType.Should().Be<string>();
					collection.ValueTypeName.Should().Be("System.String");
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
						.Should().Throw<TypeCouldNotBeResolvedException>()
						.WithMessage("A Dictionary named 'MoreKeys' already exists but it's value type could not be resolved: If your intent is to re-use this existing collection, then you need to add 'IsabelDb.Test.Entities.CustomKey' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!");

					var collection = db.Collections.First();
					collection.Name.Should().Be("MoreKeys");
					collection.KeyType.Should().Be<int>();
					collection.KeyTypeName.Should().Be("System.Int32");
					collection.ValueType.Should().BeNull("because the value type couldn't be resolved");
					collection.ValueTypeName.Should().Be("IsabelDb.Test.Entities.CustomKey");
				}
			}
		}

		[Test]
		public void TestRemoveMany1()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = db.GetDictionary<int, string>("Stuff");
					collection.Put(1, "1");
					collection.Put(2, "2");
					collection.GetAllValues().Should().Equal("1", "2");

					collection.RemoveMany(new []{2});
					collection.GetAllValues().Should().Equal("1");
				}
			}
		}

		[Test]
		public void TestRemoveMany2()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "1");
				collection.Put(2, "2");
				collection.GetAllValues().Should().Equal("1", "2");

				collection.RemoveMany(new []{3, 2, 1});
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestContainsKey()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "1");
				collection.Put(2, "2");

				collection.ContainsKey(1).Should().BeTrue();
				collection.ContainsKey(2).Should().BeTrue();
				collection.ContainsKey(0).Should().BeFalse();
				collection.ContainsKey(3).Should().BeFalse();
			}
		}

		[Test]
		public void TestGetAllKeysEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.GetAllKeys().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllKeys1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "a");
				collection.Put(2, "b");

				collection.GetAllKeys().Should().Equal(1, 2);
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
