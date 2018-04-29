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
		[Test]
		public void TestCreateInMemory()
		{
			IsabelDb.CreateInMemory();
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
		[Description("Verifies that GetDictionary allows for names to be case sensitive")]
		public void TestGetDictionaryCaseSensitiveNames()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var a = db.GetDictionary<string, object>("A");
				var b = db.GetDictionary<string, object>("a");
				a.Should().NotBe(b);

				a.Put("1", 42);
				b.Put("1", 50);

				a.Get("1").Should().Be(42);
				b.Get("1").Should().Be(50);
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
		public void TestGet1()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", "bar");
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().Be("bar");
			}
		}

		[Test]
		public void TestReplaceValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", 50);
				store.Get("foo").Should().Be(50);
			}
		}

		[Test]
		public void TestRemoveNonExistingKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Get("42").Should().BeNull();

				store.Put("42", null);
				store.Get("42").Should().BeNull();
			}
		}

		[Test]
		public void TestRemoveNullKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Remove(null)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", null);
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
				store.Put("a", 1);
				store.Put("b", 2);
				store.Remove("a");

				store.Get("a").Should().BeNull();
				store.Get("b").Should().Be(2);
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
		public void TestPutNullKey()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Put(null, 42)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void TestPutGetInt16()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				short value1 = short.MinValue;
				short value2 = short.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt32()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				int value1 = int.MinValue;
				int value2 = int.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt64()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				long value1 = long.MinValue;
				long value2 = long.MaxValue;
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
				table.PutMany(new []
				{
					new KeyValuePair<string, object>("foo", address),
					new KeyValuePair<string, object>("bar", steven)
				});


				table.Count().Should().Be(2);
				table.Get("bar").Should().Be(steven);
				table.Get("foo").Should().Be(address);
			}
		}

		[Test]
		public void TestPutMany2()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				const int count = 100000;
				var persons = new List<KeyValuePair<string, Person>>();
				var table = db.GetDictionary<string, Person>("Piggies");
				for (int i = 0; i < count; ++i)
				{
					var person = new Person
					{
						Id = i,
						Name = string.Format("Guinea Pig {0}", i)
					};
					persons.Add(new KeyValuePair<string, Person>(person.Id.ToString(), person));
				}

				var stopwatch = Stopwatch.StartNew();

				table.PutMany(persons);

				stopwatch.Stop();
				Console.Write("Writing {0} objects took {1}ms", count, stopwatch.ElapsedMilliseconds);


				table.Count().Should().Be(count);
				for (int i = 0; i < count; ++i)
				{
					var actualPerson = table.Get(i.ToString());
					actualPerson.Should().Be(persons[i].Value);
				}
			}
		}

		[Test]
		[Description("Verifies that clearing an empty dictionary is allowed")]
		public void TestClearEmpty()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				var people = db.GetDictionary<string, object>("People");
				people.Count().Should().Be(0);
				people.Clear();
				people.Count().Should().Be(0);
			}
		}

		private static void PutAndGet<T>(IsabelDb db, T value1, T value2)
		{
			var store = db.GetDictionary<string, object>("SomeTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(2);
			var actualValue1 = persons.ElementAt(0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}
	}
}