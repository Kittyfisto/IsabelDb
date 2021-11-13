using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections
{
	[TestFixture]
	public abstract class AbstractCollectionCompatibilityTest
	{
		protected SQLiteConnection CreateConnection()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			Database.CreateTables(connection);
			return connection;
		}

		protected IDatabase CreateDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, null, types, disposeConnection: false, isReadOnly: false);
		}

		protected IDatabase CreateReadOnlyDatabase(SQLiteConnection connection, params Type[] types)
		{
			return new IsabelDb(connection, null, types, disposeConnection: false, isReadOnly: true);
		}

		protected abstract ICollection<T> GetCollection<T>(IDatabase db, string name);
		protected abstract void Put<T>(ICollection<T> collection, T value);

		[Test]
		public void TestCanBeAccessed1()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					collection.CanBeAccessed.Should().BeTrue();
				}
			}
		}

		[Test]
		public void TestCanBeAccessed2()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					Put(collection, new Dog
					{
						Name = "Porthos"
					});
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = db.Collections.FirstOrDefault(x => Equals(x.Name, "Dogs"));
					collection.CanBeAccessed.Should().BeFalse("because the custom type used by the collection isn't known by this database");
				}
			}
		}

		[Test]
		public void TestClearUnaccessibleCollection()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					Put(collection, new Dog
					{
						Name = "Porthos"
					});
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = db.Collections.FirstOrDefault(x => Equals(x.Name, "Dogs"));
					collection.Count().Should().Be(1);
					new Action(() => collection.Clear()).Should().NotThrow();
					collection.Count().Should().Be(0);
				}

				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					collection.GetAllValues().Should().BeEmpty("because we've cleared the collection in a previous session");
				}
			}
		}

		[Test]
		public void TestGetAllValuesUnaccessibleCollection()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					Put(collection, new Dog
					{
						Name = "Porthos"
					});
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = db.Collections.FirstOrDefault(x => Equals(x.Name, "Dogs"));
					new Action(() => collection.GetAllValues())
						.Should().Throw<InvalidOperationException>()
						.WithMessage("This collection cannot be accessed because the type 'IsabelDb.Test.Entities.Dog' is not available!");
				}
			}
		}

		[Test]
		public void TestClearUnaccessibleReadOnlyCollection()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					Put(collection, new Dog
					{
						Name = "Porthos"
					});
				}

				using (var db = CreateReadOnlyDatabase(connection))
				{
					var collection = db.Collections.FirstOrDefault(x => Equals(x.Name, "Dogs"));
					collection.Count().Should().Be(1);
					new Action(() => collection.Clear())
						.Should().Throw<InvalidOperationException>()
						.WithMessage("The database has been opened read-only and therefore may not be modified");
					collection.Count().Should().Be(1, "because nothing should've been cleared");
				}
			}
		}

		[Test]
		public void TestRemoveUnaccessibleCollection()
		{
			using (var connection = CreateConnection())
			{
				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					var collection = GetCollection<Dog>(db, "Dogs");
					Put(collection, new Dog
					{
						Name = "Porthos"
					});
				}

				using (var db = CreateDatabase(connection))
				{
					var collection = db.Collections.FirstOrDefault(x => Equals(x.Name, "Dogs"));
					new Action(() => db.Remove(collection)).Should().NotThrow("because removing an inaccessible collection is still valid");
					db.Collections.Should().BeEmpty("because we've just removed the only collection from this database");
				}

				using (var db = CreateDatabase(connection, typeof(Dog)))
				{
					db.Collections.Should().BeEmpty("because we've just removed the only collection from this database");
				}
			}
		}
	}
}