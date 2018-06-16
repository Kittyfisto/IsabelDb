using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class Rectangle2DTest
	{
		[Test]
		[SetCulture("en-US")]
		public void TestToString()
		{
			new Rectangle2D
			{
				MinX = 1,
				MaxX = 2,
				MinY = -1,
				MaxY = 0.1
			}.ToString().Should().Be("MinX: 1, MinY: -1, MaxX: 2, MaxY: 0.1");
		}
	}
}