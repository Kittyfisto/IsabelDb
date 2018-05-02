using System;
using NUnit.Framework;
using System.Data.SQLite;
using System.Net;
using FluentAssertions;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class TypeModelTest
	{
		private SQLiteConnection _connection;

		[SetUp]
		public void Setup()
		{
			_connection = new SQLiteConnection("Data Source=:memory:");
			_connection.Open();

			Serializer.CreateTable(_connection);
		}

		[Test]
		[Description("Verifies that the type model is aware of types which are natively supported by protobuf, even if they aren't specified explicitly")]
		public void TestBuiltIntTypes()
		{
			var typeModel = TypeModel.Create(_connection, new Type[0]);
			var expectedTypes = new[]
			{
				typeof(string),
				typeof(float),
				typeof(double),
				typeof(byte),
				typeof(sbyte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(object),
				typeof(IPAddress),
				typeof(byte[])
			};
			foreach (var type in expectedTypes)
			{
				typeModel.IsRegistered(type).Should().BeTrue("because we expect the type model to intrinsically know this type");
				typeModel.GetTypeId(type).Should().BeGreaterThan(0, "because we expect the type model to intrinsically know this type");
			}
		}

		[Test]
		public void TestRegisterCustomType()
		{
			var typeModel = TypeModel.Create(_connection, new []{typeof(IPolymorphicCustomKey)});
			typeModel.IsRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue();
			var id = typeModel.GetTypeId(typeof(IPolymorphicCustomKey));
			typeModel.GetType(id).Should().Be<IPolymorphicCustomKey>();
		}

		[Test]
		[Defect("https://github.com/Kittyfisto/IsabelDb/issues/1")]
		[Description("Verifies that the type store tolerates that types cannot be resolved anymore")]
		public void TestUnavailableType()
		{
			var typeModel = TypeModel.Create(_connection, new []{typeof(CustomKey), typeof(IPolymorphicCustomKey)});
			var customKeyId = typeModel.GetTypeId(typeof(CustomKey));
			var polymorphicKeyId = typeModel.GetTypeId(typeof(IPolymorphicCustomKey));

			typeModel = TypeModel.Create(_connection, new []{typeof(IPolymorphicCustomKey)});
			typeModel.GetType(customKeyId).Should().BeNull("because the type couldn't be resolved anymore");
			typeModel.GetType(polymorphicKeyId).Should().Be<IPolymorphicCustomKey>(
				"because types which can be resolved should still be presented");
		}
	}
}
