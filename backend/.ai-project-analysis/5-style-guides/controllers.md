# Style Guide: Controllers

> Conventions unique to this project — not generic ASP.NET Core practices.

## Expression-Bodied Constructors
Controllers use single-line expression-bodied constructors when there's only one dependency:
```csharp
public RedisServersController(IMessageBus bus) => _bus = bus;
```

## Wolverine IMessageBus — Not Direct Repository Injection
Controllers inject `IMessageBus` (Wolverine) and dispatch query/command objects. They never inject repository interfaces directly.

## Route Naming: Kebab-Case Resource Names
```csharp
[Route("api/redis-servers")]
```
Resource names in routes use **kebab-case** (not camelCase or PascalCase).

## Direct DTO Return Types
Action methods return DTOs directly — not wrapped in `IActionResult` or `ActionResult<T>`:
```csharp
public async Task<RedisServersQueryResult> GetServers(...)
```

## Query Parameters with Defaults
Pagination parameters are declared as `[FromQuery]` method parameters with inline defaults:
```csharp
[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10
```

## Inline Message Construction
Query objects are constructed inline in the `InvokeAsync` call:
```csharp
return await _bus.InvokeAsync<RedisServersQueryResult>(
    new RedisServersQuery { PageNumber = pageNumber, PageSize = pageSize });
```
