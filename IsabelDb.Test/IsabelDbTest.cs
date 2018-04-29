using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class IsabelDbTest
	{
		private static void PutAndGet<T>(IsabelDb db, T value1, T value2)
		{
			var store = db.GetDictionary<string, object>("SomeTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(expected: 2);
			var actualValue1 = persons.ElementAt(index: 0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(index: 1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}

		[Test]
		[Description("Verifies that clearing an empty dictionary is allowed")]
		public void TestClearEmpty()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var people = db.GetDictionary<string, object>("People");
				people.Count().Should().Be(expected: 0);
				people.Clear();
				people.Count().Should().Be(expected: 0);
			}
		}

		[Test]
		public void TestCreateInMemory()
		{
			IsabelDb.CreateInMemory();
		}

		[Test]
		public void TestGet1()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", "bar");
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().Be("bar");
			}
		}

		[Test]
		[Description("Verifies that GetDictionary allows for names to be case sensitive")]
		public void TestGetDictionaryCaseSensitiveNames()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var a = db.GetDictionary<string, object>("A");
				var b = db.GetDictionary<string, object>("a");
				a.Should().NotBe(b);

				a.Put("1", value: 42);
				b.Put("1", value: 50);

				a.Get("1").Should().Be(expected: 42);
				b.Get("1").Should().Be(expected: 50);
			}
		}

		[Test]
		public void TestGetDictionaryDifferentTypes()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var a = db.GetDictionary<string, string>("Names");
				new Action(() => db.GetDictionary<string, int>("Names"))
					.Should().Throw<ArgumentException>()
					.WithMessage("The dictionary 'Names' has a value type of 'System.String': If your intent was to create a new dictionary, you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetNone()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().BeNull();
				db.GetDictionary<string, object>("SomeTable").GetMany("foo", "bar").Should().BeEmpty();
			}
		}

		[Test]
		public void TestOpenOrCreate1()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutGetCustomType()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var value1 = new Person {Name = "Strelok"};
				var value2 = new Person {Name = "The marked one"};
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt16()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var value1 = short.MinValue;
				var value2 = short.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt32()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var value1 = int.MinValue;
				var value2 = int.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt64()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var value1 = long.MinValue;
				var value2 = long.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetString()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var value1 = "Strelok";
				var value2 = "The marked one";
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutMany1()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var address = new Address
				{
					Country = "Germany",
					City = "Berlin",
					Street = "Any",
					Number = 42
				};
				var steven = new Person
				{
					Id = 42,
					Name = "Steven"
				};

				var table = db.GetDictionary<string, object>("Stuff");
				table.PutMany(new[]
				{
					new KeyValuePair<string, object>("foo", address),
					new KeyValuePair<string, object>("bar", steven)
				});


				table.Count().Should().Be(expected: 2);
				table.Get("bar").Should().Be(steven);
				table.Get("foo").Should().Be(address);
			}
		}

		[Test]
		[Description("Verifies that data from different tables doesn't interact with each other")]
		public void TestPutMultipleTables()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var customers = db.GetDictionary<string, object>("Customers");
				customers.Put("1", "Simon");

				var people = db.GetDictionary<string, object>("People");
				people.Put("1", "Kitty");

				customers.Get("1").Should().Be("Simon");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		public void TestPutNullKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Put(key: null, value: 42)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		[Description("Verifies that data removed from one table doesn't interact with others")]
		public void TestRemoveMultipleTables()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var customers = db.GetDictionary<string, object>("Customers");
				customers.Put("1", "Simon");

				var people = db.GetDictionary<string, object>("People");
				people.Put("1", "Kitty");

				customers.Remove("1");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		public void TestRemoveNonExistingKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Get("42").Should().BeNull();

				store.Put("42", value: null);
				store.Get("42").Should().BeNull();
			}
		}

		[Test]
		public void TestRemoveNullKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Remove(key: null)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(expected: 42);

				store.Put("foo", value: null);
				store.Get("foo").Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that ONLY the value with the specified key is removed and none other")]
		public void TestRemoveValue2()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("a", value: 1);
				store.Put("b", value: 2);
				store.Remove("a");

				store.Get("a").Should().BeNull();
				store.Get("b").Should().Be(expected: 2);
			}
		}

		[Test]
		public void TestReplaceValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(expected: 42);

				store.Put("foo", value: 50);
				store.Get("foo").Should().Be(expected: 50);
			}
		}
	}
}