using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.AcceptanceTest.Collections
{
	[TestFixture]
	public sealed class IntervalCollectionAcceptanceTest
	{
		private static readonly Type[] NoCustomTypes = new Type[0];

		[Test]
		public void TestManyOverlappingIntervals()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetIntervalCollection<int, string>("Values");
				const int count = 10000;
				const int spacing = 99;
				const int overlap = 100;

				var allValues = new List<KeyValuePair<Interval<int>, string>>();
				for (int i = 0; i < count; ++i)
				{
					var interval = Interval.Create(i, i + spacing);
					allValues.Add(new KeyValuePair<Interval<int>, string>(interval, i.ToString()));
				}
				values.PutMany(allValues);

				for (int i = overlap; i < count; ++i)
				{
					var intersectingValues = values.GetValues(i).ToList();
					intersectingValues.Should().HaveCount(overlap);
					for (int n = 0; n < overlap; ++n)
					{
						intersectingValues[n].Should().Be((i + n - spacing).ToString());
					}
				}
			}
		}
	}
}
