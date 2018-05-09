using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.DictionaryObjectStore
{
	[TestFixture]
	public sealed class UInt16KeyTest
		: AbstractDictionaryObjectStoreTest<ushort>
	{
		protected override IEnumerable<Type> CustomTypes => new Type[0];

		protected override ushort SomeKey => ushort.MinValue;

		protected override ushort DifferentKey => ushort.MaxValue;

		protected override IReadOnlyList<ushort> ManyKeys => new ushort[]
		{
			ushort.MinValue, 32000, ushort.MaxValue
		};
	}
}