using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbInt16KeyTest
		: AbstractIsabelDbKeyTest<short>
	{
		protected override Int16 SomeKey => short.MinValue;

		protected override Int16 DifferentKey => short.MaxValue;

		protected override IReadOnlyList<short> ManyKeys => new short[] {short.MinValue, -1, 0, 1, short.MaxValue};
	}
}