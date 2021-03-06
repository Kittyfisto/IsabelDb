﻿using FluentAssertions;
using IsabelDb.TypeModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using IsabelDb.Test.Entities;
using IsabelDb.Test.Entities.V2;
using IsabelDb.TypeModels.Surrogates;
using Cpu = IsabelDb.Test.Entities.V1.Cpu;

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
			typeof(ulong),
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
			var otherModel = TypeModel.Read(connection, typeRegistry);
			otherModel.Add(TypeModel.Create(availableTypes));

			EnsureReferencialIntegrity(otherModel);

			return otherModel;
		}

		private static void EnsureReferencialIntegrity(TypeModel typeModel)
		{
			var descriptions = new HashSet<TypeDescription>(typeModel.TypeDescriptions);
			foreach (var description in descriptions)
			{
				const string reason = "because type descriptions should reference only objects of the same type model";
				if (description.BaseType != null)
					descriptions.Should().Contain(description.BaseType, reason);
				if (description.SurrogateType != null)
					descriptions.Should().Contain(description.SurrogateType, reason);
				if (description.SurrogatedType != null)
					descriptions.Should().Contain(description.SurrogatedType, reason);

				typeModel.GetTypeDescription(description.TypeId).Should().BeSameAs(description, reason);
				typeModel.GetTypeDescription(description.ResolvedType).Should().BeSameAs(description);
			}
		}

		private static TypeModel Roundtrip(TypeModel model)
		{
			return Roundtrip(model, model.Types);
		}

		[Test]
		public void TestCreateTable()
		{
			using (var connection = new SQLiteConnection("Data Source=:memory:"))
			{
				connection.Open();
				TypeModel.DoesTypeTableExist(connection).Should().BeFalse();
				TypeModel.CreateTable(connection);
				TypeModel.DoesTypeTableExist(connection).Should().BeTrue();
			}
		}

		[Test]
		[Description("Verifies that for well known surrogate types, it doesn't matter if the type or its surrogate is registered, the type model ends up identical")]
		public void TestCreateFromVersion()
		{
			var typeModel1 = TypeModel.Create(new[] {typeof(Version)});
			var typeModel2 = TypeModel.Create(new[] {typeof(VersionSurrogate)});

			typeModel1.Types.Should().BeEquivalentTo(typeModel2.Types, "because the type model should be aware of type surrogates for well known types and add them itself");
		}

		[Test]
		public void TestRegisterByte()
		{
			var typeModel = TypeModel.Create(new[] {typeof(byte)});
			var description = typeModel.GetTypeDescription<byte>();
			description.Classification.Should().Be(TypeClassification.Struct);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().BeNull("because at least for now, integer types cannot inherit from System.Value type due to a protobuf constraint");
		}

		[Test]
		public void TestRoundtripByte()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(byte)}));
			var description = typeModel.GetTypeDescription<byte>();
			description.Classification.Should().Be(TypeClassification.Struct);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().BeNull("because at least for now, integer types cannot inherit from System.Value type due to a protobuf constraint");
		}

		[Test]
		public void TestRegisterInterface()
		{
			var typeModel = TypeModel.Create(new[] {typeof(IPolymorphicCustomKey)});
			var description = typeModel.GetTypeDescription<IPolymorphicCustomKey>();
			description.Classification.Should().Be(TypeClassification.Interface);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().NotBeNull();
			var baseTypeDescription = description.BaseType;
			baseTypeDescription.ResolvedType.Should().Be<object>();
		}

		[Test]
		public void TestRoundtripInterface()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(IPolymorphicCustomKey)}));
			var description = typeModel.GetTypeDescription<IPolymorphicCustomKey>();
			description.Classification.Should().Be(TypeClassification.Interface);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().NotBeNull();
			var baseTypeDescription = description.BaseType;
			baseTypeDescription.ResolvedType.Should().Be<object>();
		}

		[Test]
		public void TestRegisterClass()
		{
			var typeModel = TypeModel.Create(new[] {typeof(SomeClass)});
			var description = typeModel.GetTypeDescription<SomeClass>();
			description.Classification.Should().Be(TypeClassification.Class);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().NotBeNull();
			var baseTypeDescription = description.BaseType;
			baseTypeDescription.ResolvedType.Should().Be<object>();
		}

		[Test]
		public void TestRoundripClass()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(SomeClass)}));
			var description = typeModel.GetTypeDescription<SomeClass>();
			description.Classification.Should().Be(TypeClassification.Class);
			description.SurrogateType.Should().BeNull();
			description.UnderlyingEnumTypeDescription.Should().BeNull();
			description.BaseType.Should().NotBeNull();
			var baseTypeDescription = description.BaseType;
			baseTypeDescription.ResolvedType.Should().Be<object>();
		}

		[Test]
		public void TestRegisterStruct()
		{
			var typeModel = TypeModel.Create(new[] {typeof(SomeStruct)});
			var description = typeModel.GetTypeDescription<SomeStruct>();
			description.Classification.Should().Be(TypeClassification.Struct);
		}

		[Test]
		public void TestRoundtripStruct()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(SomeStruct)}));
			var description = typeModel.GetTypeDescription<SomeStruct>();
			description.Classification.Should().Be(TypeClassification.Struct);
		}

		[Test]
		public void TestRegisterEnum()
		{
			var typeModel = TypeModel.Create(new[] {typeof(Int32Enum)});
			var description = typeModel.GetTypeDescription<Int32Enum>();
			description.ResolvedType.Should().Be<Int32Enum>();
			description.Classification.Should().Be(TypeClassification.Enum);
			description.Name.Should().Be("Int32Enum");
			description.Namespace.Should().Be("IsabelDb.Test.Entities");
			description.Fields.Should().HaveCount(3, "because the enum has 3 members");
			description.Fields[0].Name.Should().Be("A");
			description.Fields[0].MemberId.Should().Be(0);
			description.Fields[1].Name.Should().Be("B");
			description.Fields[1].MemberId.Should().Be(1);
			description.Fields[2].Name.Should().Be("C");
			description.Fields[2].MemberId.Should().Be(2);

			var other = description.UnderlyingEnumTypeDescription;
			other.Should().NotBeNull();
			other.ResolvedType.Should().Be<int>();
		}

		[Test]
		public void TestRoundtripEnum()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(Int32Enum)}));
			var description = typeModel.GetTypeDescription<Int32Enum>();
			description.Classification.Should().Be(TypeClassification.Enum);
			description.Name.Should().Be("Int32Enum");
			description.Namespace.Should().Be("IsabelDb.Test.Entities");
			description.Fields.Should().HaveCount(3, "because the enum has 3 members");
			description.Fields[0].Name.Should().Be("A");
			description.Fields[0].MemberId.Should().Be(0);
			description.Fields[1].Name.Should().Be("B");
			description.Fields[1].MemberId.Should().Be(1);
			description.Fields[2].Name.Should().Be("C");
			description.Fields[2].MemberId.Should().Be(2);

			var other = description.UnderlyingEnumTypeDescription;
			other.Should().NotBeNull();
			other.ResolvedType.Should().Be<int>();
		}

		[Test]
		public void TestRegisterUInt32Enum()
		{
			new Action(() => TypeModel.Create(new[] {typeof(UInt32Enum)}))
				.Should().Throw<NotSupportedException>()
				.WithMessage("The type 'IsabelDb.Test.Entities.UInt32Enum' uses 'System.UInt32' as its underlying type, but this is unfortunately not supported.");
		}

		[Test]
		public void TestRegisterInt64Enum()
		{
			new Action(() => TypeModel.Create(new[] {typeof(Int64Enum)}))
				.Should().Throw<NotSupportedException>()
				.WithMessage("The type 'IsabelDb.Test.Entities.Int64Enum' uses 'System.Int64' as its underlying type, but this is unfortunately not supported.");
		}

		[Test]
		public void TestRegisterUInt64Enum()
		{
			new Action(() => TypeModel.Create(new[] {typeof(UInt64Enum)}))
				.Should().Throw<NotSupportedException>()
				.WithMessage("The type 'IsabelDb.Test.Entities.UInt64Enum' uses 'System.UInt64' as its underlying type, but this is unfortunately not supported.");
		}

		[Test]
		public void TestRegisterVersion()
		{
			var typeModel = TypeModel.Create(new[] {typeof(VersionSurrogate)});
			var version = typeModel.GetTypeDescription(typeof(Version));
			var surrogate = typeModel.GetTypeDescription(typeof(VersionSurrogate));

			version.Should().NotBeNull();
			version.Fields.Should().BeEmpty();
			version.SurrogateType.Should().NotBeNull("Because we've added a type which acts as a surrogate for Version");
			version.SurrogateType.Should().BeSameAs(surrogate);
			version.SurrogatedType.Should().BeNull("Because Version isn't a surrogate for anything");

			surrogate.SurrogateType.Should().BeNull();
			surrogate.SurrogatedType.Should().Be(version);
			surrogate.Fields.Should().HaveCount(4);
		}

		[Test]
		public void TestRegisterIpAddress()
		{
			var typeModel = TypeModel.Create(new[] {typeof(IPAddressSurrogate)});
			var ipAddress = typeModel.GetTypeDescription(typeof(IPAddress));
			var surrogate = typeModel.GetTypeDescription(typeof(IPAddressSurrogate));

			ipAddress.Should().NotBeNull();
			ipAddress.Fields.Should().BeEmpty();
			ipAddress.SurrogateType.Should().NotBeNull("Because we've added a type which acts as a surrogate for IPAddress");
			ipAddress.SurrogateType.Should().BeSameAs(surrogate);
			ipAddress.SurrogatedType.Should().BeNull("Because IPAddress isn't a surrogate for anything");

			surrogate.SurrogateType.Should().BeNull();
			surrogate.SurrogatedType.Should().Be(ipAddress);
			surrogate.Fields.Should().HaveCount(1);
			surrogate.Fields.First().Name.Should().Be(nameof(IPAddressSurrogate.Data));
		}

		[Test]
		public void TestRoundtripIpAddress()
		{
			var typeModel = Roundtrip(TypeModel.Create(new[] {typeof(IPAddressSurrogate)}));
			var ipAddress = typeModel.GetTypeDescription(typeof(IPAddress));
			var surrogate = typeModel.GetTypeDescription(typeof(IPAddressSurrogate));

			ipAddress.Should().NotBeNull();
			ipAddress.Fields.Should().BeEmpty();
			ipAddress.SurrogateType.Should().NotBeNull("Because we've added a type which acts as a surrogate for IPAddress");
			ipAddress.SurrogateType.Should().BeSameAs(surrogate);
			ipAddress.SurrogatedType.Should().BeNull("Because IPAddress isn't a surrogate for anything");

			surrogate.SurrogateType.Should().BeNull();
			surrogate.SurrogatedType.Should().Be(ipAddress);
			surrogate.Fields.Should().HaveCount(1);
			surrogate.Fields.First().Name.Should().Be(nameof(IPAddressSurrogate.Data));
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
		[Description("Verifies that the FullTypeName includes the names of its generic type arguments, which may be dictated via DataContract.Name")]
		public void TestGenericType1()
		{
			var typeModel = TypeModel.Create(new[] { typeof(Entities.V1.Motherboard) });

			var description = typeModel.GetTypeDescription(typeof(List<Cpu>));
			description.Name.Should().Be("List`1");
			description.Namespace.Should().Be("System.Collections.Generic");
			description.FullTypeName.Should().Be("System.Collections.Generic.List`1[IsabelDb.Test.Entities.Cpu]");
		}

		[Test]
		[Description("Verifies that generic type arguments are also added to the type model")]
		public void TestGenericType2()
		{
			var typeModel = TypeModel.Create(new[] { typeof(List<Cpu>) });

			var description = typeModel.GetTypeDescription(typeof(Cpu));
			description.Name.Should().Be("Cpu");
			description.Namespace.Should().Be("IsabelDb.Test.Entities");
			description.FullTypeName.Should().Be("IsabelDb.Test.Entities.Cpu");
		}

		[Test]
		public void TestRoundtripEmpty()
		{
			var model = Roundtrip(TypeModel.Create(new Type[0]));
			model.Types.Should().BeEmpty();
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
			actualDescription.ResolvedType.Should().Be(description.ResolvedType, reason);
			actualDescription.TypeId.Should().Be(description.TypeId, reason);
			actualDescription.Fields.Should().HaveCount(description.Fields.Count, reason);
			for (int i = 0; i < actualDescription.Fields.Count; ++i)
			{
				actualDescription.Fields[i].Name.Should().Be(description.Fields[i].Name);
				actualDescription.Fields[i].MemberId.Should().Be(description.Fields[i].MemberId);
				actualDescription.Fields[i].Member.Should().Be(description.Fields[i].Member);
			}

			const string reason2 = "because the deserialized type model should still be able to resolve types by their name / id";
			actualModel.GetTypeName(type).Should().Be(description.FullTypeName, reason2);
			actualModel.TryGetType(description.TypeId).Should().Be(type, reason2);
			actualModel.GetTypeId(type).Should().Be(description.TypeId, reason2);
		}

		[Test]
		public void TestAddTypeAfterRoundtrip()
		{
			var model = Roundtrip(TypeModel.Create(new[] { typeof(KeyA) }));
			model.IsTypeRegistered(typeof(KeyA)).Should().BeTrue();

			model.Add(typeof(KeyB));
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
			model.GetTypeDescription(typeof(object)).ResolvedType.Should().Be<object>();
			model.GetTypeDescription(typeof(object)).TypeId.Should().Be(model.GetTypeId(typeof(object)));
			model.TryGetType(model.GetTypeId(typeof(object))).Should().Be<object>();

			var description = model.GetTypeDescription(typeof(object));
			description.Fields.Should().BeEmpty();
		}

		[Test]
		public void TestRegisterClassWithInterfaces()
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
			description.Fields.Should().HaveCount(1);
			var field = description.Fields[0];
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
			description.Fields.Should().HaveCount(1);
			description.Fields[0].Name.Should().Be("IsVisible");
			description.Fields[0].FieldTypeDescription.FullTypeName.Should().Be("System.Boolean");
			description.Fields[0].MemberId.Should().BeGreaterThan(0);
		}

		[Test]
		public void TestTwoFields()
		{
			var model = TypeModel.Create(new[] { typeof(Point) });
			var description = model.GetTypeDescription(typeof(Point));
			description.Fields.Should().HaveCount(2);
			description.Fields[0].Name.Should().Be("X");
			description.Fields[0].FieldTypeDescription.FullTypeName.Should().Be("System.Double");
			description.Fields[0].MemberId.Should().BeGreaterThan(0);

			description.Fields[1].Name.Should().Be("Y");
			description.Fields[1].FieldTypeDescription.FullTypeName.Should().Be("System.Double");
			description.Fields[1].MemberId.Should().BeGreaterThan(0);
			description.Fields[1].MemberId.Should().NotBe(description.Fields[0].MemberId);
		}

		[Test]
		public void TestFieldsAndInheritance()
		{
			var model = TypeModel.Create(new[] { typeof(Dog) });
			var dogDescription = model.GetTypeDescription(typeof(Dog));
			dogDescription.Fields.Should().HaveCount(2, "because Dog declares one property and one field");
			dogDescription.Fields[0].Name.Should().Be(nameof(Dog.EyeColor));
			dogDescription.Fields[1].Name.Should().Be(nameof(Dog.FurColor));

			var animalDescription = dogDescription.BaseType;
			animalDescription.Fields.Should().HaveCount(2, "because Animal declares two properties");
			animalDescription.Fields[0].Name.Should().Be(nameof(Animal.Name));
			animalDescription.Fields[1].Name.Should().Be(nameof(Animal.Age));
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
			model.Types.Should().BeEmpty();
			model.IsTypeRegistered(typeof(IPolymorphicCustomKey)).Should().BeFalse();
		}

		[Test]
		public void TestGetUnregisteredTypeId()
		{
			var model = TypeModel.Create(new Type[0]);
			new Action(() => model.GetTypeId(typeof(int)))
				.Should().Throw<ArgumentException>()
				.WithMessage("The type 'System.Int32' has not been registered with this type model!");
		}

		[Test]
		public void TestGetUnregisteredTypeName()
		{
			var model = TypeModel.Create(new Type[0]);
			new Action(() => model.GetTypeName(typeof(int)))
				.Should().Throw<ArgumentException>()
				.WithMessage("The type 'System.Int32' has not been registered with this type model!");
		}

		[Test]
		public void TestSerializableContract()
		{
			var model = TypeModel.Create(new[] {typeof(ISerializableType2)});
			var description = model.GetTypeDescription(typeof(ISerializableType2));
			description.Namespace.Should().Be("IsabelDb.Entities");
			description.Name.Should().Be("ISerializableType");
			description.FullTypeName.Should().Be("IsabelDb.Entities.ISerializableType");
		}

		[Test]
		public void TestRegisterTypeWithTooManyInterfaces()
		{
			new Action(() => TypeModel.Create(new[] {typeof(TooManyInterfaces)}))
				.Should().Throw<ArgumentException>()
				.WithMessage("The type 'IsabelDb.Test.Entities.V2.TooManyInterfaces' implements too many interfaces with the [SerializableContract] attribute! It should implement no more than 1 but actually implements: IsabelDb.Test.Entities.V2.ISerializableType2, IsabelDb.Test.Entities.IPolymorphicCustomKey");
		}

		[Test]
		public void TestGetUnreigsteredTypeId()
		{
			var model = TypeModel.Create(new Type[0]);
			model.TryGetType(42).Should().BeNull();
		}
	}
}
