using System.Collections.Generic;
using FluentAssertions;
using IsabelDb.Serializers;
using NUnit.Framework;

namespace IsabelDb.Test.Serializers
{
	[TestFixture]
	public sealed class UInt64SerializerTest
	{
		public static IEnumerable<ulong> Values => new ulong[]
		{
			0,
			(ulong) sbyte.MaxValue,
			byte.MaxValue,
			(ulong) short.MaxValue,
			ushort.MaxValue,
			int.MaxValue,
			uint.MaxValue,
			long.MaxValue,
			ulong.MaxValue
		};

		[Test]
		public void TestRoundtrip([ValueSource(nameof(Values))] ulong value)
		{
			var actualValue = UInt64Serializer.FromDatabase(UInt64Serializer.ToDatabase(value));
			actualValue.Should().Be(value);
		}

		[Test]
		public void TestOrder1()
		{
			var a = UInt64Serializer.ToDatabase(0);
			var b = UInt64Serializer.ToDatabase(1);
			a.Should().BeLessThan(b);
		}

		[Test]
		public void TestOrder2()
		{
			var a = UInt64Serializer.ToDatabase(long.MaxValue);
			var b = UInt64Serializer.ToDatabase((ulong)long.MaxValue+1);
			a.Should().BeLessThan(b);
		}

		[Test]
		public void TestOrder3()
		{
			var a = UInt64Serializer.ToDatabase(0);
			var b = UInt64Serializer.ToDatabase(ulong.MaxValue-1);
			a.Should().BeLessThan(b);
		}

		[Test]
		public void TestOrder4()
		{
			var a = UInt64Serializer.ToDatabase(0);
			var b = UInt64Serializer.ToDatabase(ulong.MaxValue);
			a.Should().BeLessThan(b);
		}
	}
}
