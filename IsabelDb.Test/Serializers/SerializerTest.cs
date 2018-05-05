using System;
using System.Collections.Generic;
using FluentAssertions;
using IsabelDb.Serializers;
using IsabelDb.Test.Entities;
using IsabelDb.TypeModels;
using NUnit.Framework;

namespace IsabelDb.Test.Serializers
{
	public sealed class SerializerTest
	{
		public static IEnumerable<string> StringValues => new[] {null, string.Empty, "Hello", "World"};
		public static IEnumerable<int> IntValues => new[] {-1, 0, 1};
		public static IEnumerable<double> DoubleValues => new[] {-1.4, 0, Math.E, Math.PI};

		[Test]
		[RequriedBehaviour]
		[System.ComponentModel.Description]
		public void TestRoundtripNonStandardCtor1([ValueSource(nameof(StringValues))] string value)
		{
			var obj = new HasCtor {StringValue = value};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.StringValue.Should().Be(value);
		}

		[Test]
		[System.ComponentModel.Description]
		public void TestRoundtripNonStandardCtor2([ValueSource(nameof(IntValues))] int value)
		{
			var obj = new HasCtor {IntValue = value};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.IntValue.Should().Be(value);
		}

		[Test]
		[System.ComponentModel.Description]
		public void TestRoundtripNonStandardCtor3([ValueSource(nameof(DoubleValues))] double value)
		{
			var obj = new HasCtor {DoubleValue = value};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.DoubleValue.Should().Be(value);
		}

		private static T Roundtrip<T>(T value)
		{
			var serializer = CreateSerializerFor<T>();
			var serializedValue = serializer.Serialize(value);
			var actualValue = serializer.Deserialize((byte[]) serializedValue);
			return (T)actualValue;
		}

		private static GenericSerializer<T> CreateSerializerFor<T>()
		{
			var typeModel = TypeModel.Create(new []{typeof(T)});
			var protoBufTypeModel = ProtobufTypeModel.Compile(typeModel);
			var serializer = new Serializer(new CompiledTypeModel(protoBufTypeModel, typeModel));
			return new GenericSerializer<T>(serializer);
		}
	}
}