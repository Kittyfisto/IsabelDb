# IsabelDb
A .NET key value database to persist objects on disk

# Introduction

The following example gives you a quick introduction into how to use this database:

```
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
Every operation on a collection which modifies it (such as Put, Clear, Remove, etc...) is atomic and blocks until the data has been flushed to disk.

## Dictionary

A collection which maps values to non-null keys. There can never be more than one value for the same key. If you put a new key / value pair into
the collection, then any existing value for that same key will be overwritten:

```
var items = database.GetDictionary<string, object>("Items");
items.Put("foo", "Hello, World!");
Console.WriteLine(items.Get("foo")); //< Prints 'Hello, World!'
items.Put("foo, 42);
Console.WriteLine(items.Get("foo")); //< Prints '42'
```

## MultiValueDictionary

A collection which maps values to non-null keys: There can be multiple values for the same key. If you put a key / value pair into the collection
then that value will be appended to the previous list of values:

```
var items = database.GetMultiValueDictionary<string, object>("Items");
items.Put("foo", "Hello");
items.Put("foo, "World!");
Console.WriteLine(string.Join(", ", items.Get("foo"))); //< Prints 'Hello, World!'
```

