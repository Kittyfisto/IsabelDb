using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	/// <summary>
	///     This class shall contain those tests which require the database to be stored on disk
	///     (as to in-memory). These tests are mostly concerned with reopening an existing database
	///     and then verifying that everything works as expected.
	/// </summary>
	[TestFixture]
	public sealed class DatabaseFileTests
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

		private static IEnumerable<Type> NoCustomTypes => new Type[0];

		private string _databaseName;
		
		[Test]
		public void TestToStringFile()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.ToString().Should().Be(string.Format("IsabelDb: File '{0}' (0 collection(s))", _databaseName));
			}
		}

		[Test]
		public void TestCompactEmpty()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.Compact();
			}
		}

		[Test]
		public void TestCompactAfterDeletion()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var bag = db.GetOrCreateBag<object>("Stuff");
				bag.PutMany(Enumerable.Range(0, 10000).Cast<object>());

				var fileSize = new FileInfo(_databaseName).Length;
				fileSize.Should().BeGreaterThan(180000);

				db.Remove(bag);
				new FileInfo(_databaseName).Length.Should().BeGreaterThan(180000);

				db.Compact();
				new FileInfo(_databaseName).Length.Should().BeLessThan(40000);
			}
		}

		[Test]
		public void TestGetDictionaryAfterReopen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.GetOrCreateDictionary<string, object>("A").Put("Meaning", value: 9001);
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.GetOrCreateDictionary<string, object>("a").Put("Meaning", value: 42);

				db.GetOrCreateDictionary<string, object>("A").Get("Meaning").Should().Be(9001);
				db.GetOrCreateDictionary<string, object>("a").Get("Meaning").Should().Be(42);
			}
		}

		[Test]
		public void TestOpen1()
		{
			new Action(() => Database.Open(_databaseName, NoCustomTypes))
				.Should()
				.Throw<FileNotFoundException>();
		}

		[Test]
		public void TestOpenOrCreate2()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.GetOrCreateDictionary<string, object>("SomeTable").Put("a", "b");
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				db.GetOrCreateDictionary<string, object>("SomeTable").Get("a").Should().Be("b");
			}
		}

		[Test]
		public void TestOverwriteReopen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateDictionary<string, object>("Charts");
				charts.Put("Bar", value: 2);
				charts.Get("Bar").Should().Be(2);
				charts.Put("Bar", value: 2.2);
				charts.Get("Bar").Should().Be(2.2);
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateDictionary<string, object>("Charts");
				charts.Get("Bar").Should().Be(2.2);
			}
		}

		[Test]
		public void TestPutCloseOpenGet()
		{
			var strelok = new Person {Name = "Strelok"};
			var markedOne = new Person {Name = "The marked one"};

			using (var db = Database.OpenOrCreate(_databaseName, new[] {typeof(Person), typeof(Address)}))
			{
				db.GetOrCreateDictionary<string, object>("SomeTable").Put("foo", strelok);
				db.GetOrCreateDictionary<string, object>("SomeTable").Put("bar", markedOne);
				db.GetOrCreateDictionary<string, object>("SomeTable").Put("home", new Address
				{
					Country = "A",
					City = "B",
					Street = "C",
					Number = 4
				});
			}

			using (var db = Database.Open(_databaseName, new[] {typeof(Person), typeof(Address)}))
			{
				var persons = db.GetOrCreateDictionary<string, object>("SomeTable")
				                .GetMany(new string[]{"foo", "bar"});

				persons.Should().HaveCount(2);
				var person = persons.ElementAt(index: 0);
				person.Key.Should().Be("foo");
				person.Value.Should().Be(strelok);

				person = persons.ElementAt(index: 1);
				person.Key.Should().Be("bar");
				person.Value.Should().Be(markedOne);
			}
		}

		[Test]
		public void TestPutMany()
		{
			using (var db = Database.OpenOrCreate(_databaseName, new[] {typeof(Person)}))
			{
				const int count = 100000;
				var values = new List<KeyValuePair<string, Person>>();
				var table = db.GetOrCreateDictionary<string, Person>("Piggies");
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

				var n = 0;
				foreach (var pair in actualPersons)
				{
					pair.Key.Should().Be(values[n].Key);
					pair.Value.Should().Be(values[n].Value);
					++n;
				}
			}
		}

		[Test]
		[Description("Verifies that when data is removed, it remains removed in the next session")]
		public void TestRemoveFromDictionaryAfterReopen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateDictionary<string, object>("Charts");
				charts.Put("Pie", value: 1.5);
				charts.Get("Pie").Should().Be(1.5);
				charts.Remove("Pie");
				charts.TryGet("Pie", out var unused).Should().BeFalse();
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateDictionary<string, object>("Charts");
				charts.TryGet("Pie", out var unused).Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that when data is removed, it remains removed in the next session")]
		public void TestRemoveFromMultiValueDictionaryAfterReopen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateMultiValueDictionary<string, object>("Charts");
				charts.Put("Pie", "Hello!");
				charts.GetValues("Pie").Should().Equal("Hello!");
				charts.RemoveAll("Pie");
				charts.GetValues("Pie").Should().BeEmpty();
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateMultiValueDictionary<string, object>("Charts");
				charts.GetValues("Pie").Should().BeEmpty();
			}
		}

		[Test]
		[Description("Verifies that when data is removed, it remains removed in the next session")]
		public void TestClearBagAfterReopen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateBag<object>("Charts");
				charts.Put("Hello!");
				charts.GetAllValues().Should().Equal("Hello!");
				charts.Clear();
				charts.GetAllValues().Should().BeEmpty();
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var charts = db.GetOrCreateBag<object>("Charts");
				charts.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutAfterReOpen()
		{
			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var values = db.GetOrCreateMultiValueDictionary<int, string>("Values");
				values.Put(1, "a");
				values.Put(1, "b");
			}

			using (var db = Database.OpenOrCreate(_databaseName, NoCustomTypes))
			{
				var values = db.GetOrCreateMultiValueDictionary<int, string>("Values");
				values.Put(1, "c");
				values.GetValues(1).Should().Equal("a", "b", "c");
			}
		}

		[Test]
		[Description("OpenRead shall throw an appropriate exception if the file doesn't exist")]
		public void TestOpenReadOnly1()
		{
			new Action(() => Database.OpenRead("doesn't exist", NoCustomTypes))
				.Should().Throw<FileNotFoundException>();
		}

		[Test]
		[Description("OpenRead shall be able to read from a readonly file")]
		public void TestOpenReadOnly2()
		{
			const string filename = "ReadonlyDatabase.isdb";
			if (File.Exists(filename))
			{
				File.SetAttributes(filename, FileAttributes.Normal);
				File.Delete(filename);
			}

			using (var db = Database.OpenOrCreate(filename, NoCustomTypes))
			{
				var collection = db.GetOrCreateBag<string>("Values");
				collection.PutMany(new []{"a", "b", "c"});
			}

			File.SetAttributes(filename, FileAttributes.ReadOnly);

			using (var db = Database.OpenRead(filename, NoCustomTypes))
			{
				var collection = db.GetBag<string>("Values");
				collection.GetAllValues().Should().Equal("a", "b", "c");
			}
		}
	}
}