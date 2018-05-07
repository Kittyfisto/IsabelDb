# IsabelDb

[![Build status](https://ci.appveyor.com/api/projects/status/mqcvw9ouvh2xi12u?svg=true)](https://ci.appveyor.com/project/Kittyfisto/isabeldb)

IsabelDb is a key value store which allows you to persist .NET objects on disk and read them back again.

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

# Installation

Simply head over to [nuget.org](https://www.nuget.org/packages/IsabelDb/) to download the latest release of IsabelDb or use the following command in the Package Manager Console:
`Install-Package IsabelDb`

# Requirements

Currently, IsabelDb targets .NET 4.5.  
If you require other targets, such as .NET core , then please let me know.

# Quick facts

- IsabelDb stores all data in a single file on disk
- A database consists of zero or more collections
- Each collection can store zero or more objects where each object must be serializable
- Each operation on a collection is atomic
- Each mutating operation blocks until its data is flushed to disk
- Any custom type with a data contract can be stored in the database

## Collections

IsabelDb offers several collections which behave similar to their .NET counterpart, but their contents are stored on disk and not in memory. As such, a collection can exceed the size of a process' virtual memory and its computer's physical memory.

### Bag

A collection which stores a list of values: Values can be added to the collection and streamed back by iterating over GetAll(). Removing individiual values is not possible (use Dictionary if that's necessary).

```csharp
var items = database.GetDictionary<object>("Items");
items.PutMany(new[]{1, 42, "Hello", "World!");
Console.WriteLine(string.Join(", ", items.GetAll()); //< Prints '1, 42, Hello, World!'
```

### Dictionary

A collection which maps values to non-null keys. There can never be more than one value for the same key. If you put a new key / value pair into
the collection, then any existing value for that same key will be overwritten:

```csharp
var items = database.GetDictionary<string, object>("Items");
items.Put("foo", "Hello, World!");
Console.WriteLine(items.Get("foo")); //< Prints 'Hello, World!'
items.Put("foo", 42);
Console.WriteLine(items.Get("foo")); //< Prints '42'
```

### MultiValueDictionary

A collection which maps values to non-null keys: There can be multiple values for the same key. If you put a key / value pair into the collection
then that value will be appended to the previous list of values:

```csharp
var items = database.GetMultiValueDictionary<string, object>("Items");
items.Put("foo", "Hello");
items.Put("foo", "World!");
Console.WriteLine(string.Join(", ", items.Get("foo"))); //< Prints 'Hello, World!'
```

## Defining Serializable Types

IsabelDb works with every custom type if it follows these rules:
- The type must be marked with the [DataContract] attribute
- Fields/Properties which are to be serialized must be marked with the [DataMember] attribute
- The field/property types which are marked with the [DataMember] attribute must themselves be serializable

These rules are pretty much aligned to what Microsoft expects from a data contract. You may head over to [docs.microsoft.com](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-create-a-basic-data-contract-for-a-class-or-structure) for a more detailed explanation.

## Backward compatibility

Backward compatibility in this context refers to the ability to:
- Read data from a database which was written with a previous version
- Modify data in a database which was written with a previous version

The following list of changes is permitted without breaking backward compatibility:

If you change the name or namespace of a type marked with the [DataContract] attribute, then you should put its old namespace / name in the attribute:
```csharp
[DataContract(Namespace "OldNamespace", Name = "OldTypeName")] public class NewType {}
```

If you change the name of a field / property marked with the [DataMember] attribute, then you should put its old name in the attribute:
```csharp
[DataMember(Name = "OldPropertyName")] public string NewPropertyName { get; set; }
```

Adding new fields / properties with the [DataMember] attribute.

You can remove fields / properties with the [DataMember] attribute, however if you do so, then you will lose that data upon roundtripping and then reading back the object in an older version.


## Forward Compatibility

Forward compatibility in this context refers to the ability to:
- Read data from a database which was written with a future version
- Modify data in a database which was written with a future version

## Breaking changes to serializable types

IsabelDb tolerates same changes to the type model, but several changes are considered breaking changes. The following document provides you with a detailed overview of which changes are allowed and which are breaking: [Serialization](Serialization.MD).
