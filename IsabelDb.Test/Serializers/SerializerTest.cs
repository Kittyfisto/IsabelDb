using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using IsabelDb.Serializers;
using IsabelDb.Test.Entities;
using IsabelDb.TypeModels;
using IsabelDb.TypeModels.Surrogates;
using NUnit.Framework;
using TypeModel = IsabelDb.TypeModels.TypeModel;

namespace IsabelDb.Test.Serializers
{
	[TestFixture]
	public sealed class SerializerTest
	{
		public static IEnumerable<string> StringValues => new[] {null, string.Empty, "Hello", "World"};
		public static IEnumerable<byte> ByteValues => new byte[] {byte.MinValue, 128, byte.MaxValue};
		public static IEnumerable<int> IntValues => new[] {int.MinValue, -1, 0, 1, int.MaxValue};
		public static IEnumerable<byte[]> ByteArrayValues => new[] {null, new byte[] {0}, ByteValues.ToArray()};
		public static IEnumerable<int[]> IntArrayValues => new[] {null, new[] {0}, IntValues.ToArray()};
		public static IEnumerable<RowId> ValueKeyValues => new[]{new RowId(0), new RowId(long.MinValue), new RowId(long.MaxValue) };

		public static IEnumerable<SByteEnum> SByteEnumValues => Enum.GetValues(typeof(SByteEnum)).Cast<SByteEnum>().ToList();
		public static IEnumerable<ByteEnum> ByteEnumValues => Enum.GetValues(typeof(ByteEnum)).Cast<ByteEnum>().ToList();
		public static IEnumerable<UInt16Enum> UInt16EnumValues => Enum.GetValues(typeof(UInt16Enum)).Cast<UInt16Enum>().ToList();
		public static IEnumerable<Int16Enum> Int16EnumValues => Enum.GetValues(typeof(Int16Enum)).Cast<Int16Enum>().ToList();
		public static IEnumerable<Int32Enum> Int32EnumValues => Enum.GetValues(typeof(Int32Enum)).Cast<Int32Enum>().ToList();

		public static IEnumerable<short?> NullableShortValues =>
			new short?[] {short.MinValue, -1, 0, 1, short.MaxValue, null};

		public static IEnumerable<int?> NullableIntValues => new int?[] {int.MinValue, -1, 0, 1, int.MaxValue, null};
		public static IEnumerable<long?> NullableLongValues => new long?[] {long.MinValue, -1, 0, 1, long.MaxValue, null};

		public static IEnumerable<float?> NullableFloatValues => new float?[]
			{float.MinValue, -1.4f, 0, (float) Math.E, (float) Math.PI, float.MaxValue, null};

		public static IEnumerable<double?> NullableDoubleValues => new double?[]
			{double.MinValue, -1.4, 0, Math.E, Math.PI, double.MaxValue, null};

		public static IEnumerable<double> DoubleValues => new[] {double.MinValue, -1.4, 0, Math.E, Math.PI, double.MaxValue};

		public static IEnumerable<IPAddress> IpAddresses => new[]
		{
			IPAddress.Loopback, IPAddress.Any, IPAddress.IPv6Any, IPAddress.IPv6Loopback, IPAddress.Broadcast,
			IPAddress.IPv6None, IPAddress.None, IPAddress.Parse("192.168.0.1")
		};

		public static IEnumerable<Version> Versions => new[]
		{
			new Version(),
			new Version(1, 0),
			new Version(1, 2, 3),
			new Version(1, 2, 3, 4)
		};

		[Test]
		public void Test([Values(Int32Enum.A, Int32Enum.B, Int32Enum.C)] Int32Enum enumValue)
		{
			var tm = ProtoBuf.Meta.TypeModel.Create();
			var objectType = tm.Add(typeof(object), false);
			tm.Add(typeof(SomeClass), true);
			var someEnumType = tm.Add(typeof(Int32Enum), false);
			someEnumType.Add(0, "A");
			someEnumType.Add(1, "B");
			someEnumType.Add(2, "C");

			tm.Add(typeof(ClassWithObject), true);

			//objectType.AddSubType(300, typeof(SomeClass));
			//objectType.AddSubType(200, typeof(SomeEnum));

			var stream = new MemoryStream();
			//tm.Serialize(stream, new ClassWithObject{Value = enumValue});
			tm.Serialize(stream, enumValue);
			stream.Position = 0;

			var actualValue = tm.Deserialize(stream, null, typeof(Int32Enum));
			actualValue.Should().Be(enumValue);
			//var actualValue = tm.Deserialize(stream, null, typeof(ClassWithObject));
			//actualValue.Should().NotBeNull();
			//actualValue.Should().BeOfType<ClassWithObject>();
			//((ClassWithObject) actualValue).Value.Should().Be(enumValue);
		}

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
		public void TestRoundtripNullableShort([ValueSource(nameof(NullableShortValues))]
		                                       short? value)
		{
			var obj = new NullableShort
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripNullableInt([ValueSource(nameof(NullableIntValues))]
		                                     int? value)
		{
			var obj = new NullableInt
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripNullableLong([ValueSource(nameof(NullableLongValues))]
		                                      long? value)
		{
			var obj = new NullableLong
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripNullableFloat([ValueSource(nameof(NullableFloatValues))]
		                                       float? value)
		{
			var obj = new NullableFloat
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripNullableDouble([ValueSource(nameof(NullableDoubleValues))]
		                                        double? value)
		{
			var obj = new NullableDouble
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripByteEnum([ValueSource(nameof(ByteEnumValues))] ByteEnum value)
		{
			Roundtrip(value).Should().Be(value);
			Roundtrip(new GenericType<ByteEnum> {Value = value}).Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripSByteEnum([ValueSource(nameof(SByteEnumValues))] SByteEnum value)
		{
			Roundtrip(value).Should().Be(value);
			Roundtrip(new GenericType<SByteEnum> {Value = value}).Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripUInt16Enum([ValueSource(nameof(UInt16EnumValues))] UInt16Enum value)
		{
			Roundtrip(value).Should().Be(value);
			Roundtrip(new GenericType<UInt16Enum> {Value = value}).Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripInt16Enum([ValueSource(nameof(Int16EnumValues))] Int16Enum value)
		{
			Roundtrip(value).Should().Be(value);
			Roundtrip(new GenericType<Int16Enum> {Value = value}).Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripInt32Enum([ValueSource(nameof(Int32EnumValues))] Int32Enum value)
		{
			Roundtrip(value).Should().Be(value);
			Roundtrip(new GenericType<Int32Enum> {Value = value}).Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripTypeWithEnum([ValueSource(nameof(Int32EnumValues))] Int32Enum value)
		{
			var obj = new TypeWithEnum
			{
				Value = value
			};
			var actualObj = Roundtrip(obj);
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		public void TestRoundtripIpAddress([ValueSource(nameof(IpAddresses))] IPAddress value)
		{
			var actualObj = Roundtrip(value, typeof(IPAddressSurrogate));
			actualObj.Should().Be(value);
		}

		[Test]
		public void TestRoundtripVersion([ValueSource(nameof(Versions))] Version value)
		{
			var actualObj = Roundtrip(value, typeof(VersionSurrogate));
			actualObj.Should().Be(value);
		}

		[Test]
		[Ignore("This is unfortunately not supported yet")]
		public void TestRoundtripStringAsObject([ValueSource(nameof(StringValues))] string value)
		{
			var actualObj = Roundtrip(new ClassWithObject
			{
				Value = value
			});
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		[Test]
		[Ignore("This is unfortunately not supported yet")]
		public void TestRoundtripIntAsObject([ValueSource(nameof(IntValues))] int value)
		{
			var actualObj = Roundtrip(new ClassWithObject
			{
				Value = value
			});
			actualObj.Should().NotBeNull();
			actualObj.Value.Should().Be(value);
		}

		private static T Roundtrip<T>(T value, params Type[] additionalTypes)
		{
			var serializer = CreateSerializerFor<T>(additionalTypes);
			var serializedValue = serializer.Serialize(value);
			var actualValue = serializer.Deserialize((byte[]) serializedValue);
			return (T) actualValue;
		}

		private static GenericSerializer<T> CreateSerializerFor<T>(params Type[] additionalTypes)
		{
			var types = new List<Type>(additionalTypes) {typeof(T)};
			var typeModel = TypeModel.Create(types);
			var protoBufTypeModel = ProtobufTypeModel.Compile(typeModel);
			var serializer = new Serializer(new CompiledTypeModel(protoBufTypeModel, typeModel));
			return new GenericSerializer<T>(serializer);
		}
	}
}
