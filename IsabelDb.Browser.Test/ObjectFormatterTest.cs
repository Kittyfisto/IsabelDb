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
		public void TestPreviewByteArray()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new byte[0]).Should().Be("{}");
			formatter.Preview(new byte[]{0}).Should().Be("{0x00}");
			formatter.Preview(new byte[]{255}).Should().Be("{0xFF}");
			formatter.Preview(new byte[]{1, 2, 3}).Should().Be("{0x01, 0x02, 0x03}");
		}

		[Test]
		public void TestFormatDateTime()
		{
			var formatter = new ObjectFormatter();
			formatter.Format(new DateTime(2018, 11, 28, 19, 38, 31)).Should().Be("2018-11-28T19:38:31.0000000");
			formatter.Format(new DateTime(2018, 11, 28, 19, 38, 31, DateTimeKind.Utc)).Should().Be("2018-11-28T19:38:31.0000000Z");
		}

		[Test]
		public void TestPreviewDateTime()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new DateTime(2018, 11, 28, 19, 38, 31)).Should().Be("2018-11-28T19:38:31.0000000");
			formatter.Preview(new DateTime(2018, 11, 28, 19, 38, 31, DateTimeKind.Utc)).Should().Be("2018-11-28T19:38:31.0000000Z");
		}

		[Test]
		public void TestFormatString()
		{
			var formatter = new ObjectFormatter();
			formatter.Format("Hello, World!")
			         .Should().Be("\"Hello, World!\"");
		}

		[Test]
		public void TestPreviewString()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview("Hello, World!")
			         .Should().Be("\"Hello, World!\"");
		}

		[Test]
		public void TestPreviewSByte()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview((sbyte)-42)
			         .Should().Be("-42");
		}

		[Test]
		public void TestPreviewInt16()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview((short)-42)
			         .Should().Be("-42");
		}

		[Test]
		public void TestPreviewUInt16()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview((ushort)42)
			         .Should().Be("42");
		}

		[Test]
		public void TestFormatInt32()
		{
			var formatter = new ObjectFormatter();
			formatter.Format(42)
			         .Should().Be("42");
		}

		[Test]
		public void TestPreviewInt32()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(-42)
			         .Should().Be("-42");
		}

		[Test]
		public void TestPreviewUInt32()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(UInt32.MaxValue)
			         .Should().Be("4294967295");
		}

		[Test]
		public void TestFormatInt64()
		{
			var formatter = new ObjectFormatter();
			formatter.Format(-123456789)
			         .Should().Be("-123456789");
		}

		[Test]
		public void TestPreviewInt64()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(-123456789)
			         .Should().Be("-123456789");
		}

		[Test]
		public void TestPreviewUInt64()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(UInt64.MaxValue)
			         .Should().Be("18446744073709551615");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestFormatFloat()
		{
			var formatter = new ObjectFormatter();
			formatter.Format((float)Math.PI)
			         .Should().Be("3.141593");
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
		public void TestFormatDouble()
		{
			var formatter = new ObjectFormatter();
			formatter.Format(Math.E)
			         .Should().Be("2.71828182845905");
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
		public void TestFormatKeyValuePair()
		{
			var formatter = new ObjectFormatter();
			formatter.Format(new KeyValuePair<int, int>(42, 1))
			         .Should().Be("{\r\n\tKey: 42,\r\n\tValue: 1\r\n}");
		}

		[Test]
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
			         .Should().Be("{1, 2, 3}");
		}

		[Test]
		public void TestPreviewIntArray()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new []{1, 2, 3, 4})
			         .Should().Be("{1, 2, 3, 4}");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestPreviewDictionary()
		{
			var formatter = new ObjectFormatter();
			formatter.Preview(new Dictionary<int, int>{{1, 42}, {2, 1337}})
			         .Should().Be("{{Key: 1, Value: 42}, {Key: 2, Value: 1337}}");
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