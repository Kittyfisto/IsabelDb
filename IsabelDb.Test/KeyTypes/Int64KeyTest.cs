using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class Int64KeyTest
		: AbstractIsabelDbKeyTest<Int64>
	{
		protected override Int64 SomeKey => Int64.MinValue;

		protected override Int64 DifferentKey => Int64.MaxValue;

		protected override IReadOnlyList<long> ManyKeys => new long[] {long.MinValue, -1, 0, 1, long.MaxValue};
	}
}