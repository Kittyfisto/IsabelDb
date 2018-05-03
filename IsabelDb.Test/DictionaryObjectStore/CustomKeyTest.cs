using System;
using System.Collections.Generic;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.DictionaryObjectStore
{
	[TestFixture]
	public sealed class CustomKeyTest
		: AbstractDictionaryObjectStoreTest<CustomKey>
	{
		protected override IEnumerable<Type> CustomTypes => new []{typeof(CustomKey)};

		protected override CustomKey SomeKey => new CustomKey {A = 1, B = 2, C = 3, D = 4};

		protected override CustomKey DifferentKey =>
			new CustomKey {A = int.MinValue, B = int.MaxValue, C = int.MinValue, D = int.MaxValue};

		protected override IReadOnlyList<CustomKey> ManyKeys => new[] {SomeKey, DifferentKey};
	}
}