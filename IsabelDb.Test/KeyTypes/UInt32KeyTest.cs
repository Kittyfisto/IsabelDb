using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class UInt32KeyTest
		: AbstractIsabelDbKeyTest<UInt32>
	{
		protected override UInt32 SomeKey => 0;

		protected override UInt32 DifferentKey => UInt32.MaxValue;

		protected override IReadOnlyList<uint> ManyKeys => new uint[]
		{
			0, int.MaxValue, uint.MaxValue
		};
	}
}