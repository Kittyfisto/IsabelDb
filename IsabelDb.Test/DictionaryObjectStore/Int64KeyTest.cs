using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.DictionaryObjectStore
{
	[TestFixture]
	public sealed class Int64KeyTest
		: AbstractDictionaryObjectStoreTest<long>
	{
		protected override long SomeKey => long.MinValue;

		protected override long DifferentKey => long.MaxValue;

		protected override IReadOnlyList<long> ManyKeys => new[] {long.MinValue, -1, 0, 1, long.MaxValue};
	}
}