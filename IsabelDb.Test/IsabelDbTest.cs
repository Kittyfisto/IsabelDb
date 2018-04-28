using System;
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