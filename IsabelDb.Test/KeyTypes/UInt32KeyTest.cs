using System.Collections.Generic;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class UInt32KeyTest
		: AbstractIsabelDbKeyTest<uint>
	{
		protected override uint SomeKey => 0;

		protected override uint DifferentKey => uint.MaxValue;

		protected override IReadOnlyList<uint> ManyKeys => new uint[]
		{
			0, int.MaxValue, uint.MaxValue
		};
	}
}