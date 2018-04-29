using System.Net;
using NUnit.Framework;

namespace IsabelDb.Test.KeyTypes
{
	[TestFixture]
	public sealed class IsabelDbIpAddressKeyTest
		: AbstractIsabelDbKeyTest<IPAddress>
	{
		protected override IPAddress SomeKey => IPAddress.Loopback;

		protected override IPAddress DifferentKey => IPAddress.IPv6Loopback;
	}
}