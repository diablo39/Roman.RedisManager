# Domain: Data Layer

## Overview
The data layer lives in the Infrastructure project and provides concrete implementations of repository interfaces defined in the Domain layer. It uses **StackExchange.Redis** for Redis connectivity and the **Options pattern** for configuration.

## File Structure
```
src/Roman.RedisManager.Infrastructure/
├── Configuration/
│   ├── RedisConfiguration.cs
│   └── RedisServerConfiguration.cs
├── Repositories/
│   ├── RedisRepository.cs
│   └── RedisServerRepository.cs
└── Roman.RedisManager.Infrastructure.csproj
```

## Repository Implementations

### RedisRepository — StackExchange.Redis Integration

```csharp
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using StackExchange.Redis;
using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redisConnectionMultiplexer;

        public RedisRepository(IConnectionMultiplexer redisConnectionMultiplexer)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
        }

        public RedisSearchResult SearchForKeys(string predicate)
        {
            var serverEndpoints = _redisConnectionMultiplexer.GetEndPoints();
            var server = _redisConnectionMultiplexer.GetServer(serverEndpoints.First());
            var searchResult = server.Keys(pattern: predicate);
            var cursor = (IScanningCursor)searchResult;
            var keys = searchResult.Select(e => new RedisKey(e.ToString())).ToList();
            return new RedisSearchResult(keys, cursor.Cursor);
        }
    }
}
```

**Key observations:**
- Type alias: `using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;` resolves naming conflict with `StackExchange.Redis.RedisKey`.
- `readonly` field for the injected dependency.
- Returns domain `RedisSearchResult`, mapping from Redis-specific types internally.
- TODO comment documents known limitation: `//TODO: fix searching - will not work for Redis Cluster`.

### RedisServerRepository — Options Pattern Integration

```csharp
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web.Configuration;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisServerRepository : IRedisServerRepository
    {
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisServerRepository(IOptions<RedisConfiguration> redisConfiguration)
        {
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public IReadOnlyCollection<RedisServer> ListRedisServers()
        {
            RedisConfiguration configuration = _redisConfiguration.Value ?? throw new Exception("Configuration can't be null");
            var result = configuration.Servers.Select(e => new RedisServer(e.Name, e.Endpoints.ToList())).ToList();
            return result;
        }
    }
}
```

**Key observations:**
- Null-guard in constructor: `?? throw new ArgumentNullException(nameof(redisConfiguration))`.
- Double validation: constructor null-check + runtime `.Value` null-check.
- Maps configuration objects to domain entities using LINQ `.Select()`.
- Returns `List<T>` (implicitly cast to `IReadOnlyCollection<T>`).

## DI Registration (in Program.cs)

```csharp
builder.Services.AddSingleton<IRedisServerRepository, RedisServerRepository>();
```

- Registered as **singleton** (configuration doesn't change at runtime).
- `RedisRepository` is not yet registered in DI — it requires `IConnectionMultiplexer` which is not yet wired up (suggests this feature is still in development).

## NuGet Dependencies

```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.1" />
<PackageReference Include="Microsoft.Extensions.Options" Version="10.0.1" />
<PackageReference Include="StackExchange.Redis" Version="2.10.1" />
```

References the Domain project:
```xml
<ProjectReference Include="..\Roman.RedisManager.Domain\Roman.RedisManager.Domain.csproj" />
```
