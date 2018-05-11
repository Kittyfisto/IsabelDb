using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class UInt64KeyTest
		: AbstractDictionaryObjectStoreTest<ulong>
	{
		protected override IEnumerable<Type> CustomTypes => new Type[0];

		protected override ulong SomeKey => ulong.MinValue;

		protected override ulong DifferentKey => ulong.MaxValue;

		protected override IReadOnlyList<ulong> ManyKeys => new ulong[] { ulong.MinValue, 1, long.MaxValue, ulong.MaxValue };
	}
}