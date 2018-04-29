﻿using System;
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
				db.GetDictionary<string, object>("SomeTable").Put("a", "b");
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("SomeTable").Get("a").Should().Be("b");
			}
		}

		[Test]
		public void TestGetDictionaryAfterReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("A").Put("Meaning", 9001);
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("a").Put("Meaning", 42);

				db.GetDictionary<string, object>("A").Get("Meaning").Should().Be(9001);
				db.GetDictionary<string, object>("a").Get("Meaning").Should().Be(42);
			}
		}

		[Test]
		public void TestOverwriteReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Put("Bar", 2);
				charts.Get("Bar").Should().Be(2);
				charts.Put("Bar", 2.2);
				charts.Get("Bar").Should().Be(2.2);
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Get("Bar").Should().Be(2.2);
			}
		}

		[Test]
		[Description("Verifies that when data is removed, it remains removed in the next session")]
		public void TestRemoveAfterReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Put("Pie", 1.5);
				charts.Get("Pie").Should().Be(1.5);
				charts.Remove("Pie");
				charts.Get("Pie").Should().BeNull();
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Get("Pie").Should().BeNull();
			}
		}

		[Test]
		public void TestPutCloseOpenGet()
		{
			var strelok = new Person {Name = "Strelok"};
			var markedOne = new Person {Name = "The marked one"};

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", strelok);
				db.GetDictionary<string, object>("SomeTable").Put("bar", markedOne);
				db.GetDictionary<string, object>("SomeTable").Put("home", new Address
				{
					Country = "A",
					City = "B",
					Street = "C",
					Number = 4
				});
			}

			using (var db = IsabelDb.Open(_databaseName))
			{
				var persons = db.GetDictionary<string, object>("SomeTable").Get("foo", "bar");
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