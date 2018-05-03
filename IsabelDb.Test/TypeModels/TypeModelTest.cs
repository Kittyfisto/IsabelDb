using FluentAssertions;
using IsabelDb.TypeModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using IsabelDb.Test.Entities;

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

		public static IEnumerable<Type> Types => NonPolymorphicTypes.Concat(new[]
		{
			typeof(CustomKey),
			typeof(IPolymorphicCustomKey),
			typeof(KeyA),
			typeof(KeyB)
		});

		private static TypeModel Roundtrip(TypeModel model, IEnumerable<Type> availableTypes)
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			TypeModel.CreateTable(connection);
			model.Write(connection);

			var typeRegistry = new TypeResolver(availableTypes);
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
		public void TestRoundtrip([ValueSource(nameof(Types))] Type type)
		{
			var model = TypeModel.Create(new[] {type});
			var description = model.GetTypeDescription(type);

			var actualModel = Roundtrip(model);
			var actualDescription = actualModel.GetTypeDescription(type);
			const string reason = "because the type description should contain equal values upon being deserialized again";
			actualDescription.Should().NotBeNull(reason);
			actualDescription.Should().NotBeSameAs(description, reason);
			actualDescription.Name.Should().Be(description.Name, reason);
			actualDescription.Namespace.Should().Be(description.Namespace, reason);
			actualDescription.FullTypeName.Should().Be(description.FullTypeName, reason);
			actualDescription.Type.Should().Be(description.Type, reason);
			actualDescription.TypeId.Should().Be(description.TypeId, reason);
			actualDescription.Members.Should().HaveCount(description.Members.Count, reason);
			for (int i = 0; i < actualDescription.Members.Count; ++i)
			{
				actualDescription.Members[i].Name.Should().Be(description.Members[i].Name);
				actualDescription.Members[i].MemberId.Should().Be(description.Members[i].MemberId);
			}

			const string reason2 = "because the deserialized type model should still be able to resolve types by their name / id";
			actualModel.GetTypeName(type).Should().Be(description.FullTypeName, reason2);
			actualModel.GetType(description.TypeId).Should().Be(type, reason2);
			actualModel.GetTypeId(type).Should().Be(description.TypeId, reason2);
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

			var description = model.GetTypeDescription(typeof(object));
			description.Members.Should().BeEmpty();
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
		public void TestKeyA()
		{
			var model = TypeModel.Create(new[] { typeof(KeyA) });
			var description = model.GetTypeDescription(typeof(KeyA));
			description.Members.Should().HaveCount(1);
			var field = description.Members[0];
			field.Name.Should().Be(nameof(KeyA.Value));
		}

		[Test]
		public void TestAnimalArray()
		{
			var model = TypeModel.Create(new[] { typeof(Numbers) });
			model.IsTypeRegistered(typeof(Numbers)).Should().BeTrue();
			model.IsTypeRegistered(typeof(Animal)).Should().BeTrue("because Numbers has a field of type Animal[] and thus it's type should've been registered as well!");
		}

		[Test]
		public void TestBoolProperty()
		{
			var model = TypeModel.Create(new[] { typeof(Menu) });
			var description = model.GetTypeDescription(typeof(Menu));
			description.Members.Should().HaveCount(1);
			description.Members[0].Name.Should().Be("IsVisible");
			description.Members[0].TypeDescription.FullTypeName.Should().Be("System.Boolean");
			description.Members[0].MemberId.Should().BeGreaterThan(0);
		}

		[Test]
		public void TestTwoFields()
		{
			var model = TypeModel.Create(new[] { typeof(Point) });
			var description = model.GetTypeDescription(typeof(Point));
			description.Members.Should().HaveCount(2);
			description.Members[0].Name.Should().Be("X");
			description.Members[0].TypeDescription.FullTypeName.Should().Be("System.Double");
			description.Members[0].MemberId.Should().BeGreaterThan(0);

			description.Members[1].Name.Should().Be("Y");
			description.Members[1].TypeDescription.FullTypeName.Should().Be("System.Double");
			description.Members[1].MemberId.Should().BeGreaterThan(0);
			description.Members[1].MemberId.Should().NotBe(description.Members[0].MemberId);
		}

		[Test]
		public void TestFieldsAndInheritance()
		{
			var model = TypeModel.Create(new[] { typeof(Dog) });
			var dogDescription = model.GetTypeDescription(typeof(Dog));
			dogDescription.Members.Should().HaveCount(2, "because Dog declares one property and one field");
			dogDescription.Members[0].Name.Should().Be(nameof(Dog.EyeColor));
			dogDescription.Members[1].Name.Should().Be(nameof(Dog.FurColor));

			var animalDescription = dogDescription.BaseType;
			animalDescription.Members.Should().HaveCount(2, "because Animal declares two properties");
			animalDescription.Members[0].Name.Should().Be(nameof(Animal.Name));
			animalDescription.Members[1].Name.Should().Be(nameof(Animal.Age));
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
