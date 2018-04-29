using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	public sealed class PolymorphicKeyTest
		: AbstractIsabelDbKeyTest<IPolymorphicCustomKey>
	{
		#region Overrides of AbstractIsabelDbKeyTest<IPolymorphicCustomKey>

		protected override IPolymorphicCustomKey SomeKey => new KeyA{Value = "42"};

		protected override IPolymorphicCustomKey DifferentKey => new KeyB {Value = "9001"};

		protected override IReadOnlyList<IPolymorphicCustomKey> ManyKeys => new IPolymorphicCustomKey[]
		{
			new KeyA {Value = null},
			new KeyB {Value = null},

			new KeyA {Value = "a"},
			new KeyB {Value = "b"},

			new KeyA {Value = "PSY"},
			new KeyB {Value = "PSY"}
		};

		#endregion
	}

	[TestFixture]
	public sealed class CustomKeyTest
		: AbstractIsabelDbKeyTest<CustomKey>
	{
		protected override CustomKey SomeKey => new CustomKey{A = 1, B = 2, C = 3, D = 4};
		protected override CustomKey DifferentKey => new CustomKey{A = int.MinValue, B = int.MaxValue, C = int.MinValue, D = int.MaxValue};
		protected override IReadOnlyList<CustomKey> ManyKeys => new[] {SomeKey, DifferentKey};
	}
}