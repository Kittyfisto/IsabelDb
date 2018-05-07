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
