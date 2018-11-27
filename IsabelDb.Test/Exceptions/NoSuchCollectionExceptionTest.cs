using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class NoSuchCollectionExceptionTest
		: AbstractExceptionTest<NoSuchCollectionException>
	{
		#region Overrides of AbstractExceptionTest<NoSuchCollectionException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new NoSuchCollectionException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}