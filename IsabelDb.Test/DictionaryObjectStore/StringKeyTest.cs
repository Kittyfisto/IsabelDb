using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.DictionaryObjectStore
{
	[TestFixture]
	public sealed class StringKeyTest
		: AbstractDictionaryObjectStoreTest<string>
	{
		protected override string SomeKey => "Foo";

		protected override string DifferentKey => "Bar";

		protected override IReadOnlyList<string> ManyKeys => new[]
		{
			" ",
			"	", "a", "A", "Hello, World!", "휘파람",
			"1",
			"العَرَبِيَّة",
			"日本語"
		};
	}
}