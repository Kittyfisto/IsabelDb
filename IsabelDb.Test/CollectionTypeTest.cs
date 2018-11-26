using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class CollectionTypeTest
	{
		[Test]
		public void TestUnknown()
		{
			((int) CollectionType.Unknown).Should().Be(0, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestBag()
		{
			((int) CollectionType.Bag).Should().Be(1, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestDictionary()
		{
			((int) CollectionType.Dictionary).Should().Be(2, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestMultiValueDictionary()
		{
			((int) CollectionType.MultiValueDictionary).Should().Be(3, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestIntervalCollection()
		{
			((int) CollectionType.IntervalCollection).Should().Be(4, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestOrderedCollection()
		{
			((int) CollectionType.OrderedCollection).Should().Be(5, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestPoint2DCollection()
		{
			((int) CollectionType.Point2DCollection).Should().Be(6, "because the numeric value for this enum member may NEVER change");
		}

		[Test]
		public void TestQueue()
		{
			((int) CollectionType.Queue).Should().Be(7, "because the numeric value for this enum member may NEVER change");
		}
	}
}
