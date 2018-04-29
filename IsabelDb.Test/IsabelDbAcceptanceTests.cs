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
	public sealed class IsabelDbAcceptanceTests
	{
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

			Console.WriteLine("Filename: {0}", _databaseName);
		}

		private string _databaseName;

		[Test]
		public void TestGetDictionaryAfterReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("A").Put("Meaning", value: 9001);
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				db.GetDictionary<string, object>("a").Put("Meaning", value: 42);

				db.GetDictionary<string, object>("A").Get("Meaning").Should().Be(expected: 9001);
				db.GetDictionary<string, object>("a").Get("Meaning").Should().Be(expected: 42);
			}
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
		public void TestOverwriteReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Put("Bar", value: 2);
				charts.Get("Bar").Should().Be(expected: 2);
				charts.Put("Bar", value: 2.2);
				charts.Get("Bar").Should().Be(expected: 2.2);
			}

			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Get("Bar").Should().Be(expected: 2.2);
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
				var persons = db.GetDictionary<string, object>("SomeTable").GetMany("foo", "bar");
				persons.Should().HaveCount(expected: 2);
				var person = persons.ElementAt(index: 0);
				person.Key.Should().Be("foo");
				person.Value.Should().Be(strelok);

				person = persons.ElementAt(index: 1);
				person.Key.Should().Be("bar");
				person.Value.Should().Be(markedOne);
			}
		}

		[Test]
		[Description("Verifies that when data is removed, it remains removed in the next session")]
		public void TestRemoveAfterReopen()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				var charts = db.GetDictionary<string, object>("Charts");
				charts.Put("Pie", value: 1.5);
				charts.Get("Pie").Should().Be(expected: 1.5);
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
		public void TestPutMany()
		{
			using (var db = IsabelDb.OpenOrCreate(_databaseName))
			{
				const int count = 100000;
				var values = new List<KeyValuePair<string, Person>>();
				var table = db.GetDictionary<string, Person>("Piggies");
				for (var i = 0; i < count; ++i)
				{
					var person = new Person
					{
						Id = i,
						Name = string.Format("Guinea Pig {0}", i)
					};
					values.Add(new KeyValuePair<string, Person>(person.Id.ToString(), person));
				}

				var stopwatch = Stopwatch.StartNew();
				table.PutMany(values);
				stopwatch.Stop();
				Console.WriteLine("Writing {0} objects took {1}ms", count, stopwatch.ElapsedMilliseconds);


				table.Count().Should().Be(count);

				stopwatch.Restart();
				var actualPersons = table.GetAll();
				stopwatch.Stop();
				Console.WriteLine("Reading {0} objects took {1}ms", count, stopwatch.ElapsedMilliseconds);

				int n = 0;
				foreach (var pair in actualPersons)
				{
					pair.Key.Should().Be(values[n].Key);
					pair.Value.Should().Be(values[n].Value);
					++n;
				}
			}
		}
	}
}