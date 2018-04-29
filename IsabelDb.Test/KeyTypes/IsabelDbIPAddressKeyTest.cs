using System.Collections.Generic;
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

		protected override IReadOnlyList<IPAddress> ManyKeys => new[]
		{
			IPAddress.Any,
			IPAddress.Broadcast,
			IPAddress.IPv6Any,
			IPAddress.IPv6Loopback,
			IPAddress.Loopback,
			IPAddress.Parse("192.168.0.1"),
			IPAddress.Parse("1.1.1.1")
		};
	}
}