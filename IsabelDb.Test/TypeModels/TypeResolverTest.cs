using FluentAssertions;
using IsabelDb.Test.Entities;
using IsabelDb.TypeModels;
using NUnit.Framework;

namespace IsabelDb.Test.TypeModels
{
	[TestFixture]
	public sealed class TypeResolverTest
	{
		[Test]
		public void TestRegisterPolymorphicType()
		{
			var resolver = new TypeResolver(new []{typeof(KeyA), typeof(IPolymorphicCustomKey)});
			resolver.IsRegistered(typeof(KeyA)).Should().BeTrue();
			resolver.Resolve("IsabelDb.Test.Entities.KeyA").Should().Be<KeyA>();

			resolver.IsRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue();
			resolver.Resolve("IsabelDb.Test.Entities.IPolymorphicCustomKey").Should().Be<IPolymorphicCustomKey>();
		}

		[Test]
		public void TestDataContractExplicitNamespaceNoName()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNamespaceNoName)});
			resolver.GetName(typeof(ExplicitNamespaceNoName))
				.Should().Be("Blur.ExplicitNamespaceNoName");
			resolver.Resolve("Blur.ExplicitNamespaceNoName")
			        .Should().Be<ExplicitNamespaceNoName>();
		}

		[Test]
		public void TestDataContractExplicitNameNoNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNameNoNamespace)});
			resolver.GetName(typeof(ExplicitNameNoNamespace))
				.Should().Be("IsabelDb.Test.Song2");
			resolver.Resolve("IsabelDb.Test.Song2")
			        .Should().Be<ExplicitNameNoNamespace>();
		}

		[Test]
		public void TestDataContractNoNameNoNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(DataContractNoNameNoNamespace)});
			resolver.GetName(typeof(DataContractNoNameNoNamespace))
				.Should().Be(typeof(DataContractNoNameNoNamespace).FullName);
			resolver.Resolve(typeof(DataContractNoNameNoNamespace).FullName)
			        .Should().Be<DataContractNoNameNoNamespace>();
		}

		[Test]
		public void TestExplicitNameAndNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNameAndNamespace)});
			resolver.GetName(typeof(ExplicitNameAndNamespace))
				.Should().Be("my app.FooName");
			resolver.Resolve("my app.FooName").Should().Be<ExplicitNameAndNamespace>();
		}
	}
}
