# Style Guide: Repository Implementations

> Conventions unique to this project.

## Namespace
`Roman.RedisManager.Infrastructure.Repositories` — mirrors the Domain layer's `Repositories/` folder.

## Constructor Injection with Null Guards
Only some repositories use null guards — it's inconsistent:
- `RedisServerRepository`: `?? throw new ArgumentNullException(nameof(redisConfiguration))`
- `RedisRepository`: No null guard.

## Readonly Fields
Injected dependencies are stored in `private readonly` fields with underscore prefix:
```csharp
private readonly IConnectionMultiplexer _redisConnectionMultiplexer;
private readonly IOptions<RedisConfiguration> _redisConfiguration;
```

## Type Alias for Name Conflicts
When StackExchange.Redis types conflict with domain types, a using alias resolves it:
```csharp
using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;
```

## LINQ Mapping to Domain Entities
Repository methods map infrastructure types to domain entities inline using `.Select()`:
```csharp
var result = configuration.Servers.Select(e => new RedisServer(e.Name, e.Endpoints.ToList())).ToList();
```

## Double Null Validation on Options
`RedisServerRepository` validates both the `IOptions<T>` wrapper (in constructor) and the `.Value` (at call site):
```csharp
RedisConfiguration configuration = _redisConfiguration.Value ?? throw new Exception("Configuration can't be null");
```

## DI Lifetime
Repositories are registered as **singletons** in `Program.cs`.
