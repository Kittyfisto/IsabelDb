using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.HashSet
{
	[TestFixture]
	public sealed class HashSetTest
		: AbstractCollectionTest<IHashSet<string>>
	{
		private string _last;

		[Test]
		public void TestAddRemoveAdd()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<int>("Stuff");
				hashSet.Add(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().BeEquivalentTo(int.MinValue);

				hashSet.Remove(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEmpty();

				hashSet.Add(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().BeEquivalentTo(int.MinValue);
			}
		}

		[Test]
		public void TestAddDifferentTypes()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(ProcessorArchitecture)))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				hashSet.Add(42);
				hashSet.Add("Erased");
				hashSet.Add(new Version(8, 9, 42, 2));
				hashSet.Add(ProcessorArchitecture.Amd64);

				hashSet.Count().Should().Be(4);
				hashSet.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					42, "Erased", new Version(8, 9, 42, 2),
					ProcessorArchitecture.Amd64
				});
			}
		}

		[Test]
		public void TestToString()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				db.GetHashSet<string>("Stuff").ToString().Should().Be("HashSet<System.String>(\"Stuff\")");
			}
		}

		[Test]
		public void TestAddNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				new Action(() => hashSet.Add(null)).Should().Throw<ArgumentNullException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEquivalentTo();
			}
		}

		[Test]
		public void TestAddManyNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				new Action(() => hashSet.AddMany(null)).Should().Throw<ArgumentNullException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEquivalentTo();
			}
		}

		[Test]
		public void TestAddManySomeNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				new Action(() => hashSet.AddMany(new object[]{1, null, 2})).Should().Throw<ArgumentException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEquivalentTo();
			}
		}

		[Test]
		public void TestContainsNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				new Action(() => hashSet.Contains(null)).Should().Throw<ArgumentNullException>();

				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEquivalentTo();
			}
		}

		[Test]
		public void TestRemoveNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<object>("Stuff");
				new Action(() => hashSet.Remove(null)).Should().Throw<ArgumentNullException>();

				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEquivalentTo();
			}
		}

		[Test]
		public void TestAddSameValueTwice()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<string>("Stuff");
				hashSet.Add("Hello, World!");
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().BeEquivalentTo(new object[]{"Hello, World!"});

				hashSet.Add("Hello, World!");
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().BeEquivalentTo(new object[]{"Hello, World!"});
			}
		}

		[Test]
		public void TestRemoveWhileEmpty()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<string>("Stuff");
				hashSet.Remove("Stuff").Should().BeFalse();
			}
		}

		[Test]
		public void TestRemoveNonExistentValue()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetHashSet<string>("Stuff");
				hashSet.Add("Add");

				hashSet.Remove("Stuff").Should().BeFalse();
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().BeEquivalentTo(new object[] {"Add"});
			}
		}

		#region Overrides of AbstractCollectionTest<IHashSet<string>>

		protected override CollectionType CollectionType => CollectionType.HashSet;

		protected override IHashSet<string> GetCollection(IDatabase db, string name)
		{
			return db.GetHashSet<string>(name);
		}

		protected override void Put(IHashSet<string> collection, string value)
		{
			collection.Add(value);
			_last = value;
		}

		protected override void PutMany(IHashSet<string> collection, params string[] values)
		{
			collection.AddMany(values);
			_last = values.LastOrDefault();
		}

		protected override void RemoveLastPutValue(IHashSet<string> collection)
		{
			collection.Remove(_last);
		}

		#endregion
	}
}
