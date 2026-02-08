# Style Guide: Domain Entities

> Conventions unique to this project — not generic DDD practices.

## Block-Scoped Namespaces
All entity files use block-scoped namespaces (`namespace X { ... }`), not file-scoped (`namespace X;`):
```csharp
namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServer { ... }
}
```

## Restricted Setters — Mixed Strategy
- `internal set` on `RedisServer` properties — writable within the Domain assembly.
- `protected set` on `RedisKey` and `RedisSearchResult` properties — writable only by subclasses.

There is no single consistent pattern — both are used depending on the entity.

## Primary Constructors for Simple Entities
Single-property entities use C# primary constructors:
```csharp
public class RedisKey(string key)
{
    public string Key { get; protected set; } = key;
}
```

## Traditional Constructors for Multi-Property Entities
Entities with multiple properties use explicit constructors:
```csharp
public RedisServer(string name, IEnumerable<string> endpoints)
{
    Name = name;
    Endpoints = endpoints;
}
```

## Private Constants for Magic Values
Named constants replace magic numbers:
```csharp
private const long _emptyCursor = 0;
```

Convention: `_camelCase` with underscore prefix for private const fields.

## Computed Properties Use Explicit Get Blocks
```csharp
public bool HasMoreResults
{
    get { return Cursor != _emptyCursor; }
}
```
Not expression-bodied (`=>`), but explicit `get { return ...; }`.

## Collection Types
- `IEnumerable<T>` is the default collection property type (not `List<T>`, `IList<T>`, etc.).
