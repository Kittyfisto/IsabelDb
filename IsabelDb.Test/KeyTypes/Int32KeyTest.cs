using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class Int32KeyTest
		: AbstractIsabelDbKeyTest<int>
	{
		protected override int SomeKey => int.MinValue;

		protected override int DifferentKey => int.MaxValue;

		protected override IReadOnlyList<int> ManyKeys => new[] {int.MinValue, -1, 0, 1, int.MaxValue};
	}
}