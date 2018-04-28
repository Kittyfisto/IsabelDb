using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class IsabelDbTest
	{
		private string _databaseName;

		[SetUp]
		public void Setup()
		{
			var fname = string.Format("{0}.isdb",
			                          TestContext.CurrentContext.Test.Name);
			_databaseName = Path.Combine(Path.GetTempPath(), "IsabelDb", "Tests", fname);
			var dir = Path.GetDirectoryName(_databaseName);
			Directory.CreateDirectory(dir);
			if (File.Exists(_databaseName))
				File.Delete(_databaseName);
		}

		[Test]
		public void TestOpen1()
		{
			new Action(() => IsabelDb.Open(_databaseName))
				.Should()
				.Throw<FileNotFoundException>();
		}

		[Test]
		public void TestOpenOrCreate1()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var store = db["SomeTable"];
				store.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestOpenOrCreate2()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db["SomeTable"].Put("a", "b");
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db["SomeTable"].Get("a").Should().Be("b");
			}
		}

		[Test]
		[Description("Verifies that data from different tables doesn't interact with each other")]
		public void TestPutMultipleTables()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var customers = db["Customers"];
				customers.Put("1", "Simon");

				var people = db["People"];
				people.Put("1", "Kitty");

				customers.Get("1").Should().Be("Simon");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		[Description("Verifies that data removed from one table doesn't interact with others")]
		public void TestRemoveMultipleTables()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var customers = db["Customers"];
				customers.Put("1", "Simon");

				var people = db["People"];
				people.Put("1", "Kitty");

				customers.Remove("1");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		public void TestReplaceValue()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var store = db["SomeTable"];
				store.Put("foo", 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", 50);
				store.Get("foo").Should().Be(50);
			}
		}

		[Test]
		public void TestRemoveNonExistingKey()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var store = db["SomeTable"];
				store.Get("42").Should().BeNull();

				store.Put("42", null);
				store.Get("42").Should().BeNull();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var store = db["SomeTable"];
				store.Put("foo", 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", null);
				store.Get("foo").Should().BeNull();
			}
		}

		[Test]
		public void TestGet1()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db["SomeTable"].Put("foo", "bar");
				db["SomeTable"].Get("foo").Should().Be("bar");
			}
		}

		[Test]
		public void TestGetNone()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db["SomeTable"].Get("foo").Should().BeNull();
				db["SomeTable"].Get("foo", "bar").Should().BeEmpty();
				db["SomeTable"].Get<int>("foo", "bar").Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutGetInt16()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				short value1 = short.MinValue;
				short value2 = short.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt32()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				int value1 = int.MinValue;
				int value2 = int.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt64()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				long value1 = long.MinValue;
				long value2 = long.MaxValue;
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetString()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var value1 = "Strelok";
				var value2 = "The marked one";
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetCustomType()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var value1 = new Person {Name = "Strelok"};
				var value2 = new Person {Name = "The marked one"};
				PutAndGet(db, value1, value2);
			}
		}

		[Test]
		public void TestPutMany1()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
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

				var table = db["Stuff"];
				table.Put(new []
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
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				const int count = 10000;
				var persons = new List<KeyValuePair<string, Person>>();
				var table = db["wadwdw"];
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

				table.Put(persons);

				stopwatch.Stop();
				Console.Write("Writing {0} objects took {1}ms", count, stopwatch.ElapsedMilliseconds);


				table.Count().Should().Be(count);
				for (int i = 0; i < count; ++i)
				{
					var actualPerson = table.Get<Person>(i.ToString());
					actualPerson.Should().Be(persons[i].Value);
				}
			}
		}

		[Test]
		public void TestPutCloseOpenGet()
		{
			var strelok = new Person {Name = "Strelok"};
			var markedOne = new Person {Name = "The marked one"};

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db["SomeTable"].Put("foo", strelok);
				db["SomeTable"].Put("bar", markedOne);
				db["SomeTable"].Put("home", new Address
				{
					Country = "A",
					City = "B",
					Street = "C",
					Number = 4
				});
			}

			using (var db = IsabelDb.Open(_databaseName))
			{
				var persons = db["SomeTable"].Get("foo", "bar");
				persons.Should().HaveCount(2);
				var person = persons.ElementAt(0);
				person.Key.Should().Be("foo");
				person.Value.Should().Be(strelok);

				person = persons.ElementAt(1);
				person.Key.Should().Be("bar");
				person.Value.Should().Be(markedOne);
			}
		}

		private static void PutAndGet<T>(IsabelDb db, T value1, T value2)
		{
			var store = db["SomeTable"];

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.Get("foo", "bar");
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