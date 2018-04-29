using System.Collections.Generic;

namespace IsabelDb.Test.KeyTypes
{
	public sealed class PolymorphicKeyTest
		: AbstractIsabelDbKeyTest<IPolymorphicCustomKey>
	{
		protected override IPolymorphicCustomKey SomeKey => new KeyA {Value = "42"};

		protected override IPolymorphicCustomKey DifferentKey => new KeyB {Value = "9001"};

		protected override IReadOnlyList<IPolymorphicCustomKey> ManyKeys => new IPolymorphicCustomKey[]
		{
			// This test data has been chosen to verify that the database
			// treats two keys as equal only if they have the same bit pattern
			// AND are of the same type: Two keys with the same bit pattern
			// but of different types may obviously not be equal!
			new KeyA {Value = null},
			new KeyB {Value = null},

			new KeyA {Value = "a"},
			new KeyB {Value = "b"},

			new KeyA {Value = "PSY"},
			new KeyB {Value = "PSY"}
		};
	}
}