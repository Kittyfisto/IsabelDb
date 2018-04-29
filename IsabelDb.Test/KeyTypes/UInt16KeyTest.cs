using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class UInt16KeyTest
		: AbstractIsabelDbKeyTest<ushort>
	{
		protected override ushort SomeKey => ushort.MinValue;

		protected override ushort DifferentKey => ushort.MaxValue;

		protected override IReadOnlyList<ushort> ManyKeys => new ushort[]
		{
			ushort.MinValue, 32000, ushort.MaxValue
		};
	}
}