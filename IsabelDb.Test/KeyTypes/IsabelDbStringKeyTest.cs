using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbStringKeyTest
		: AbstractIsabelDbKeyTest<string>
	{
		protected override string SomeKey => "Foo";

		protected override string DifferentKey => "Bar";
	}
}