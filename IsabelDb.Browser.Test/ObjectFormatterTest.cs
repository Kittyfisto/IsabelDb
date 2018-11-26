using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Browser.Test
{
	[TestFixture]
	public sealed class ObjectFormatterTest
	{
		[Test]
		public void TestPreviewString()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview("Hello, World!")
			         .Should().Be("\"Hello, World!\"");
		}

		[Test]
		public void TestPreviewInt32()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(42)
			         .Should().Be("42");
		}

		[Test]
		public void TestPreviewInt64()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(-123456789)
			         .Should().Be("-123456789");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewFloat()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview((float)Math.PI)
			         .Should().Be("3.141593");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewFloatEllipses()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview((float)Math.PI, 6)
			         .Should().Be("3.1...");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewDouble()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(Math.E)
			         .Should().Be("2.71828182845905");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewKeyValuePair()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new KeyValuePair<int, int>(42, 1))
			         .Should().Be("{Key: 42, Value: 1}");
		}

		[Test]
		public void TestPreviewIntList()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new List<int>{1, 2, 3})
			         .Should().Be("{Count: 3, 1, 2, 3}");
		}

		[Test]
		public void TestPreviewIntArray()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new []{1, 2, 3, 4})
			         .Should().Be("{Count: 4, 1, 2, 3, 4}");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewDictionary()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new Dictionary<int, int>{{1, 42}, {2, 1337}})
			         .Should().Be("{Count: 2, {Key: 1, Value: 42}, {Key: 2, Value: 1337}}");
		}

		[Test]
		public void TestPreviewCustomType()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new DataObject
			{
				Property = "My code sucks",
				Value = 42
			}).Should().Be("{Property: \"My code sucks\", Value: 42}");
		}
	}
}