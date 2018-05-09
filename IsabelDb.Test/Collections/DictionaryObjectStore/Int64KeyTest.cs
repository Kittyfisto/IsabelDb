using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.DictionaryObjectStore
{
	[TestFixture]
	public sealed class Int64KeyTest
		: AbstractDictionaryObjectStoreTest<long>
	{
		protected override IEnumerable<Type> CustomTypes => new Type[0];

		protected override long SomeKey => long.MinValue;

		protected override long DifferentKey => long.MaxValue;

		protected override IReadOnlyList<long> ManyKeys => new[] {long.MinValue, -1, 0, 1, long.MaxValue};
	}
}