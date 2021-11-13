using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Exceptions
{
	public abstract class AbstractExceptionTest<T>
		where T : Exception
	{
		[Test]
		public void TestDefaultCtor()
		{
			var ctor = typeof(T).GetConstructor(new Type[0]);
			if (ctor == null)
			{
				Assert.Fail("The exception '{0}' is expected to have a default ctor (one without arguments)",
				            typeof(T).FullName);
			}
		}

		[Test]
		[Description("Ensures that the exception lies inside the IsabelDb namespace")]
		public void TestNamespace()
		{
			var type = typeof(T);
			type.Namespace.Should().Be("IsabelDb");
		}

		[Test]
		[Description("Ensures that the exception can roundtrip with a BinaryFormatter")]
		public abstract void TestRoundtrip();

		/// <summary>
		///     Performs a serialization roundtrip using <see cref="BinaryFormatter" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <returns></returns>
		public static T Roundtrip(T that)
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, that);

				stream.Position = 0;
				var exception = formatter.Deserialize(stream);
				exception.Should().BeOfType<T>();
				return (T) exception;
			}
		}
	}
}
