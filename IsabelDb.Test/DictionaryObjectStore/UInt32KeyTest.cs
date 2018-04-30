using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.DictionaryObjectStore
{
	[TestFixture]
	public sealed class UInt32KeyTest
		: AbstractDictionaryObjectStoreTest<uint>
	{
		protected override IEnumerable<Type> CustomTypes => new Type[0];

		protected override uint SomeKey => 0;

		protected override uint DifferentKey => uint.MaxValue;

		protected override IReadOnlyList<uint> ManyKeys => new uint[]
		{
			0, int.MaxValue, uint.MaxValue
		};
	}
}