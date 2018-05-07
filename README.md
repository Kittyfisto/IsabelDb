# IsabelDb
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

# Quick facts

- IsabelDb stores all data in a single file on disk
- A database consists of zero or more collections
- Each collection can store zero or more objects where each object must be serializable
- Each operation on a collection is atomic
- Each mutating operation blocks until its data is flushed to disk

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

Most of the type you will be creating your own data types to store in the database. The following rules apply:
- A serializable type must be marked with the [DataContract] attribute
- Fields/Properties which are to be serialized must be marked with the [DataMember] attribute
- Serializable types may inherit from other serializable types

## Backward/Forward Compatibility

IsabelDb offers both forward and backward compatibility.

## Breaking changes to serializable types

IsabelDb tolerates same changes to the type model, but several changes are considered breaking changes. The following document provides you with a detailed overview of which changes are allowed and which are breaking: [Serialization](Serialization.MD).
