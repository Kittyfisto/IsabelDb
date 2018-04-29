using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class IsabelDbAcceptanceTests
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
		public void TestOpenOrCreate2()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary("SomeTable").Put("a", "b");
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary("SomeTable").Get("a").Should().Be("b");
			}
		}

		[Test]
		public void TestGet1()
		{
			using (var db = IsabelDb.CreateInMemory())
			{
				db.GetDictionary("SomeTable").Put("foo", "bar");
				db.GetDictionary("SomeTable").Get("foo").Should().Be("bar");
			}
		}

		[Test]
		public void TestPutCloseOpenGet()
		{
			var strelok = new Person {Name = "Strelok"};
			var markedOne = new Person {Name = "The marked one"};

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary("SomeTable").Put("foo", strelok);
				db.GetDictionary("SomeTable").Put("bar", markedOne);
				db.GetDictionary("SomeTable").Put("home", new Address
				{
					Country = "A",
					City = "B",
					Street = "C",
					Number = 4
				});
			}

			using (var db = IsabelDb.Open(_databaseName))
			{
				var persons = db.GetDictionary("SomeTable").Get("foo", "bar");
				persons.Should().HaveCount(2);
				var person = persons.ElementAt(0);
				person.Key.Should().Be("foo");
				person.Value.Should().Be(strelok);

				person = persons.ElementAt(1);
				person.Key.Should().Be("bar");
				person.Value.Should().Be(markedOne);
			}
		}
	}
}