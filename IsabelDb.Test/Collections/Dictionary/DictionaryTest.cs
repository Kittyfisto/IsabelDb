using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class DictionaryTest
		: AbstractCollectionTest<IDictionary<int, string>>
	{
		private int _lastKey;

		[SetUp]
		public void SetUp()
		{
			_lastKey = 0;
		}

		[Test]
		public void TestGet1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", "bar");
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().Be("bar");
				db.GetDictionary<string, object>("SomeTable").TryGet("foo", out var value).Should().BeTrue();
				value.Should().Be("bar");
			}
		}

		[Test]
		public void TestGetManyValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.Put(3, "C");
				collection.Put(4, "D");
				collection.GetManyValues(new[] {2, 3}).Should().Equal("B", "C");
			}
		}

		[Test]
		public void TestGetManyValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.GetManyValues(new[] {0, 1}).Should().Equal("A");
				collection.GetManyValues(new[] {2, 3}).Should().Equal("B");
			}
		}

		[Test]
		public void TestGetManyValues3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<int, string>("Stuff");
				collection.Put(1, "A");
				collection.Put(2, "B");
				collection.GetManyValues(new[] {0, 3, 5}).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetDictionaryDifferentTypes()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var a = db.GetDictionary<string, string>("Names");
				new Action(() => db.GetDictionary<string, int>("Names"))
					.Should().Throw<ArgumentException>()
					.WithMessage("The dictionary 'Names' has a value type of 'System.String': If your intent was to create a new dictionary, you have to pick a new name!");
			}
		}

		[Test]
		public void TestGetNone()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").TryGet("foo", out var unused).Should().BeFalse();
				db.GetDictionary<string, object>("SomeTable").GetMany("foo", "bar").Should().BeEmpty();
			}
		}

		protected override IDictionary<int, string> GetCollection(Database db, string name)
		{
			return db.GetDictionary<int, string>(name);
		}

		protected override void Put(IDictionary<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _lastKey), value);
		}

		protected override void PutMany(IDictionary<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, string>>(values.Length);
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, string>(Interlocked.Increment(ref _lastKey), value));
			}
			collection.PutMany(pairs);
		}
	}
}
