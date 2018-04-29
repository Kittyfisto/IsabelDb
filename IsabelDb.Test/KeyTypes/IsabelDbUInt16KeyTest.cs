using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbUInt16KeyTest
		: AbstractIsabelDbKeyTest<ushort>
	{
		protected override UInt16 SomeKey => ushort.MinValue;

		protected override UInt16 DifferentKey => ushort.MaxValue;

		protected override IReadOnlyList<ushort> ManyKeys => new ushort[]
		{
			ushort.MinValue, 32000, ushort.MaxValue
		};
	}
}