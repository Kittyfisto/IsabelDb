using System;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbUInt16KeyTest
		: AbstractIsabelDbKeyTest<ushort>
	{
		protected override UInt16 SomeKey => ushort.MinValue;

		protected override UInt16 DifferentKey => ushort.MaxValue;
	}
}