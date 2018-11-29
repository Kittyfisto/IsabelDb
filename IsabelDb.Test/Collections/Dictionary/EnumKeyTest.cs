using System;
using System.Collections.Generic;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	//[Ignore("Not yet implemented")]
	public sealed class EnumKeyTest
		: AbstractDictionaryObjectStoreTest<Int32Enum>
	{
		protected override IEnumerable<Type> CustomTypes => new []{typeof(Int32Enum)};

		protected override Int32Enum SomeKey => Int32Enum.A;

		protected override Int32Enum DifferentKey => Int32Enum.B;

		protected override IReadOnlyList<Int32Enum> ManyKeys => new Int32Enum[] { Int32Enum.A, Int32Enum.B, Int32Enum.C };
	}
}