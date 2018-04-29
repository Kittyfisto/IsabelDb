using System;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbInt32KeyTest
		: AbstractIsabelDbKeyTest<Int32>
	{
		protected override Int32 SomeKey => int.MinValue;

		protected override Int32 DifferentKey => int.MaxValue;
	}
}