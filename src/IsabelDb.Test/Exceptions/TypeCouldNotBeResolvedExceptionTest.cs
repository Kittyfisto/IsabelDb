using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class TypeCouldNotBeResolvedExceptionTest
		: AbstractExceptionTest<TypeCouldNotBeResolvedException>
	{
		#region Overrides of AbstractExceptionTest<TypeCouldNotBeResolvedException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new TypeCouldNotBeResolvedException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}