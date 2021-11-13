using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class TypeNotRegisteredExceptionTest
		: AbstractExceptionTest<TypeNotRegisteredException>
	{
		#region Overrides of AbstractExceptionTest<TypeNotRegisteredException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new TypeNotRegisteredException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}