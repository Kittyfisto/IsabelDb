using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using NUnit.Framework;

namespace IsabelDb.Benchmark
{
	public class Foo
	{
		public Foo()
		{
			//throw new NullReferenceException();
		}

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
