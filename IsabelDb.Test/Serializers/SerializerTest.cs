using System;
using System.Collections.Generic;
using System.Linq;
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
		public static IEnumerable<byte> ByteValues => new byte[] {byte.MinValue, 128, byte.MaxValue};
		public static IEnumerable<int> IntValues => new[] {int.MinValue, -1, 0, 1, int.MaxValue};
		public static IEnumerable<byte[]> ByteArrayValues => new[] {null, new byte[]{0}, ByteValues.ToArray()};
		public static IEnumerable<int[]> IntArrayValues => new[] {null, new[]{0}, IntValues.ToArray()};
		public static IEnumerable<int?> NullableIntValues => new int?[] {int.MinValue, -1, 0, 1, int.MaxValue, null};
		public static IEnumerable<double> DoubleValues => new[] {double.MinValue, -1.4, 0, Math.E, Math.PI, double.MaxValue};

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

		[Test]
		public void TestRoundtripIntList()
		{
			var obj = new IntList
			{
				Values = new List<int>(IntValues)
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Values.Should().Equal(IntValues);
		}

		//[Test]
		//public void Test()
		//{
		//	var typeModel = ProtoBuf.Meta.TypeModel.Create();
		//	var type = typeModel.Add(typeof(IntArray), applyDefaultBehaviour: false);
		//	var field = type.AddField(1, "Values", typeof(int), typeof(int[]));
		//	field.IsPacked = true;

		//	var value = new IntArray {Values = new int[10000]};
		//	using (var stream = new MemoryStream())
		//	{
		//		typeModel.Serialize(stream, value);
		//		stream.Position = 0;
		//		var actualValue = typeModel.Deserialize(stream, null, typeof(IntArray));
		//		int n = 0;
		//	}
		//}

		[Test]
		public void TestRoundtripIntArray([ValueSource(nameof(IntArrayValues))] int[] values)
		{
			var obj = new IntArray
			{
				Values = values
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Values.Should().Equal(values);
		}

		[Test]
		public void TestRoundtripByteArray([ValueSource(nameof(ByteArrayValues))] byte[] values)
		{
			var obj = new ByteArray
			{
				Values = values
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Values.Should().Equal(values);
		}

		[Test]
		public void TestRoundtripNullableField([ValueSource(nameof(NullableIntValues))] int? value)
		{
			var obj = new NullableInt
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
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