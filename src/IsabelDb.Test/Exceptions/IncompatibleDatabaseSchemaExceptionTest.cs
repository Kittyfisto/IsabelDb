using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	[TestFixture]
	public sealed class IncompatibleDatabaseSchemaExceptionTest
		: AbstractExceptionTest<IncompatibleDatabaseSchemaException>
	{
		#region Overrides of AbstractExceptionTest<IncompatibleDatabaseSchemaException>

		public override void TestRoundtrip()
		{
			var message = "Some exception message";
			var exception = new IncompatibleDatabaseSchemaException(message);
			var actualException = Roundtrip(exception);
			actualException.Message.Should().Be(message);
		}

		#endregion
	}
}
