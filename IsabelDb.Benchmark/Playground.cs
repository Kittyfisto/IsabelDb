using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiteDB;
using NUnit.Framework;
using ProtoBuf.Meta;

namespace IsabelDb.Benchmark
{
	public class Foo
	{
		public string Stuff { get; set; }

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("Stuff: {0}", Stuff);
		}

		#endregion
	}

	[TestFixture]
	public sealed class Playground
	{
		[Test]
		public void TestWrite()
		{
			var sw = Stopwatch.StartNew();
			File.WriteAllText(@"G:\foo.txt", "Hello, World!");
			sw.Stop();
			Console.WriteLine("Write: {0}ms", sw.ElapsedMilliseconds);
		}

		[Test]
		public void Foo()
		{
			var fname = "foo.isdb";
			if (File.Exists(fname))
				File.Delete(fname);

			using (var db = IsabelDb.OpenOrCreate(fname, new Type[0]))
			{
				var stuff = db.GetDictionary<int, int>("stuff");
				var sw = Stopwatch.StartNew();
				const int count = 100;
				for (int i = 0; i < count; ++i)
				{
					stuff.Put(i, 2);
				}
				sw.Stop();
				Console.WriteLine("Avg: {0}ms", sw.ElapsedMilliseconds / count);
			}
		}

		[Test]
		public void Test()
		{
			const string filename = "lite.db";
			if (File.Exists(filename))
				File.Delete(filename);

			using (var db = new LiteDatabase(filename))
			{
				var values = db.GetCollection<Customer>("Stuff");
				values.Insert(new Customer {Name = "Simon"});
			}

			using (var db = new LiteDatabase(filename))
			{
				var values = db.GetCollection<Customer>("Stuff");
				var customers = values.FindAll().ToList();
				Write(customers);
			}

			using (var db = new LiteDatabase(filename))
			{
				var values = db.GetCollection<Foo>("Stuff");
				var customers = values.FindAll().ToList();
				Write(customers);
			}
		}

		private static void Write<T>(List<T> values)
		{
			Console.WriteLine("# Customers: {0}", values.Count);
			foreach (var value in values)
			{
				Console.WriteLine(value);
			}
		}
	}
}
