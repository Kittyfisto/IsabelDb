# Serialization constraints

This page helps you to find out what is and what isn't possible with this database.

## Enums

This library supports serialization of enums so as long as they have one of the following underlying types:
- System.Byte
- System.SByte
- System.UInt16
- System.Int16
- System.Int32  

The following types are unfortunately **not** supported as underlying types:
- System.UInt32
- System.Int64
- System.UInt64

## Polymorphism involving built-in types and enums

This library does generally support polymorphism, however there are unsupported corner cases.  
You can, for example, do the following:

```csharp
var bag = database.GetBag<object>("Stuff");
bag.Put(42);
bag.Put(MyCustomEnum.SomeValue);
```

**HOWEVER**, you cannot use basic types (string, int, long, byte, etc..), nor enums in custom types in place of an object:

```csharp
[DataContract] public class MyCustomClass{
	[DataMember] public object Value;
}

var bag = database.GetBag<MyCustomClass>("Stuff");
bag.Put(MyCustomClass{Value = 42}); //< This will NOT work
bag.Put(MyCustomClass{Value = "Hello, World!"}); //< This will NOT work
bag.Put(MyCustomClass{Value = MyCustomEnum.SomeValue}); //< This will NOT work
```

It is my wish to enable this in the future, but I haven't been able to figure out how to configure a protobuf type model
which allows me to do this. Let me know if this is of interest to you.
'
