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
		public void TestAddRemovedCollcection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<int>("Stuff");
				db.Remove(hashSet);

				new Action(() => hashSet.Add(42))
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Stuff\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		public void TestAddManyRemovedCollcection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<int>("Stuff");
				db.Remove(hashSet);

				new Action(() => hashSet.AddMany(new []{42}))
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Stuff\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		public void TestRemoveRemovedCollcection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<int>("Stuff");
				db.Remove(hashSet);

				new Action(() => hashSet.Remove(42))
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Stuff\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		public void TestContainsRemovedCollcection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<int>("Stuff");
				db.Remove(hashSet);

				new Action(() => hashSet.Contains(42))
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Stuff\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		public void TestAddRemoveAdd()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<int>("Stuff");
				hashSet.Add(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().Equal(int.MinValue);

				hashSet.Remove(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().BeEmpty();

				hashSet.Add(int.MinValue).Should().BeTrue();
				hashSet.Count().Should().Be(1);
				hashSet.GetAllValues().Should().Equal(int.MinValue);
			}
		}

		[Test]
		public void TestAddDifferentTypes()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(ProcessorArchitecture)))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				hashSet.Add(42);
				hashSet.Add("Erased");
				hashSet.Add(new Version(8, 9, 42, 2));

				hashSet.Count().Should().Be(3);
                hashSet.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					42, "Erased", new Version(8, 9, 42, 2)
				});
			}
		}

		[Test]
		public void TestToString()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				db.GetOrCreateHashSet<string>("Stuff").ToString().Should().Be("HashSet<System.String>(\"Stuff\")");
			}
		}

		[Test]
		public void TestAddNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				new Action(() => hashSet.Add(null)).Should().Throw<ArgumentNullException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().Equal();
			}
		}

		[Test]
		public void TestAddManyNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				new Action(() => hashSet.AddMany(null)).Should().Throw<ArgumentNullException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().Equal();
			}
		}

		[Test]
		public void TestAddManySomeNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				new Action(() => hashSet.AddMany(new object[]{1, null, 2})).Should().Throw<ArgumentException>();
				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().Equal();
			}
		}

		[Test]
		public void TestContainsNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				new Action(() => hashSet.Contains(null)).Should().Throw<ArgumentNullException>();

				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().Equal();
			}
		}

		[Test]
		public void TestRemoveNull()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<object>("Stuff");
				new Action(() => hashSet.Remove(null)).Should().Throw<ArgumentNullException>();

				hashSet.Count().Should().Be(0);
				hashSet.GetAllValues().Should().Equal();
			}
		}

		[Test]
		public void TestAddSameValueTwice()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<string>("Stuff");
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
				var hashSet = db.GetOrCreateHashSet<string>("Stuff");
				hashSet.Remove("Stuff").Should().BeFalse();
			}
		}

		[Test]
		public void TestRemoveNonExistentValue()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var hashSet = db.GetOrCreateHashSet<string>("Stuff");
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

		protected override IHashSet<string> CreateCollection(IDatabase db, string name)
		{
			return db.CreateHashSet<string>(name);
		}

		protected override IHashSet<string> GetOrCreateCollection(IDatabase db, string name)
		{
			return db.GetOrCreateHashSet<string>(name);
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
