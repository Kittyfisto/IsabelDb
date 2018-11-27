using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class BreakingChangeExceptionTest
		: AbstractExceptionTest<BreakingChangeException>
	{
		#region Overrides of AbstractExceptionTest<BreakingChangeException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new BreakingChangeException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}