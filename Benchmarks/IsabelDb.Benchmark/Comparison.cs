using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using NUnit.Framework;

namespace IsabelDb.Benchmark
{
	[TestFixture]
	public sealed class Comparison
	{
		[SetUp]
		public void Setup()
		{
			var databasePath = Path.Combine(Path.GetTempPath(), "IsabelDb", "Benchmarks");
			Directory.CreateDirectory(databasePath);

			var filePath = CreateDatabaseName(databasePath, "isdb");
			if (File.Exists(filePath))
				File.Delete(filePath);
			_isabel = new TestBench<Database>(Database.OpenOrCreate(filePath, new []{typeof(Customer)}),
			                                    filePath);

			var liteDatabase = CreateDatabaseName(databasePath, "ldb");
			if (File.Exists(liteDatabase))
				File.Delete(liteDatabase);
			_lite = new TestBench<LiteDatabase>(new LiteDatabase(liteDatabase),
			                                      liteDatabase);
		}

		public static IEnumerable<int> Sizes => new[]
		{
			1,
			10,
			100,
			1000,
			10000,
			100000,
			1000000
		};

		private TestBench<Database> _isabel;
		private TestBench<LiteDatabase> _lite;

		private static string CreateDatabaseName(string path, string extension)
		{
			var fname = string.Format("{0}.{1}",
			                          TestContext.CurrentContext.Test.Name,
			                          extension);
			var databaseName = Path.Combine(path, fname);
			return databaseName;
		}

		private List<Customer> CreateCustomers(int count)
		{
			var ret = new List<Customer>(count);
			for (var i = 0; i < count; ++i) ret.Add(new Customer {Id = i + 1});

			return ret;
		}

		[Test]
		public void TestBulkInsertObjects([ValueSource(nameof(Sizes))] int testSize)
		{
			var customers = CreateCustomers(testSize);

			const string collectionName = "Foo";
			var testCase = string.Format("Insert {0} values (without warmup)", testSize);

			var collection1 = _isabel.Db.GetBag<Customer>(collectionName);
			_isabel.Measure(testCase, db =>
			{
				collection1.PutMany(customers);
			});

			var collection2 = _lite.Db.GetCollection<Customer>(collectionName);
			_lite.Measure(testCase, db =>
			{
				collection2.InsertBulk(customers);
			});
		}

		[Test]
		public void TestBulkReadObjects([ValueSource(nameof(Sizes))] int testSize)
		{
			var customers = CreateCustomers(testSize);

			const string collectionName = "Foo";
			var testCase = string.Format("Read {0} values (without warmup)", testSize);

			var collection1 = _isabel.Db.GetBag<Customer>(collectionName);
			collection1.PutMany(customers);

			var collection2 = _lite.Db.GetCollection<Customer>(collectionName);
			collection2.InsertBulk(customers);

			_isabel.Measure(testCase, db =>
			{
				collection1.GetAll().ToList();
			});
			_lite.Measure(testCase, db =>
			{
				collection2.FindAll().ToList();
			});
		}
	}
}