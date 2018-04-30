using System;
using FluentAssertions;
using NUnit.Framework;
using System.Data.SQLite;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class TypeStoreTest
	{
		private SQLiteConnection _connection;
		private TypeRegistry _typeResolver;

		[SetUp]
		public void Setup()
		{
			_connection = new SQLiteConnection("Data Source=:memory:");
			_connection.Open();

			TypeStore.CreateTable(_connection);

			_typeResolver = new TypeRegistry(new Type[0]);
		}

		[Test]
		[Defect("https://github.com/Kittyfisto/IsabelDb/issues/1")]
		[Description("Verifies that the type store tolerates that types cannot be resolved anymore")]
		public void TestUnavailableType()
		{
			_typeResolver.Register<CustomKey>();
			_typeResolver.Register<IPolymorphicCustomKey>();

			var store = new TypeStore(_connection, _typeResolver);
			var customKeyId = store.GetOrCreateTypeId(typeof(CustomKey));
			var polymorphicKeyId = store.GetOrCreateTypeId(typeof(IPolymorphicCustomKey));

			_typeResolver = new TypeRegistry(new Type[0]);
			_typeResolver.Register<IPolymorphicCustomKey>();
			store = new TypeStore(_connection, _typeResolver);
			store.GetTypeFromTypeId(customKeyId).Should().BeNull("because the type couldn't be resolved anymore");
			store.GetTypeFromTypeId(polymorphicKeyId).Should().Be<IPolymorphicCustomKey>(
				"because types which can be resolved should still be presented");
		}
	}
}
