using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using FluentAssertions;
using IsabelDb.Test.Entities;
using IsabelDb.TypeModels;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class DatabaseTest
	{
		private static IEnumerable<Type> NoCustomTypes => new Type[0];

		[Test]
		public void TestToStringInMemory()
		{
			using (var database = Database.CreateInMemory(new Type[0]))
			{
				database.ToString().Should().Be("IsabelDb: In memory (0 collection(s))");
			}
		}

		#region Incompatible Versions

		[Test]
		public void TestMissingVariablesTable()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();

				TypeModel.CreateTable(connection);
				CollectionsTable.CreateTable(connection);

				new Action(() => Database.Open(connection, null, new Type[0], false))
					.Should().Throw<IncompatibleDatabaseSchemaException>()
					.WithMessage("The database is missing the 'isabel_variables' table. It may have been created with an early vesion of IsabelDb or it may not even be an IsabelDb file. Are you sure the path is correct?");
			}
		}

		[Test]
		public void TestMissingTypeModelTable()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();

				VariablesTable.CreateTable(connection);
				CollectionsTable.CreateTable(connection);

				new Action(() => Database.Open(connection, null, new Type[0], false))
					.Should().Throw<IncompatibleDatabaseSchemaException>()
					.WithMessage("The database is missing the 'isabel_types' table. The table may have been deleted or this may not even be an IsabelDb file. Are you sure the path is correct?");
			}
		}

		[Test]
		public void TestMissingCollectionsTable()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();
				
				VariablesTable.CreateTable(connection);
				TypeModel.CreateTable(connection);

				new Action(() => Database.Open(connection, null, new Type[0], false))
					.Should().Throw<IncompatibleDatabaseSchemaException>()
					.WithMessage("The database is missing the 'isabel_collections' table. The table may have been deleted or this may not even be an IsabelDb file. Are you sure the path is correct?");
			}
		}

		[Test]
		public void TestOldDatabaseSchema()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();

				VariablesTable.CreateTable(connection);
				TypeModel.CreateTable(connection);
				CollectionsTable.CreateTable(connection);

				using (var command = connection.CreateCommand())
				{
					command.CommandText = string.Format("INSERT OR REPLACE INTO {0} (key, value) VALUES (@key, @value)", VariablesTable.TableName);
					command.Parameters.AddWithValue("@key", VariablesTable.IsabelSchemaVersionKey);
					command.Parameters.AddWithValue("@value", 0);
					command.ExecuteNonQuery();
				}

				new Action(() => Database.Open(connection, null, new Type[0], false))
					.Should().Throw<IncompatibleDatabaseSchemaException>()
					.WithMessage("The database was created with an earlier version of IsabelDb (Schema version: 0) and its schema is incompatible to this version.");
			}
		}

		[Test]
		public void TestNewerDatabaseSchema()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();

				VariablesTable.CreateTable(connection);
				TypeModel.CreateTable(connection);
				CollectionsTable.CreateTable(connection);

				using (var command = connection.CreateCommand())
				{
					command.CommandText = string.Format("INSERT OR REPLACE INTO {0} (key, value) VALUES (@key, @value)", VariablesTable.TableName);
					command.Parameters.AddWithValue("@key", VariablesTable.IsabelSchemaVersionKey);
					command.Parameters.AddWithValue("@value", 101);
					command.ExecuteNonQuery();
				}

				new Action(() => Database.Open(connection, null, new Type[0], false))
					.Should().Throw<IncompatibleDatabaseSchemaException>()
					.WithMessage("The database was created with a newer version of IsabelDb (Schema version: 101) and its schema is incompatible to this version.");
			}
		}

		#endregion

		[Test]
		public void TestCreateInMemory()
		{
			Database.CreateInMemory(NoCustomTypes);
		}

		[Test]
		public void TestGetWrongBag()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetBag<int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a Bag");
			}
		}

		[Test]
		public void TestGetWrongQueue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetQueue<int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a Queue");
			}
		}

		[Test]
		public void TestGetWrongDictionary()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetBag<int>("Stuff");
				new Action(() => db.GetDictionary<int, int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Bag and cannot be treated as a Dictionary");
			}
		}
		
		[Test]
		public void TestGetWrongMultiValueDictionary()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetMultiValueDictionary<int, int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a MultiValueDictionary");
			}
		}

		[Test]
		public void TestGetWrongOrderedCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetOrderedCollection<int, int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a OrderedCollection");
			}
		}

		[Test]
		public void TestGetWrongIntervalCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetIntervalCollection<int, int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a IntervalCollection");
			}
		}

		[Test]
		public void TestGetWrongPoint2DCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<int, int>("Stuff");
				new Action(() => db.GetPoint2DCollection<int>("Stuff"))
					.Should().Throw<WrongCollectionTypeException>()
					.WithMessage("The collection 'Stuff' is a Dictionary and cannot be treated as a Point2DCollection");
			}
		}

		[Test]
		[Description("Verifies that creating a bag for a non-registered custom type is not allowed")]
		public void TestGetBagNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetBag<CustomKey>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a queue for a non-registered custom type is not allowed")]
		public void TestGetQueueNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetQueue<CustomKey>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a 2d point collection for a non-registered custom type is not allowed")]
		public void TestGetPoint2DCollectionNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetPoint2DCollection<Message>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.Message' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a dictionary for a non-registered custom type is not allowed")]
		public void TestGetDictionaryNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetDictionary<CustomKey, string>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");

				new Action(() => db.GetDictionary<string, Point>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.Point' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a multi value dictionary for a non-registered custom type is not allowed")]
		public void TestGetMultiValueDictionaryNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetMultiValueDictionary<CustomKey, string>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");

				new Action(() => db.GetMultiValueDictionary<string, Point>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.Point' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating an ordered collection for a non-registered custom type is not allowed")]
		public void TestGetOrderedCollectionNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetOrderedCollection<MySortableKey, string>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.MySortableKey' has not been registered when the database was created and thus may not be used as the value type in a collection");

				new Action(() => db.GetDictionary<string, Point>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.Point' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating an interval collection for a non-registered custom type is not allowed")]
		public void TestGetIntervalCollectionNonRegisteredType()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetIntervalCollection<MySortableKey, string>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.MySortableKey' has not been registered when the database was created and thus may not be used as the value type in a collection");

				new Action(() => db.GetIntervalCollection<string, Point>("SomeTable"))
					.Should().Throw<TypeNotRegisteredException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.Point' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that putting an object of a non-registered type in a collection is not allowed")]
		public void TestGetPutRegistered1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("SomeTable");
				new Action(() => values.Put("Foo", new CustomKey()))
					.Should().Throw<ArgumentException>();
			}
		}

		[Test]
		[Description("Verifies that putting an object of a non-registered type in a collection is not allowed")]
		public void TestGetPutRegistered2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<object, string>("SomeTable");
				new Action(() => values.Put(new CustomKey(), "Foo"))
					.Should().Throw<ArgumentException>();
			}
		}

		[Test]
		[Description("Verifies that putting an object of a non-registered type in a collection is not allowed")]
		public void TestGetPutRegistered3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetBag<object>("SomeTable");
				new Action(() => values.Put(new CustomKey()))
					.Should().Throw<ArgumentException>();
			}
		}

		[Test]
		public void TestOpenOrCreate1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutGetCustomType()
		{
			using (var db = Database.CreateInMemory(new []{typeof(Person)}))
			{
				var value1 = new Person {Name = "Strelok"};
				var value2 = new Person {Name = "The marked one"};
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt16()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = short.MinValue;
				var value2 = short.MaxValue;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt32()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = int.MinValue;
				var value2 = int.MaxValue;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetInt64()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = long.MinValue;
				var value2 = long.MaxValue;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetString()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = "Strelok";
				var value2 = "The marked one";
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetFloat()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = (float)Math.PI;
				var value2 = (float)Math.E;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetDouble()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = Math.PI;
				var value2 = Math.E;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetByte()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				byte value1 = byte.MinValue;
				byte value2 = byte.MaxValue;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetSByte()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				sbyte value1 = sbyte.MinValue;
				sbyte value2 = sbyte.MaxValue;
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetByteArray1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = new byte[]{0};
				var value2 = new byte[]{0, 255, 128, 42, 1};
				var store = db.GetDictionary<string, object>("ObjectTable");

				store.Put("foo", value1);
				store.Put("bar", value2);

				var persons = store.GetMany("foo", "bar");
				persons.Should().HaveCount(2);
				var actualValue1 = persons.ElementAt(index: 0);
				actualValue1.Key.Should().Be("foo");
				((byte[])actualValue1.Value).Should().Equal(value1);

				var actualValue2 = persons.ElementAt(index: 1);
				actualValue2.Key.Should().Be("bar");
				((byte[])actualValue2.Value).Should().Equal(value2);
			}
		}

		[Test]
		public void TestPutGetByteArray2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = new byte[]{0};
				var value2 = new byte[]{0, 255, 128, 42, 1};
				var store = db.GetDictionary<string, byte[]>("ObjectTable");

				store.Put("foo", value1);
				store.Put("bar", value2);

				var persons = store.GetMany("foo", "bar");
				persons.Should().HaveCount(2);
				var actualValue1 = persons.ElementAt(index: 0);
				actualValue1.Key.Should().Be("foo");
				actualValue1.Value.Should().Equal(value1);

				var actualValue2 = persons.ElementAt(index: 1);
				actualValue2.Key.Should().Be("bar");
				actualValue2.Value.Should().Equal(value2);
			}
		}

		[Test]
		public void TestPutGetDateTime()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = new DateTime(2018, 06, 07, 13, 25, 23);
				var value2 = new DateTime(2018, 06, 07, 13, 26, 02);
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetGuid()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var value1 = Guid.NewGuid();
				var value2 = Guid.Parse("C6138CD7-EBFB-4BBD-A82F-493FF2F0828D");
				PutAndGetObjectTable(db, value1, value2);
				PutAndGetValueTable(db, value1, value2);
			}
		}

		[Test]
		public void TestPutGetPoint()
		{
			using (var db = Database.CreateInMemory(new []{typeof(Point)}))
			{
				var store = db.GetDictionary<int, Point>("Points");
				var p0 = new Point {X = 0, Y = 0};
				var p1 = new Point {X = 1, Y = 1};
				var p2 = new Point {X = -3600, Y = 9000};
				var p3 = new Point {X = Math.E, Y = Math.PI};

				store.Put(0, p0);
				store.Put(1, p1);
				store.Put(2, p2);
				store.Put(3, p3);

				store.Get(0).Should().Be(p0);
				store.Get(1).Should().Be(p1);
				store.Get(2).Should().Be(p2);
				store.Get(3).Should().Be(p3);
			}
		}

		[Test]
		public void TestPutSomeStruct()
		{
			using (var db = Database.CreateInMemory(new[] {typeof(SomeStruct)}))
			{
				var values = db.GetDictionary<int, SomeStruct>("Values");
				values.Put(42, new SomeStruct{Value = "Answer to the Ultimate Question of Life, the Universe, and Everything"});
				values.Get(42).Value.Should()
				      .Be("Answer to the Ultimate Question of Life, the Universe, and Everything");
			}
		}

		[Test]
		[Description("Verifies that an automatic rollback is performed if the transaction is not specifically committed")]
		public void TestRollbackTransaction()
		{
			using (var db = Database.CreateInMemory(new[] {typeof(Address), typeof(Person)}))
			{
				var values = db.GetDictionary<int, string>("Values");
				using (var transaction = db.BeginTransaction())
				{
					values.Put(1, "stuff");
					values.Get(1).Should().Be("stuff");
				}

				values.TryGet(1, out var value).Should().BeFalse();
				value.Should().BeNull();
			}
		}

		[Test]
		public void TestCommitTransaction()
		{
			using (var db = Database.CreateInMemory(new[] {typeof(Address), typeof(Person)}))
			{
				var values = db.GetDictionary<int, string>("Values");
				using (var transaction = db.BeginTransaction())
				{
					values.Put(1, "a");
					values.Put(2, "b");
					values.Get(1).Should().Be("a");
					transaction.Commit();
				}

				values.Get(1).Should().Be("a");
				values.Get(2).Should().Be("b");
			}
		}

		[Test]
		public void TestPutMany1()
		{
			using (var db = Database.CreateInMemory(new []{typeof(Address), typeof(Person)}))
			{
				var address = new Address
				{
					Country = "Germany",
					City = "Berlin",
					Street = "Any",
					Number = 42
				};
				var steven = new Person
				{
					Id = 42,
					Name = "Steven"
				};

				var table = db.GetDictionary<string, object>("Stuff");
				table.PutMany(new[]
				{
					new KeyValuePair<string, object>("foo", address),
					new KeyValuePair<string, object>("bar", steven)
				});


				table.Count().Should().Be(2);
				table.Get("bar").Should().Be(steven);
				table.Get("foo").Should().Be(address);
			}
		}

		[Test]
		[Description("Verifies that data from different tables doesn't interact with each other")]
		public void TestPutMultipleTables()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var customers = db.GetDictionary<string, object>("Customers");
				customers.Put("1", "Simon");

				var people = db.GetDictionary<string, object>("People");
				people.Put("1", "Kitty");

				customers.Get("1").Should().Be("Simon");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		public void TestPutNullKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Put(key: null, value: 42)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		[Description("Verifies that data removed from one collection doesn't interact with others")]
		public void TestRemoveMultipleCollections()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var customers = db.GetDictionary<string, object>("Customers");
				customers.Put("1", "Simon");

				var people = db.GetDictionary<string, object>("People");
				people.Put("1", "Kitty");

				customers.Remove("1");
				people.Get("1").Should().Be("Kitty");
			}
		}

		[Test]
		public void TestRemoveCollectionByName()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("Customers");
				db.Collections.Should().HaveCount(1);

				db.Remove("Customers");
				db.Collections.Should().BeEmpty();
			}
		}

		[Test]
		public void TestRemoveNonExistingCollectionByName()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<string, object>("Customers");
				db.Collections.Should().Equal(collection);

				db.Remove("Some other collection");
				db.Collections.Should().Equal(collection);
			}
		}

		[Test]
		public void TestRemoveNullName()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var collection = db.GetDictionary<string, object>("Customers");
				db.Collections.Should().Equal(collection);

				db.Remove((string)null);
				db.Collections.Should().Equal(collection);
			}
		}

		[Test]
		public void TestRemoveNonExistingKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.TryGet("42", out var unused).Should().BeFalse();

				store.Put("42", value: null);
				store.TryGet("42", out unused).Should().BeFalse();
			}
		}

		[Test]
		public void TestRemoveNullKey()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Remove(key: null)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", value: null);
				store.TryGet("foo", out var unused).Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that ONLY the value with the specified key is removed and none other")]
		public void TestRemoveValue2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("a", value: 1);
				store.Put("b", value: 2);
				store.Remove("a");

				store.TryGet("a", out var unused).Should().BeFalse();
				store.Get("b").Should().Be(2);
			}
		}

		[Test]
		public void TestReplaceValue()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(42);

				store.Put("foo", value: 50);
				store.Get("foo").Should().Be(50);
			}
		}

		[Test]
		public void TestPutGetPolymorphicGraph()
		{
			var types = new[]
			{
				typeof(Message),
				typeof(Animal),
				typeof(Dog)
			};
			using (var db = Database.CreateInMemory(types))
			{
				var messages = db.GetDictionary<int, Message>("Messages");
				messages.Put(1, new Message
				{
					Value = new Dog
					{
						Name = "Goofy"
					}
				});

				var message = messages.Get(1);
				message.Value.Should().BeOfType<Dog>();
				((Dog) message.Value).Name.Should().Be("Goofy");
			}
		}

		private static void PutAndGetObjectTable<T>(IDatabase db, T value1, T value2)
		{
			var store = db.GetDictionary<string, object>("ObjectTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(2);
			var actualValue1 = persons.ElementAt(index: 0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(index: 1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}

		private static void PutAndGetValueTable<T>(IDatabase db, T value1, T value2)
		{
			var store = db.GetDictionary<string, T>("ValueTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(2);
			var actualValue1 = persons.ElementAt(index: 0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(index: 1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}
	}
}