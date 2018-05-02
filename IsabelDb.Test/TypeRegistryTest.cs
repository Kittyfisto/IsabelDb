using System;
using FluentAssertions;
using IsabelDb.TypeModel;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class TypeRegistryTest
	{
		[Test]
		public void TestRegisterPolymorphicType()
		{
			var resolver = new TypeRegistry(new Type[0]);
			resolver.Register(typeof(KeyA));
			resolver.IsRegistered(typeof(KeyA)).Should().BeTrue();
			resolver.Register(typeof(IPolymorphicCustomKey));
			resolver.IsRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue();
		}

		[Test]
		public void TestDataContractExplicitNamespaceNoName()
		{
			var resolver = new TypeRegistry(new []{typeof(ExplicitNamespaceNoName)});
			resolver.GetName(typeof(ExplicitNamespaceNoName))
				.Should().Be("Blur.ExplicitNamespaceNoName");
		}

		[Test]
		public void TestDataContractExplicitNameNoNamespace()
		{
			var resolver = new TypeRegistry(new []{typeof(ExplicitNameNoNamespace)});
			resolver.GetName(typeof(ExplicitNameNoNamespace))
				.Should().Be("IsabelDb.Test.Song2");
		}

		[Test]
		public void TestDataContractNoNameNoNamespace()
		{
			var resolver = new TypeRegistry(new []{typeof(DataContractNoNameNoNamespace)});
			resolver.GetName(typeof(DataContractNoNameNoNamespace))
				.Should().Be(typeof(DataContractNoNameNoNamespace).FullName);
		}

		[Test]
		public void TestExplicitNameAndNamespace()
		{
			var resolver = new TypeRegistry(new []{typeof(ExplicitNameAndNamespace)});
			resolver.GetName(typeof(ExplicitNameAndNamespace))
				.Should().Be("my app.FooName");
		}
	}
}
