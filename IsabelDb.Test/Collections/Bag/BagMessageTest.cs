﻿using System;
using System.Linq;
using FluentAssertions;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Bag
{
	[TestFixture]
	public sealed class BagMessageTest
	{
		[Test]
		[Description("Verifies that storing basic types in object fields is not supported (protobuf 'limitation')")]
		public void TestStoreObjectString()
		{
			var message = new Message
			{
				Value = "Foo"
			};
			using (var db = Database.CreateInMemory(new[] {typeof(Message)}))
			{
				var bag = db.GetOrCreateBag<Message>("Messages");
				new Action(() => bag.Put(message))
					.Should().Throw<Exception>("because protobuf doesn't support native types to inherit from anything");
				bag.Count().Should().Be(0);
				bag.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		[Description("Verifies that storing basic types in object fields is not supported (protobuf 'limitation')")]
		public void TestStoreObjectInt()
		{
			var message = new Message
			{
				Value = 42
			};
			using (var db = Database.CreateInMemory(new[] {typeof(Message)}))
			{
				var bag = db.GetOrCreateBag<Message>("Messages");
				new Action(() => bag.Put(message))
					.Should().Throw<Exception>("because protobuf doesn't support native types to inherit from anything");
				bag.Count().Should().Be(0);
				bag.GetAllValues().Should().BeEmpty();
			}
		}

		[Test]
		[Description("Verifies that storing custom types in object fields is supported")]
		public void TestStoreObjectKey()
		{
			var message = new Message
			{
				Value = new CustomKey()
			};
			using (var db = Database.CreateInMemory(new []{typeof(Message), typeof(CustomKey)}))
			{
				var bag = db.GetOrCreateBag<Message>("Messages");
				bag.Put(message);
				var actualValue = bag.GetAllValues().First();
				actualValue.Should().NotBeNull();
				actualValue.Value.Should().BeOfType<CustomKey>();
				actualValue.Value.Should().Be(message.Value);
			}
		}
	}
}