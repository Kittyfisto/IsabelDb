using System;
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

		[Test]
		public void TestSquaredDistance1()
		{
			Point2D.SquaredDistance(Point2D.Zero, Point2D.Zero).Should().Be(0);
			Point2D.SquaredDistance(new Point2D(1, 2), new Point2D(1, 2)).Should().Be(0);
		}

		[Test]
		public void TestSquaredDistance2()
		{
			Point2D.SquaredDistance(Point2D.Zero, new Point2D(1, 1)).Should().Be(2);
			Point2D.SquaredDistance(Point2D.Zero, new Point2D(1, -1)).Should().Be(2);
			Point2D.SquaredDistance(Point2D.Zero, new Point2D(-1, 1)).Should().Be(2);
			Point2D.SquaredDistance(Point2D.Zero, new Point2D(-1, -1)).Should().Be(2);
		}

		[Test]
		public void TestDistance1()
		{
			Point2D.Distance(Point2D.Zero, Point2D.Zero).Should().Be(0);
			Point2D.Distance(new Point2D(1, 2), new Point2D(1, 2)).Should().Be(0);
		}

		[Test]
		public void TestDistance2()
		{
			Point2D.Distance(Point2D.Zero, new Point2D(1, 1)).Should().Be(Math.Sqrt(2));
			Point2D.Distance(Point2D.Zero, new Point2D(1, -1)).Should().Be(Math.Sqrt(2));
			Point2D.Distance(Point2D.Zero, new Point2D(-1, 1)).Should().Be(Math.Sqrt(2));
			Point2D.Distance(Point2D.Zero, new Point2D(-1, -1)).Should().Be(Math.Sqrt(2));
		}
	}
}
