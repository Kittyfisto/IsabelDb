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
	}
}
