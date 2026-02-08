# Domain: CQRS Messaging

## Overview
The project implements the CQRS (Command Query Responsibility Segregation) pattern using **Wolverine** as an in-process mediator. Handlers live in the Application layer and are auto-discovered by Wolverine's assembly scanning.

## File Structure
```
src/Roman.RedisManager.Application/
├── CQRS/
│   └── RedisServersQueryHandler.cs
├── GlobalUsings.cs
└── Roman.RedisManager.Application.csproj
```

## Handler Convention

Wolverine handlers follow a strict convention-based approach — no interfaces, no base classes, no attributes. Wolverine discovers handlers by method signature.

### Example: RedisServersQueryHandler

```csharp
namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServersQuery
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public record RedisServerDto(string Name, string Id);

    public record RedisServersQueryResult(
        List<RedisServerDto> Servers,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public static class RedisServersQueryHandler
    {
        public static RedisServersQueryResult Handle(RedisServersQuery query, IRedisServerRepository repository)
        {
            var allServers = repository.ListRedisServers().ToList();
            var totalCount = allServers.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedServers = allServers
                .Skip(skip)
                .Take(query.PageSize)
                .Select(s => new RedisServerDto(s.Name, s.Name.ToMd5Hash()))
                .ToList();
            return new RedisServersQueryResult(pagedServers, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
```

### Key Observations

1. **Static handler class**: `RedisServersQueryHandler` is a `static class`.
2. **Static Handle method**: The method is named `Handle` — this is the Wolverine convention for synchronous handlers. Async handlers would use `HandleAsync`.
3. **First parameter is the message**: `RedisServersQuery query` — Wolverine routes messages to handlers based on this type.
4. **Additional parameters are DI-resolved**: `IRedisServerRepository repository` is injected by Wolverine from the DI container.
5. **Return type is the response**: The method returns `RedisServersQueryResult` directly.
6. **Co-located types**: The query class, DTOs, and handler are all in the same file.
7. **Namespace**: Uses `Roman.RedisManager.Web.Wolverine` despite being in the Application project — this is a deliberate cross-layer namespace choice.

## Message Types

### Query Classes
- Plain C# classes (not records) with public `get; set;` properties.
- Example: `RedisServersQuery` with `PageSize` and `PageNumber`.

### Result Types
- C# `record` types with positional parameters for immutability.
- Example: `RedisServersQueryResult(List<RedisServerDto> Servers, int TotalCount, int PageNumber, int PageSize)`.

### DTOs
- C# `record` types used to project domain entities for API responses.
- Example: `RedisServerDto(string Name, string Id)`.

## Domain Entity to DTO Mapping
Mapping happens inside the handler using LINQ `.Select()`:
```csharp
.Select(s => new RedisServerDto(s.Name, s.Name.ToMd5Hash()))
```

The `ToMd5Hash()` extension method is imported via `global using Roman.RedisManager.Extensions;` in `GlobalUsings.cs`.

## Wolverine Configuration (in Program.cs)
```csharp
builder.UseWolverine(opts =>
{
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (assembly.GetName().Name!.StartsWith("Roman.RedisManager"))
        {
            opts.Discovery.IncludeAssembly(assembly);
        }
    }
    opts.Durability.Mode = DurabilityMode.MediatorOnly;
});
```

- Scans all `Roman.RedisManager.*` assemblies for handlers.
- Runs in `MediatorOnly` mode (no external messaging transport).

## Invocation from Controllers
```csharp
return await _bus.InvokeAsync<RedisServersQueryResult>(
    new RedisServersQuery { PageNumber = pageNumber, PageSize = pageSize });
```
