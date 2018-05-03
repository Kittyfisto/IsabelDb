using FluentAssertions;
using IsabelDb.TypeModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace IsabelDb.Test.TypeModels
{
	[TestFixture]
	public sealed class TypeModelTest
	{
		public static IEnumerable<Type> NonPolymorphicTypes => new[]
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
			typeof(byte[])
		};

		private static TypeModel Roundtrip(TypeModel model, IEnumerable<Type> availableTypes)
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			TypeModel.CreateTable(connection);
			model.Write(connection);

			var typeRegistry = new TypeRegistry(availableTypes);
			model = TypeModel.Read(connection, typeRegistry);
			return model;
		}

		private static TypeModel Roundtrip(TypeModel model)
		{
			return Roundtrip(model, model);
		}

		[Test]
		[Description("")]
		public void TestChangeBaseClass()
		{
			var typeModel = TypeModel.Create(new[] { typeof(Entities.V1.Plane) });
			new Action(() => Roundtrip(typeModel, new[] { typeof(Entities.V2.Plane), typeof(Entities.V2.Thing) }))
				.Should().Throw<BreakingChangeException>("because V2.Plane has changed its base class from Thing to object and changing base classes is a breaking change")
				.WithMessage("The base class of the type 'IsabelDb.Test.Entities.Plane' has been changed from 'IsabelDb.Test.Entities.Thing' to 'System.Object': This is a breaking change!");
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var model = Roundtrip(TypeModel.Create(new Type[0]));
			model.Should().BeEmpty();
		}

		[Test]
		public void TestRoundtripObject()
		{
			var model = Roundtrip(TypeModel.Create(new[] { typeof(object) }));
			model.IsTypeRegistered(typeof(object)).Should().BeTrue();
			model.GetTypeId(typeof(object)).Should().BeGreaterThan(0);
			model.GetTypeName(typeof(object)).Should().Be("System.Object");
			model.GetType(model.GetTypeId(typeof(object))).Should().Be<object>();
		}

		[Test]
		public void TestRoundtripInterface()
		{
			var model = TypeModel.Create(new[] { typeof(IPolymorphicCustomKey) });
			var description = model.GetTypeDescription(typeof(IPolymorphicCustomKey));
			description.BaseType.Should().NotBeNull();
			description.BaseType.Type.Should().Be<object>();

			model = Roundtrip(model);
			description = model.GetTypeDescription(typeof(IPolymorphicCustomKey));
			description.BaseType.Should().NotBeNull();
			description.BaseType.Type.Should().Be<object>();
		}

		[Test]
		public void TestAddTypeAfterRoundtrip()
		{
			var model = Roundtrip(TypeModel.Create(new[] { typeof(KeyA) }));
			model.IsTypeRegistered(typeof(KeyA)).Should().BeTrue();

			model.AddType(typeof(KeyB));
			model.IsTypeRegistered(typeof(KeyB)).Should().BeTrue();
			model.GetTypeId(typeof(KeyB)).Should().BeGreaterThan(0);
			model.GetTypeId(typeof(KeyB)).Should().NotBe(model.GetTypeId(typeof(KeyA)));
		}

		[Test]
		public void TestBuiltInTypes([ValueSource(nameof(NonPolymorphicTypes))] Type type)
		{
			var model = TypeModel.Create(new[] { type });
			var description = model[type];
			description.BaseType.Should().BeNull("because protobuf doesn't allow built-in types to inherit from anything");
		}

		[Test]
		public void TestObject()
		{
			var model = TypeModel.Create(new[] { typeof(object) });
			model.IsTypeRegistered(typeof(object)).Should().BeTrue();
			model.GetTypeId(typeof(object)).Should().BeGreaterThan(0);
			model.GetTypeName(typeof(object)).Should().Be("System.Object");
			model.GetTypeDescription(typeof(object)).Type.Should().Be<object>();
			model.GetTypeDescription(typeof(object)).TypeId.Should().Be(model.GetTypeId(typeof(object)));
			model.GetType(model.GetTypeId(typeof(object))).Should().Be<object>();
		}

		[Test]
		public void TestRegisterClass()
		{
			var model = TypeModel.Create(new[] { typeof(KeyA) });

			model.IsTypeRegistered(typeof(object)).Should().BeTrue("because base classes should automatically be registered");
			model.GetTypeId(typeof(object)).Should().BeGreaterThan(0, "because protobuf requires that ids be greater than 0");

			model.IsTypeRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue("because serializabke interfaces should automatically be registered");
			model.GetTypeId(typeof(IPolymorphicCustomKey)).Should().BeGreaterThan(model.GetTypeId(typeof(object)), "because protobuf requires that ids be greater than 0");

			model.IsTypeRegistered(typeof(KeyA)).Should().BeTrue("because we've explicitly registered that type");
			model.GetTypeId(typeof(KeyA)).Should().BeGreaterThan(model.GetTypeId(typeof(IPolymorphicCustomKey)));
		}

		[Test]
		public void TestTwoTypes()
		{
			var model = TypeModel.Create(new[] { typeof(KeyA), typeof(KeyB) });
			var idA = model.GetTypeId(typeof(KeyA));
			idA.Should().BeGreaterThan(0);

			var idB = model.GetTypeId(typeof(KeyB));
			idB.Should().BeGreaterThan(0);

			idA.Should().NotBe(idB);
		}

		[Test]
		public void TestPolymorphicKey()
		{
			var model = TypeModel.Create(new[] { typeof(KeyA) });
			model.IsTypeRegistered(typeof(KeyA)).Should().BeTrue();
			model.IsTypeRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue();
			model.IsTypeRegistered(typeof(object)).Should().BeTrue();
		}

		[Test]
		public void TestCreateEmpty()
		{
			var model = TypeModel.Create(new Type[0]);
			model.Should().BeEmpty();
			model.IsTypeRegistered(typeof(IPolymorphicCustomKey)).Should().BeFalse();
		}
	}
}
