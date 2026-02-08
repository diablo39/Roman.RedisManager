# Domain: Domain Model

## Overview
The Domain layer is the innermost layer in the Clean Architecture. It contains entities, value objects, and repository interfaces. It has **zero external NuGet dependencies** — only framework types.

## File Structure
```
src/Roman.RedisManager.Domain/
├── Entities/
│   ├── RedisServer.cs
│   ├── RedisKey.cs
│   └── RedisSearchResult.cs
├── Repositories/
│   ├── IRedisRepository.cs
│   └── IRedisServerRepository.cs
└── Roman.RedisManager.Domain.csproj
```

## Entity Patterns

### RedisServer — Traditional Constructor with Internal Setters

```csharp
namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServer
    {
        public string Name { get; internal set; }
        public IEnumerable<string> Endpoints { get; internal set; }

        public RedisServer(string name, IEnumerable<string> endpoints)
        {
            Name = name;
            Endpoints = endpoints;
        }
    }
}
```

**Conventions observed:**
- Block-scoped namespace (not file-scoped).
- Properties use `internal set` — writable within the assembly, read-only externally.
- Constructor takes all required state.
- No base class or interface for entities.

### RedisKey — Primary Constructor with Protected Setter

```csharp
namespace Roman.RedisManager.Domain.Entities
{
    public class RedisKey(string key)
    {
        public string Key { get; protected set; } = key;
    }
}
```

**Conventions observed:**
- C# primary constructor for simple entities.
- Property uses `protected set` — writable only by derived classes.
- Direct property initialization from primary constructor parameter.

### RedisSearchResult — Primary Constructor with Computed Property

```csharp
namespace Roman.RedisManager.Domain.Entities
{
    public class RedisSearchResult(IEnumerable<RedisKey> keys, long cursor)
    {
        private const long _emptyCursor = 0;
        public long Cursor { get; protected set; } = cursor;
        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;
        public bool HasMoreResults
        {
            get { return Cursor != _emptyCursor; }
        }
    }
}
```

**Conventions observed:**
- Primary constructor with multiple parameters.
- Private const for magic values (`_emptyCursor = 0`).
- Computed property (`HasMoreResults`) with explicit getter.
- `protected set` for all data properties.

## Repository Interface Patterns

### IRedisRepository
```csharp
namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        RedisSearchResult SearchForKeys(string predicate);
    }
}
```

### IRedisServerRepository
```csharp
namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisServerRepository
    {
        IReadOnlyCollection<RedisServer> ListRedisServers();
    }
}
```

**Conventions observed:**
- Interfaces live in `Domain/Repositories/`, separate from entities.
- Return types are domain entities or `IReadOnlyCollection<T>` — never infrastructure types.
- Method names are descriptive verbs: `SearchForKeys`, `ListRedisServers`.
- Synchronous methods (no `Task<T>` return types currently).

## Project Dependencies
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Zero `PackageReference` items — pure domain layer.
