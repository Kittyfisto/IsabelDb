using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class CollectionNameAlreadyInUseExceptionTest
		: AbstractExceptionTest<CollectionNameAlreadyInUseException>
	{
		#region Overrides of AbstractExceptionTest<CollectionNameAlreadyInUseException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new CollectionNameAlreadyInUseException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}
