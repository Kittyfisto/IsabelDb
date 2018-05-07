# IsabelDb
IsabelDb is a key value store which allows you to write .NET objects to disk and read them back again.

# Introduction

The following example gives you a quick introduction into how to use this database:

```csharp
[DataContract]
public class Document
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Content { get; set; }
}

using (var database = IsabelDb.Database.OpenOrCreate("some file path.isdb", new[]{typeof(Document)}))
{
    var cache = database.GetDictionary<int, Document>("Cache");
    cache.Put(42, new Document{Name = "Foo.txt", Content = "..." });
    ....
    var document = cache.Get(42);
    if (document != null)
    {
        Console.WriteLine(document.Content);
    }
}
```

# Concepts

A database created with this library consists of zero or more collections.
Each collection can store zero or more objects where each object must be serializable.
An object is serializable if it's a natively supported type (integer types, floating-point types, string, lists, arrays, etc...)
or if it is marked with the DataContract attribute.

## Dictionary

A collection which maps values to non-null keys. There can never be more than one value for the same key. If you put a new key / value pair into
the collection, then any existing value for that same key will be overwritten:

```csharp
var items = database.GetDictionary<string, object>("Items");
items.Put("foo", "Hello, World!");
Console.WriteLine(items.Get("foo")); //< Prints 'Hello, World!'
items.Put("foo", 42);
Console.WriteLine(items.Get("foo")); //< Prints '42'
```

## MultiValueDictionary

A collection which maps values to non-null keys: There can be multiple values for the same key. If you put a key / value pair into the collection
then that value will be appended to the previous list of values:

```csharp
var items = database.GetMultiValueDictionary<string, object>("Items");
items.Put("foo", "Hello");
items.Put("foo", "World!");
Console.WriteLine(string.Join(", ", items.Get("foo"))); //< Prints 'Hello, World!'
```

## Defining Serializable Types

Most of the type you will be creating your own data types to store in the database. The following rules apply:
- A serializable type must be marked with the [DataContract] attribute
- Fields/Properties which are to be serialized must be marked with the [DataMember] attribute
- Serializable types may inherit from other serializable types

## Breaking changes to serializable types

The following list gives you an overview over the list of changes to a type model which you should avoid if you desire to read data written with previous type models:

### Changing the base type of a serializable type
Changing the base class of a type is considered a breaking change and will prevent you from reading data from database prior to this change:

V1:
```csharp
[DataContract] public abstract class Base{}
[DataContract] public class Foo : Base {}
```

V2:
```csharp
[DataContract] public abstract class Base{}
[DataContract] public class Foo {}
```

### Changing the type of serializable field / property
Changing the type of a field is considered a breaking change and will prevent you from reading data from database prior to this change:

V1:
```csharp
[DataContract] public class Foo
{
    [DataMember]
    public int Age { get; set; }
}
```

V2:
```csharp
[DataContract] public class Foo
{
    [DataMember]
    public double Age { get; set; }
}
```

## Backward/Forward Compatibility

IsabelDb offers both forward and backward compatibility.
If you decide 
