using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections
{
	public abstract class AbstractCollectionTest<TCollection>
		: AbstractTest
		where TCollection : ICollection<string>
	{
		protected static readonly Type[] NoCustomTypes = new Type[0];

		#region Create collection

		[Test]
		public void TestCreateNewCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				collection.Should().NotBeNull();
				db.Collections.Should().BeEquivalentTo(new object[] {collection});

				GetCollection(db, "Values").Should().BeSameAs(collection);
				GetOrCreateCollection(db, "Values").Should().BeSameAs(collection);
			}
		}

		[Test]
		public void TestGetOrCreateCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetOrCreateCollection(db, "stuff");
				collection.Should().NotBeNull();
				db.Collections.Should().BeEquivalentTo(new object[] {collection});

				GetCollection(db, "stuff").Should().BeSameAs(collection);
				GetOrCreateCollection(db, "stuff").Should().BeSameAs(collection);
			}
		}

		[Test]
		public void TestGetCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => GetCollection(db, "Values")).Should().Throw<NoSuchCollectionException>();
				db.Collections.Should().BeEmpty();
			}
		}

		[Test]
		public void TestCreateExistingCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				new Action(() => CreateCollection(db, "Values")).Should().Throw<CollectionNameAlreadyInUseException>();
				db.Collections.Should().BeEquivalentTo(new object[] {collection},
				                                       "because only one collection may have been created");
			}
		}

		[Test]
		public void TestGetSameCollectionTwice()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection1 = GetOrCreateCollection(db, "Values1");
				var collection2 = GetOrCreateCollection(db, "Values1");
				var collection3 = GetOrCreateCollection(db, "Values2");

				collection1.Should().BeSameAs(collection2);
				collection1.Should().NotBeSameAs(collection3);
			}
		}

		#endregion

		#region Transactions

		[Test]
		public void TestRollbackCollectionCreate()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				using (var transaction = db.BeginTransaction())
				{
					var collection = db.CreateBag<string>("Stuff");
					db.Collections.Should().BeEquivalentTo(new[]{collection});

					transaction.Rollback();
				}

				db.Collections.Should().BeEmpty("because we've rolled back the transaction and thus the collection may not have been created after all");
			}
		}

		[Test]
		public void TestRollbackCollectionCreateAndCreateNewAfterwards()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				using (var transaction = db.BeginTransaction())
				{
					var collection = CreateCollection(db, "Phrases");
					Put(collection, "Peek-A-Boo");
					db.Collections.Should().BeEquivalentTo(new[]{collection});

					transaction.Rollback();
				}

				var otherCollection = GetOrCreateCollection(db, "Phrases");
				otherCollection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestRollbackCollectionRemoval()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "Peek-A-Boo");
				db.Collections.Should().BeEquivalentTo(new[]{collection});
				using (var transaction = db.BeginTransaction())
				{
					db.Remove(collection);
					db.Collections.Should().BeEmpty();

					transaction.Rollback();
				}

				db.Collections.Should().BeEquivalentTo(new object[]{collection}, "because the collection removal should've been rolled back");
				collection.GetAllValues().Should().BeEquivalentTo(new object[] {"Peek-A-Boo"},
				                                                  "because the content from the collection should not have been affected");
			}
		}

		[Test]
		public void TestRollbackCollectionCreationAndRemoval()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				using (var transaction = db.BeginTransaction())
				{
					var collection = CreateCollection(db, "Stuff");
					Put(collection, "Peek-A-Boo");
					db.Collections.Should().Equal(collection);
					collection.GetAllValues().Should().BeEquivalentTo(new object[] {"Peek-A-Boo"});

					db.Remove(collection);
					db.Collections.Should().BeEmpty();

					transaction.Rollback();
				}

				db.Collections.Should().BeEmpty();
				var otherCollection = GetOrCreateCollection(db, "Stuff");
				otherCollection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestCommitSeveralAddMany()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");

				using (var transaction = db.BeginTransaction())
				{
					PutMany(collection, "a", "b", "c");
					PutMany(collection, "d", "e", "f");

					transaction.Commit();
				}

				collection.GetAllValues().Should().BeEquivalentTo(new object[]
				{
					"a", "b", "c",
					"d", "e", "f"
				});
			}
		}

		[Test]
		public void TestRollbackSeveralAddMany()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");

				using (var transaction = db.BeginTransaction())
				{
					PutMany(collection, "a", "b", "c");
					PutMany(collection, "d", "e", "f");

					transaction.Rollback();
				}

				collection.GetAllValues().Should().BeEmpty();
			}
		}

		#endregion

		[Test]
		public void TestGetAllValuesAgain()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "SomeTable");
				PutMany(collection, "foo", "bar");

				collection = GetCollection(db, "SomeTable");
				collection.GetAllValues().Should().Equal("foo", "bar");
			}
		}

		[Test]
		[Description("Verifies that collection names are case sensitive")]
		public void TestGetDictionaryCaseSensitiveNames()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var a = CreateCollection(db, "A");
				var b = CreateCollection(db, "a");
				a.Should().NotBe(b);

				Put(a, "42");
				Put(b, "50");

				a.GetAllValues().Should().Equal("42");
				b.GetAllValues().Should().Equal("50");
			}
		}

		[Test]
		[Description("Verifies that clearing an empty collection is allowed")]
		public void TestClearEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearOneItem()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "Foo");
				collection.Count().Should().Be(1);
				collection.GetAllValues().Should().HaveCount(1);

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestClearManyItems()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "A");
				Put(collection, "B");
				Put(collection, "C");
				collection.Count().Should().Be(3);
				collection.GetAllValues().Should().HaveCount(3);

				collection.Clear();
				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestCountEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				collection.Count().Should().Be(0);
			}
		}

		[Test]
		public void TestGetAllValuesEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllValuesOneValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "Monty");
				collection.GetAllValues().Should().Equal("Monty");
			}
		}

		[Test]
		public void TestGetAllValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "Monty");
				Put(collection, "A");
				Put(collection, "B");
				collection.GetAllValues().Should().Equal("Monty", "A", "B");
			}
		}

		[Test]
		public void TestCountOne()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");
				Put(collection, "Monty Python");
			}
		}

		[Test]
		public void TestPut()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = CreateCollection(db, "Values");

				const int count = 1000;
				var values = new List<string>(count);
				for (int i = 0; i < count; ++i)
				{
					var value = "Stuff" + i;
					Put(collection, value);
					values.Add(value);
				}

				collection.GetAllValues().Should().Equal(values);
			}
		}

		[Test]
		public void TestGetCollections()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, false))
				{
					db.Collections.Should().BeEmpty();
					var collection = CreateCollection(db, "Stuff");
					db.Collections.Should().Equal(collection);
				}
			}
		}

		[Test]
		public void TestCollectionName()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, false))
				{
					var collection = CreateCollection(db, "For The Fallen Dreams");
					collection.Name.Should().Be("For The Fallen Dreams");
					collection.Type.Should().Be(CollectionType);
				}
			}
		}

		#region Session Tests

		[Test]
		[Description("Verifies that values stored in one session can be read from the collection in a different one")]
		public void TestPutGetDifferentSessions()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "Values");
					Put(collection, "Stuff");
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = GetCollection(db, "Values");
					collection.Count().Should().Be(1);
					collection.GetAllValues().Should().Equal("Stuff");
				}
			}
		}

		[Test]
		[Description("Verifies that new values can be appended to the same collection in a different session")]
		public void TestPutPutDifferentSessions()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "Values");
					Put(collection, "Peter");
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = GetCollection(db, "Values");
					Put(collection, "Parker");
					collection.Count().Should().Be(2);
					collection.GetAllValues().Should().Equal("Peter", "Parker");
				}
			}
		}

		[Test]
		[Description("Verifies that removed values are still gone, even if the database is reopened")]
		public void TestRemoveDifferentSessions()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "Values");
					Put(collection, "Peter");
					collection.GetAllValues().Should().Equal("Peter");

					RemoveLastPutValue(collection);
					collection.Count().Should().Be(0);
					collection.GetAllValues().Should().BeEmpty();
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = GetCollection(db, "Values");
					collection.Count().Should().Be(0);
					collection.GetAllValues().Should().BeEmpty();
				}
			}
		}

		[Test]
		public void TestGetNonExistantCollectionInReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
			{
				new Action(() => GetCollection((IDatabase) db, "Stuff"))
					.Should().Throw<NoSuchCollectionException>()
					.WithMessage("Unable to find a collection named 'Stuff'");
			}
		}

		[Test]
		public void TestPutReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: false))
				{
					var collection = CreateCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
				{
					var stuff = GetCollection(db, "Stuff");
					stuff.GetAllValues().Should().Equal("One");

					new Action(() => Put(stuff, "Two"))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					stuff.GetAllValues().Should().Equal("One");
				}
			}
		}

		[Test]
		public void TestClearReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: false))
				{
					var collection = CreateCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = GetCollection(db, "Stuff");
					collection.GetAllValues().Should().Equal("One");

					new Action(() => collection.Clear())
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One");
				}
			}
		}

		[Test]
		public void TestPutManyReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, false))
				{
					var collection = CreateCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = GetCollection(db, "Stuff");
					collection.GetAllValues().Should().Equal("One");

					new Action(() => PutMany(collection, "Two", "Three"))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One");
				}
			}
		}

		[Test]
		public void TestRemoveReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, false))
				{
					var collection = CreateCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
				{
					var collection = GetCollection(db, "Stuff");
					collection.GetAllValues().Should().Equal("One");

					new Action(() => RemoveLastPutValue(collection))
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");

					collection.GetAllValues().Should().Equal("One");
				}
			}
		}

		[Test]
		public void TestGetCollectionsAfterReopen()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, false))
				{
					db.Collections.Should().BeEmpty();
					var collection = CreateCollection(db, "Stuff");
					db.Collections.Should().Equal(collection);
				}

				using (var db = new IsabelDb(connection, null, NoCustomTypes, false, isReadOnly: true))
				{
					db.Collections.Should().HaveCount(1);
					var collection = db.Collections.First();
					var actualCollection = GetCollection(db, "Stuff");
					collection.Should().BeSameAs(actualCollection);
				}
			}
		}

		[Test]
		public void TestRestoreCollection()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					CreateCollection(db, "For The Fallen Dreams");
				}

				using (var db = CreateDatabase(connection))
				{
					db.Collections.Should().HaveCount(1);
					var collection = db.Collections.First();
					collection.Name.Should().Be("For The Fallen Dreams");
					collection.Type.Should().Be(CollectionType);
				}
			}
		}

		#endregion

		#region Removing Collections

		[Test]
		public void TestRemove1()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "For The Fallen Dreams");
					db.Collections.Should().Equal(collection);
					db.Remove(collection);
					db.Collections.Should().BeEmpty();
				}
			}
		}

		[Test]
		[Description("Verifies that removed collections remain removed even after the connection is closed")]
		public void TestRemove2()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "For The Fallen Dreams");
					db.Collections.Should().Equal(collection);
					db.Remove(collection);
					db.Collections.Should().BeEmpty();
				}

				using (var db = CreateDatabase(connection))
				{
					db.Collections.Should().BeEmpty();
				}
			}
		}
		
		[Test]
		[Description("Verifies that removing the same collection twice is allowed and doesn't do anything further")]
		public void TestRemove3()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "For The Fallen Dreams");
				db.Remove(collection);
				new Action(() => db.Remove(collection)).Should().NotThrow();
			}
		}

		[Test]
		[Description("Verifies that the data from removed collections isn't available anymore")]
		public void TestRemove4()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection))
				{
					var collection = CreateCollection(db, "Characters");
					Put(collection, "Harry Bosch");
					db.Remove(collection);
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = GetOrCreateCollection(db, "Characters");
					collection.GetAllValues().Should().BeEmpty();
				}
			}
		}

		[Test]
		[Description("Verifies that remove ignores collections from other databases, even if they have the same name")]
		public void TestRemove5()
		{
			using (var connection1 = CreateConnection())
			using (var db1 = CreateDatabase(connection1))
			using (var connection2 = CreateConnection())
			using (var db2 = CreateDatabase(connection2))
			{
				var collection1 = CreateCollection(db1, "Characters");
				Put(collection1, "Harry Bosch");

				var collection2 = CreateCollection(db2, "Characters");
				Put(collection2, "Jerry Edgar");

				new Action(() => db1.Remove(collection2)).Should().NotThrow();
				db1.Collections.Should().Equal(collection1);
				collection1.GetAllValues().Should().Equal("Harry Bosch");
			}
		}

		[Test]
		public void TestRemove6()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				new Action(() => db.Remove((ICollection)null)).Should().NotThrow();
			}
		}

		[Test]
		[Description("Verifies that remove doesn't remove another collection with the same name")]
		public void TestRemove7()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection1 = CreateCollection(db, "Characters");
				Put(collection1, "Harry Bosch");
				db.Remove(collection1);

				var collection2 = GetOrCreateCollection(db, "Characters");
				Put(collection2, "Jerry Edgar");

				new Action(() => db.Remove(collection1)).Should().NotThrow();
				collection2.GetAllValues().Should().Equal("Jerry Edgar");
			}
		}

		[Test]
		[Description("Verifies that reading data from a removed collection no longer is allowed")]
		public void TestCountRemovedCollection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => collection.Count())
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		[Description("Verifies that reading data from a removed collection no longer is allowed")]
		public void TestGetAllValuesRemovedCollection()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => collection.GetAllValues())
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		[Description("Verifies that writing data to a removed collection is NOT allowed and throws")]
		public void TestRemove9()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => Put(collection, "Madeline Bosch"))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		[Description("Verifies that writing data to a removed collection is NOT allowed and throws")]
		public void TestRemove10()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => PutMany(collection, "Eleanor Wish", "Maddeline Bosch"))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		[Description("Verifies that clearing a removed collection is NOT allowed")]
		public void TestRemove11()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => collection.Clear())
					.Should()
					.Throw<InvalidOperationException>()
					.WithMessage("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		[Test]
		[Description("Verifies that ICollection.ToString() changes once a collection has been removed")]
		public void TestRemove12()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = CreateCollection(db, "Characters");
				db.Remove(collection);

				collection.ToString().Should().Be("This collection (\"Characters\") has been removed from the database and may no longer be used");
			}
		}

		#endregion

		protected abstract CollectionType CollectionType { get; }
		protected abstract TCollection GetCollection(IDatabase db, string name);
		protected abstract TCollection CreateCollection(IDatabase db, string name);
		protected abstract TCollection GetOrCreateCollection(IDatabase db, string name);
		protected abstract void Put(TCollection collection, string value);
		protected abstract void PutMany(TCollection collection, params string[] values);
		protected abstract void RemoveLastPutValue(TCollection collection);
	}
}