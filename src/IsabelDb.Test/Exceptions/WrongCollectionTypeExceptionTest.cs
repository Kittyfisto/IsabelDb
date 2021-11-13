using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class WrongCollectionTypeExceptionTest
		: AbstractExceptionTest<WrongCollectionTypeException>
	{
		#region Overrides of AbstractExceptionTest<WrongCollectionTypeException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new WrongCollectionTypeException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}