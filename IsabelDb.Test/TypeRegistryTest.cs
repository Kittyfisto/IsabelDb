using FluentAssertions;
using NUnit.Framework;
using System.Runtime.Serialization;

namespace IsabelDb.Test
{
	[DataContract(Name = "FooName", Namespace = "my app")]
	sealed class ExplicitNameAndNamespace
	{

	}

	[DataContract]
	sealed class DataContractNoNameNoNamespace
	{

	}

	[DataContract(Name = "Song2")]
	sealed class ExplicitNameNoNamespace
	{

	}

	[DataContract(Namespace = "Blur")]
	sealed class ExplicitNamespaceNoName
	{

	}


	[TestFixture]
	public sealed class TypeRegistryTest
	{
		[Test]
		public void TestDataContractExplicitNamespaceNoName()
		{
			var resolver = new TypeRegistry();
			resolver.Register<ExplicitNamespaceNoName>();
			resolver.GetName(typeof(ExplicitNamespaceNoName))
				.Should().Be("Blur.ExplicitNamespaceNoName");
		}

		[Test]
		public void TestDataContractExplicitNameNoNamespace()
		{
			var resolver = new TypeRegistry();
			resolver.Register<ExplicitNameNoNamespace>();
			resolver.GetName(typeof(ExplicitNameNoNamespace))
				.Should().Be("IsabelDb.Test.Song2");
		}

		[Test]
		public void TestDataContractNoNameNoNamespace()
		{
			var resolver = new TypeRegistry();
			resolver.Register<DataContractNoNameNoNamespace>();
			resolver.GetName(typeof(DataContractNoNameNoNamespace))
				.Should().Be(typeof(DataContractNoNameNoNamespace).AssemblyQualifiedName);
		}

		[Test]
		public void TestExplicitNameAndNamespace()
		{
			var resolver = new TypeRegistry();
			resolver.Register<ExplicitNameAndNamespace>();
			resolver.GetName(typeof(ExplicitNameAndNamespace))
				.Should().Be("my app.FooName");
		}
	}
}
