using System;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class IntervalTest
	{
		[Test]
		public void TestCreate()
		{
			var interval = Interval.Create(10, 20);
			interval.Minimum.Should().Be(10);
			interval.Maximum.Should().Be(20);
		}

		[Test]
		public void TestCreateReversed()
		{
			var interval = Interval.Create(-100, -1000);
			interval.Minimum.Should().Be(-1000);
			interval.Maximum.Should().Be(-100);
		}

		[Test]
		public void TestToString()
		{
			var interval = Interval.Create(2, 4);
			interval.ToString().Should().Be("[2, 4]");
		}

		[Test]
		public void TestCreateContains1()
		{
			var interval = Interval.Create(Math.E, Math.PI);
			interval.Contains(3).Should().BeTrue();

			interval.Contains(4).Should().BeFalse();
			interval.Contains(2).Should().BeFalse();
		}

		[Test]
		public void TestCreateContains2()
		{
			var interval = Interval.Create(Math.E, Math.PI);
			interval.Contains(Math.E).Should().BeTrue();
			interval.Contains(Math.PI).Should().BeTrue();
		}
	}
}
