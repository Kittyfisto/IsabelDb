using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class Point2DTest
	{
		[Test]
		public void TestEquality()
		{
			var a = new Point2D(1, 2);
			var b = new Point2D(2, 1);
			a.Equals(b).Should().BeFalse();
			b.Equals(a).Should().BeFalse();
			(a == b).Should().BeFalse();
			(a != b).Should().BeTrue();
		}

		[Test]
		public void TestToString()
		{
			new Point2D(1, 2).ToString().Should().Be("X: 1, Y: 2");
		}
	}
}
