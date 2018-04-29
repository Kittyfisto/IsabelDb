using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbCustomKeyTest
		: AbstractIsabelDbKeyTest<CustomKey>
	{
		protected override CustomKey SomeKey => new CustomKey{A = 1, B = 2, C = 3, D = 4};
		protected override CustomKey DifferentKey => new CustomKey{A = int.MinValue, B = int.MaxValue, C = int.MinValue, D = int.MaxValue};
	}
}