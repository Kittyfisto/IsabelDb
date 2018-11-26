﻿using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;
using System.Collections.Generic;

namespace IsabelDb.Test.Collections.Queue
{
	[TestFixture]
	public sealed class QueueTest
		: AbstractCollectionTest<IQueue<string>>
	{
		[Test]
		public void TestEnqueueDequeue()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(SomeStruct)))
			{
				var collection = db.GetQueue<SomeStruct>("Messages");
				collection.Enqueue(new SomeStruct {Value = "Hello"});
				collection.Count().Should().Be(1);

				collection.Enqueue(new SomeStruct {Value = "World"});
				collection.Count().Should().Be(2);

				collection.TryDequeue(out var message).Should().BeTrue();
				message.Value.Should().Be("Hello");
				collection.Count().Should().Be(1);

				collection.TryDequeue(out message).Should().BeTrue();
				message.Value.Should().Be("World");
				collection.Count().Should().Be(0);

				collection.TryDequeue(out message).Should().BeFalse();
				message.Should().Be(new SomeStruct());
			}
		}

		[Test]
		public void TestTryPeek()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(SomeStruct)))
			{
				var collection = db.GetQueue<SomeStruct>("Messages");
				collection.Enqueue(new SomeStruct {Value = "Hello"});
				collection.Enqueue(new SomeStruct {Value = "World"});

				collection.TryPeek(out var message).Should().BeTrue();
				message.Value.Should().Be("Hello");
				collection.Count().Should().Be(2);

				collection.TryPeek(out message).Should().BeTrue();
				message.Value.Should().Be("Hello");
				collection.Count().Should().Be(2);
			}
		}

		[Test]
		public void TestTryPeekTryDequeue()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(SomeStruct)))
			{
				var collection = db.GetQueue<SomeStruct>("Messages");
				collection.Enqueue(new SomeStruct {Value = "Hello"});
				collection.Enqueue(new SomeStruct {Value = "World"});

				collection.TryPeek(out var message).Should().BeTrue();
				message.Value.Should().Be("Hello");
				collection.Count().Should().Be(2);

				collection.TryDequeue(out message).Should().BeTrue();
				message.Value.Should().Be("Hello");
				collection.Count().Should().Be(1);

				collection.TryPeek(out message).Should().BeTrue();
				message.Value.Should().Be("World");
			}
		}

		[Test]
		public void TestEmpty()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(Message)))
			{
				var collection = db.GetQueue<Message>("Messages");
				collection.TryPeek(out var message).Should().BeFalse();
				message.Should().BeNull();

				collection.TryDequeue(out message).Should().BeFalse();
				message.Should().BeNull();
			}
		}

		[Test]
		public void TestEnqueueAlot()
		{
			using (var connection = CreateConnection())
			using (var db = CreateDatabase(connection, typeof(Dog)))
			{
				var collection = db.GetQueue<Dog>("Dogs");
				const int count = 4096;

				var dogs = new Queue<Dog>();
				for (int i = 0; i < count; ++i)
				{
					var dog = new Dog
					{
						Age = i,
						EyeColor = i % 2 == 0 ? "Yellow" : "Brown"
					};
					dogs.Enqueue(dog);
					collection.Enqueue(dog);
				}

				while (dogs.Count > 0)
				{
					var expectedDog = dogs.Dequeue();
					collection.TryDequeue(out var actualDog).Should().BeTrue();
					actualDog.Should().Be(expectedDog);
				}

				collection.TryDequeue(out var unused).Should().BeFalse();
				unused.Should().BeNull();
			}
		}

		#region Overrides of AbstractCollectionTest<IQueue<string>>

		protected override CollectionType CollectionType => CollectionType.Queue;

		protected override IQueue<string> GetCollection(IDatabase db, string name)
		{
			return db.GetQueue<string>(name);
		}

		protected override void Put(IQueue<string> collection, string value)
		{
			collection.Enqueue(value);
		}

		protected override void PutMany(IQueue<string> collection, params string[] values)
		{
			collection.EnqueueMany(values);
		}

		protected override void RemoveLastPutValue(IQueue<string> collection)
		{
			collection.TryDequeue(out var unused);
		}

		#endregion
	}
}