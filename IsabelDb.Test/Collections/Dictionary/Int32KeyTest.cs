using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class Int32KeyTest
		: AbstractDictionaryObjectStoreTest<int>
	{
		protected override IEnumerable<Type> CustomTypes => new Type[0];

		protected override int SomeKey => int.MinValue;

		protected override int DifferentKey => int.MaxValue;

		protected override IReadOnlyList<int> ManyKeys => new[] {int.MinValue, -1, 0, 1, int.MaxValue};
	}
}