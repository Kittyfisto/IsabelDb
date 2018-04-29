using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.DictionaryObjectStore
{
	[TestFixture]
	public sealed class Int16KeyTest
		: AbstractDictionaryObjectStoreTest<short>
	{
		protected override short SomeKey => short.MinValue;

		protected override short DifferentKey => short.MaxValue;

		protected override IReadOnlyList<short> ManyKeys => new short[] {short.MinValue, -1, 0, 1, short.MaxValue};
	}
}