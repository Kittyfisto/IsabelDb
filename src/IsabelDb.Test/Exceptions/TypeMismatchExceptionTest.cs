using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class TypeMismatchExceptionTest
		: AbstractExceptionTest<TypeMismatchException>
	{
		#region Overrides of AbstractExceptionTest<TypeMismatchException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new TypeMismatchException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}