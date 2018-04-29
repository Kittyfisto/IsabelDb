using System;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbInt64KeyTest
		: AbstractIsabelDbKeyTest<Int64>
	{
		protected override Int64 SomeKey => Int64.MinValue;

		protected override Int64 DifferentKey => Int64.MaxValue;
	}
}