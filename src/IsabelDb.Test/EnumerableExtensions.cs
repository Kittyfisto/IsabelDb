using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace IsabelDb.Test
{
	public static class EnumerableExtensions
	{
		public static void ShouldBeUnique(this IEnumerable<RowId> ids)
		{
			foreach (var id in ids)
			{
				ids.Count(x => Equals(x, id)).Should().Be(1, "because every returned row id should be unique");
			}
		}
	}
}