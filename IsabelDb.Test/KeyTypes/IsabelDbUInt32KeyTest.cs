using System;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbUInt32KeyTest
		: AbstractIsabelDbKeyTest<UInt32>
	{
		protected override UInt32 SomeKey => 0;

		protected override UInt32 DifferentKey => UInt32.MaxValue;
	}
}