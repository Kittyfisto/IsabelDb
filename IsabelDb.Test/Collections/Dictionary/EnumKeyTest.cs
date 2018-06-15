using System;
using System.Collections.Generic;
using IsabelDb.Test.Entities;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	[Ignore("Not yet implemented")]
	public sealed class EnumKeyTest
		: AbstractDictionaryObjectStoreTest<SomeEnum>
	{
		protected override IEnumerable<Type> CustomTypes => new []{typeof(SomeEnum)};

		protected override SomeEnum SomeKey => SomeEnum.A;

		protected override SomeEnum DifferentKey => SomeEnum.B;

		protected override IReadOnlyList<SomeEnum> ManyKeys => new SomeEnum[] { SomeEnum.A, SomeEnum.B, SomeEnum.C };
	}
}