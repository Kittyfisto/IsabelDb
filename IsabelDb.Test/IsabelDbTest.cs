using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class IsabelDbTest
	{
		private static IEnumerable<Type> NoCustomTypes => new Type[0];

		private static void PutAndGetObjectTable<T>(IsabelDb db, T value1, T value2)
		{
			var store = db.GetDictionary<string, object>("ObjectTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(expected: 2);
			var actualValue1 = persons.ElementAt(index: 0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(index: 1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}

		private static void PutAndGetValueTable<T>(IsabelDb db, T value1, T value2)
		{
			var store = db.GetDictionary<string, T>("ValueTable");

			store.Put("foo", value1);
			store.Put("bar", value2);

			var persons = store.GetMany("foo", "bar");
			persons.Should().HaveCount(expected: 2);
			var actualValue1 = persons.ElementAt(index: 0);
			actualValue1.Key.Should().Be("foo");
			actualValue1.Value.Should().Be(value1);

			var actualValue2 = persons.ElementAt(index: 1);
			actualValue2.Key.Should().Be("bar");
			actualValue2.Value.Should().Be(value2);
		}

		[Test]
		[Description("Verifies that clearing an empty dictionary is allowed")]
		public void TestClearEmpty()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var people = db.GetDictionary<string, object>("People");
				people.Count().Should().Be(expected: 0);
				people.Clear();
				people.Count().Should().Be(expected: 0);
			}
		}

		[Test]
		public void TestCreateInMemory()
		{
			IsabelDb.CreateInMemory(NoCustomTypes);
		}

		[Test]
		[Description("Verifies that creating a collection for a non-registered custom type is not allowed")]
		public void TestGetCollectionNonRegisteredType1()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
					new Action(() => db.GetDictionary<string, CustomKey>("SomeTable"))
					.Should().Throw<ArgumentException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a collection for a non-registered custom type is not allowed")]
		public void TestGetCollectionNonRegisteredType2()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetDictionary<CustomKey, string>("SomeTable"))
					.Should().Throw<ArgumentException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the key type in a collection");
			}
		}

		[Test]
		[Description("Verifies that creating a collection for a non-registered custom type is not allowed")]
		public void TestGetCollectionNonRegisteredType3()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				new Action(() => db.GetBag<CustomKey>("SomeTable"))
					.Should().Throw<ArgumentException>()
					.WithMessage("The type 'IsabelDb.Test.Entities.CustomKey' has not been registered when the database was created and thus may not be used as the value type in a collection");
			}
		}

		[Test]
		[Description("Verifies that putting an object of a non-registered type in a collection is not allowed")]
		public void TestGetPutRegistered1()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetBag<object>("SomeTable");
				new Action(() => values.Put(new CustomKey()))
					.Should().Throw<ArgumentException>();
			}
		}

		[Test]
		public void TestGet1()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").Put("foo", "bar");
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().Be("bar");
			}
		}

		[Test]
		[Description("Verifies that GetDictionary allows for names to be case sensitive")]
		public void TestGetDictionaryCaseSensitiveNames()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var a = db.GetDictionary<string, object>("A");
				var b = db.GetDictionary<string, object>("a");
				a.Should().NotBe(b);

				a.Put("1", value: 42);
				b.Put("1", value: 50);

				a.Get("1").Should().Be(expected: 42);
				b.Get("1").Should().Be(expected: 50);
			}
		}

		[Test]
		public void TestGetDictionaryDifferentTypes()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				db.GetDictionary<string, object>("SomeTable").Get("foo").Should().BeNull();
				db.GetDictionary<string, object>("SomeTable").GetMany("foo", "bar").Should().BeEmpty();
			}
		}

		[Test]
		public void TestOpenOrCreate1()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutGetCustomType()
		{
			using (var db = IsabelDb.CreateInMemory(new []{typeof(Person)}))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var value1 = new byte[]{0};
				var value2 = new byte[]{0, 255, 128, 42, 1};
				var store = db.GetDictionary<string, object>("ObjectTable");

				store.Put("foo", value1);
				store.Put("bar", value2);

				var persons = store.GetMany("foo", "bar");
				persons.Should().HaveCount(expected: 2);
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var value1 = new byte[]{0};
				var value2 = new byte[]{0, 255, 128, 42, 1};
				var store = db.GetDictionary<string, byte[]>("ObjectTable");

				store.Put("foo", value1);
				store.Put("bar", value2);

				var persons = store.GetMany("foo", "bar");
				persons.Should().HaveCount(expected: 2);
				var actualValue1 = persons.ElementAt(index: 0);
				actualValue1.Key.Should().Be("foo");
				actualValue1.Value.Should().Equal(value1);

				var actualValue2 = persons.ElementAt(index: 1);
				actualValue2.Key.Should().Be("bar");
				actualValue2.Value.Should().Equal(value2);
			}
		}

		[Test]
		public void TestPutGetPoint()
		{
			using (var db = IsabelDb.CreateInMemory(new []{typeof(Point)}))
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
		public void TestPutMany1()
		{
			using (var db = IsabelDb.CreateInMemory(new []{typeof(Address), typeof(Person)}))
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


				table.Count().Should().Be(expected: 2);
				table.Get("bar").Should().Be(steven);
				table.Get("foo").Should().Be(address);
			}
		}

		[Test]
		[Description("Verifies that data from different tables doesn't interact with each other")]
		public void TestPutMultipleTables()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Put(key: null, value: 42)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		[Description("Verifies that data removed from one table doesn't interact with others")]
		public void TestRemoveMultipleTables()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
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
		public void TestRemoveNonExistingKey()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Get("42").Should().BeNull();

				store.Put("42", value: null);
				store.Get("42").Should().BeNull();
			}
		}

		[Test]
		public void TestRemoveNullKey()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<string, object>("Foo");
				new Action(() => values.Remove(key: null)).Should().Throw<ArgumentNullException>();
			}
		}

		[Test]
		public void TestRemoveValue()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(expected: 42);

				store.Put("foo", value: null);
				store.Get("foo").Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that ONLY the value with the specified key is removed and none other")]
		public void TestRemoveValue2()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("a", value: 1);
				store.Put("b", value: 2);
				store.Remove("a");

				store.Get("a").Should().BeNull();
				store.Get("b").Should().Be(expected: 2);
			}
		}

		[Test]
		public void TestReplaceValue()
		{
			using (var db = IsabelDb.CreateInMemory(NoCustomTypes))
			{
				var store = db.GetDictionary<string, object>("SomeTable");
				store.Put("foo", value: 42);
				store.Get("foo").Should().Be(expected: 42);

				store.Put("foo", value: 50);
				store.Get("foo").Should().Be(expected: 50);
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
			using (var db = IsabelDb.CreateInMemory(types))
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
	}
}