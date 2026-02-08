# Style Guide: CQRS Handlers

> Conventions unique to this project — specific to the Wolverine mediator pattern.

## Static Class + Static Handle Method
Handlers are **static classes** with a **static `Handle` method** — this is the Wolverine convention:
```csharp
public static class RedisServersQueryHandler
{
    public static RedisServersQueryResult Handle(RedisServersQuery query, IRedisServerRepository repository)
    { ... }
}
```

## Handler Naming
`{MessageType}Handler` — e.g., `RedisServersQueryHandler` handles `RedisServersQuery`.

## Co-Located Types
The query class, DTOs, and handler are **all defined in the same file**:
```
RedisServersQuery           (class — the message)
RedisServerDto              (record — the DTO)
RedisServersQueryResult     (record — the response)
RedisServersQueryHandler    (static class — the handler)
```

## Query vs Result Type Styles
- **Queries**: Plain classes with `{ get; set; }` properties.
- **Results/DTOs**: C# `record` types with positional parameters.

## Cross-Layer Namespace
Despite living in `src/Roman.RedisManager.Application/CQRS/`, the handler uses namespace:
```csharp
namespace Roman.RedisManager.Web.Wolverine
```
This is intentional — it places the types where the Web layer can reference them without extra using directives.

## Inline Pagination Logic
Pagination is computed directly in the handler (not delegated to a shared utility):
```csharp
var skip = (query.PageNumber - 1) * query.PageSize;
var pagedServers = allServers.Skip(skip).Take(query.PageSize)...
```

## Entity-to-DTO Mapping in Handler
Mapping uses LINQ `.Select()` — no AutoMapper or dedicated mapper classes:
```csharp
.Select(s => new RedisServerDto(s.Name, s.Name.ToMd5Hash()))
```

## Extension Methods for Transformations
Complex transformations (like MD5 hashing) use extension methods imported via `GlobalUsings.cs`:
```csharp
global using Roman.RedisManager.Extensions;
```
