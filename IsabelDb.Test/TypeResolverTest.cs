using FluentAssertions;
using IsabelDb.Test.Entities;
using IsabelDb.TypeModels;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class TypeResolverTest
	{
		[Test]
		public void TestRegisterPolymorphicType()
		{
			var resolver = new TypeResolver(new []{typeof(KeyA), typeof(IPolymorphicCustomKey)});
			resolver.IsRegistered(typeof(KeyA)).Should().BeTrue();
			resolver.IsRegistered(typeof(IPolymorphicCustomKey)).Should().BeTrue();
		}

		[Test]
		public void TestDataContractExplicitNamespaceNoName()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNamespaceNoName)});
			resolver.GetName(typeof(ExplicitNamespaceNoName))
				.Should().Be("Blur.ExplicitNamespaceNoName");
		}

		[Test]
		public void TestDataContractExplicitNameNoNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNameNoNamespace)});
			resolver.GetName(typeof(ExplicitNameNoNamespace))
				.Should().Be("IsabelDb.Test.Song2");
		}

		[Test]
		public void TestDataContractNoNameNoNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(DataContractNoNameNoNamespace)});
			resolver.GetName(typeof(DataContractNoNameNoNamespace))
				.Should().Be(typeof(DataContractNoNameNoNamespace).FullName);
		}

		[Test]
		public void TestExplicitNameAndNamespace()
		{
			var resolver = new TypeResolver(new []{typeof(ExplicitNameAndNamespace)});
			resolver.GetName(typeof(ExplicitNameAndNamespace))
				.Should().Be("my app.FooName");
		}
	}
}
