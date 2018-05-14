using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections
{
	public abstract class AbstractCollectionTest<TCollection>
		where TCollection : ICollection<string>
	{
		protected static readonly Type[] NoCustomTypes = new Type[0];

		[Test]
		public void TestGetSameCollectionTwice()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection1 = GetCollection(db, "Values1");
				var collection2 = GetCollection(db, "Values1");
				var collection3 = GetCollection(db, "Values2");

				collection1.Should().BeSameAs(collection2);
				collection1.Should().NotBeSameAs(collection3);
			}
		}

		[Test]
		public void TestGetAllValuesAgain()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "SomeTable");
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
				var a = GetCollection(db, "A");
				var b = GetCollection(db, "a");
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
				var collection = GetCollection(db, "Values");
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
				var collection = GetCollection(db, "Values");
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
				var collection = GetCollection(db, "Values");
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
				var collection = GetCollection(db, "Values");
				collection.Count().Should().Be(0);
			}
		}

		[Test]
		public void TestGetAllValuesEmpty()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllValuesOneValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
				Put(collection, "Monty");
				collection.GetAllValues().Should().Equal("Monty");
			}
		}

		[Test]
		public void TestGetAllValues()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");
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
				var collection = GetCollection(db, "Values");
				Put(collection, "Monty Python");
			}
		}

		[Test]
		public void TestPut()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = GetCollection(db, "Values");

				const int count = 1000;
				var values = new List<string>(count);
				for (int i = 0; i < count; ++i)
				{
					var value = "Stuff";
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
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					db.Collections.Should().BeEmpty();
					var collection = GetCollection(db, "Stuff");
					db.Collections.Should().Equal(collection);
				}
			}
		}

		[Test]
		public void TestCollectionName()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = GetCollection(db, "For The Fallen Dreams");
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
					var collection = GetCollection(db, "Values");
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
					var collection = GetCollection(db, "Values");
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
					var collection = GetCollection(db, "Values");
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
			using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
			{
				new Action(() => GetCollection((IDatabase) db, "Stuff"))
					.Should().Throw<ArgumentException>()
					.WithMessage("Unable to find a collection named 'Stuff'");
			}
		}

		[Test]
		public void TestPutReadOnlyDatabase()
		{
			using (var connection = CreateConnection())
			{
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = GetCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
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
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = GetCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
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
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = GetCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
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
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					var collection = GetCollection(db, "Stuff");
					Put(collection, "One");
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
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
				using (var db = new IsabelDb(connection, NoCustomTypes, false, false))
				{
					db.Collections.Should().BeEmpty();
					var collection = GetCollection(db, "Stuff");
					db.Collections.Should().Equal(collection);
				}

				using (var db = new IsabelDb(connection, NoCustomTypes, false, isReadOnly: true))
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
					GetCollection(db, "For The Fallen Dreams");
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
					var collection = GetCollection(db, "For The Fallen Dreams");
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
					var collection = GetCollection(db, "For The Fallen Dreams");
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
				var collection = GetCollection(db, "For The Fallen Dreams");
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
					var collection = GetCollection(db, "Characters");
					Put(collection, "Harry Bosch");
					db.Remove(collection);
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = GetCollection(db, "Characters");
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
				var collection1 = GetCollection(db1, "Characters");
				Put(collection1, "Harry Bosch");

				var collection2 = GetCollection(db2, "Characters");
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
				new Action(() => db.Remove(null)).Should().NotThrow();
			}
		}

		[Test]
		[Description("Verifies that remove doesn't remove another collection with the same name")]
		public void TestRemove7()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection1 = GetCollection(db, "Characters");
				Put(collection1, "Harry Bosch");
				db.Remove(collection1);

				var collection2 = GetCollection(db, "Characters");
				Put(collection2, "Jerry Edgar");

				new Action(() => db.Remove(collection1)).Should().NotThrow();
				collection2.GetAllValues().Should().Equal("Jerry Edgar");
			}
		}

		[Test]
		[Description("Verifies that reading data from a removed collection is allowed, but never returns any values")]
		public void TestRemove8()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = GetCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				collection.Count().Should().Be(0);
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		[Description("Verifies that writing data to a removed collection is NOT allowed and throws")]
		public void TestRemove9()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = GetCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => Put(collection, "Madeline Bosch"))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("This collection has been removed from the database and may no longer be modified");
			}
		}

		[Test]
		[Description("Verifies that writing data to a removed collection is NOT allowed and throws")]
		public void TestRemove10()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = GetCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => PutMany(collection, "Eleanor Wish", "Maddeline Bosch"))
					.Should().Throw<InvalidOperationException>()
					.WithMessage("This collection has been removed from the database and may no longer be modified");
			}
		}

		[Test]
		[Description("Verifies that clearing a removed collection is allowed and is a NOP")]
		public void TestRemove11()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection))
			{
				var collection = GetCollection(db, "Characters");
				Put(collection, "Harry Bosch");
				db.Remove(collection);

				new Action(() => collection.Clear()).Should().NotThrow();
				collection.GetAllValues().Should().BeEmpty();
			}
		}

		#endregion

		protected abstract CollectionType CollectionType { get; }
		protected abstract TCollection GetCollection(IDatabase db, string name);
		protected abstract void Put(TCollection collection, string value);
		protected abstract void PutMany(TCollection collection, params string[] values);
		protected abstract void RemoveLastPutValue(TCollection collection);

		protected SQLiteConnection CreateConnection()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			Database.CreateTables(connection);
			return connection;
		}

		protected IDatabase CreateDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, types, disposeConnection: false, isReadOnly: false);
		}

		protected IReadOnlyDatabase CreateReadOnlyDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, types, disposeConnection: false, isReadOnly: true);
		}
	}
}