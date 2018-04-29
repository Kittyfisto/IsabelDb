using System;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbInt16KeyTest
		: AbstractIsabelDbKeyTest<short>
	{
		protected override Int16 SomeKey => short.MinValue;

		protected override Int16 DifferentKey => short.MaxValue;
	}
}