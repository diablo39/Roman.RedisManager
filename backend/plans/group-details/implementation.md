# Redis Server Group Details Endpoint

## Goal
Deliver `GET api/redis-server-groups/{id}` that returns master/replica topology for both standalone and cluster Redis groups by extending the domain model, infrastructure connection caching, Wolverine handler, controller action, and tests.

## Prerequisites
Make sure that the user is currently on the `feature/redis-server-group-details-endpoint` branch before beginning implementation. If not, move to the correct branch. If the branch does not exist, create it from `main`.
Technology stack & dependencies for this feature: .NET 10, ASP.NET Core 10 Web API, Wolverine 5.9.2, StackExchange.Redis 2.10.1, Microsoft.Extensions.Options, xUnit 2.9.3, Shouldly 4.3.0.

### Step-by-Step Instructions

#### Step 1: Domain Layer - Entities and Repository Interface
- [x] Create the new Redis node entity by copying the code below into `src/Roman.RedisManager.Domain/Entities/RedisServerNode.cs`:

```csharp
using System;

namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServerNode
    {
        public string Host { get; internal set; }

        public int Port { get; internal set; }

        public string Role { get; internal set; }

        public RedisServerNode(string host, int port, string role)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));
            }

            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or whitespace.", nameof(role));
            }

            Host = host;
            Port = port;
            Role = role.ToLowerInvariant();
        }
    }
}
```

- [x] Update the Redis repository contract so it can return server nodes by copying the code below into `src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs`:

```csharp
using Roman.RedisManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        RedisSearchResult SearchForKeys(string predicate);

        Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId);
    }
}
```

##### Step 1 Verification Checklist
- [ ] Run `dotnet build src/Roman.RedisManager.Domain/Roman.RedisManager.Domain.csproj` (should succeed without warnings).
- [ ] Confirm no nullable or analyzer warnings appear for the new entity and interface.

#### Step 1 STOP & COMMIT
**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 2: Infrastructure Layer - Redis Connection Manager & DI
- [ ] Add the new connection manager contract by copying the code below into `src/Roman.RedisManager.Infrastructure/Redis/IRedisConnectionManager.cs`:

```csharp
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Roman.RedisManager.Infrastructure.Redis
{
    public interface IRedisConnectionManager : IAsyncDisposable
    {
        Task<IConnectionMultiplexer> GetConnectionAsync(string groupId);
    }
}
```

- [ ] Implement the lazy, per-group connection cache by copying the code below into `src/Roman.RedisManager.Infrastructure/Redis/RedisConnectionManager.cs`:

```csharp
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Infrastructure.Redis
{
    public sealed class RedisConnectionManager : IRedisConnectionManager
    {
        private readonly IOptions<RedisConfiguration> _redisConfiguration;
        private readonly ConcurrentDictionary<string, Lazy<Task<IConnectionMultiplexer>>> _connections = new();

        public RedisConnectionManager(IOptions<RedisConfiguration> redisConfiguration)
        {
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task<IConnectionMultiplexer> GetConnectionAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("Group identifier cannot be null or whitespace.", nameof(groupId));
            }

            var serverGroup = ResolveGroup(groupId);
            var connectionFactory = new Lazy<Task<IConnectionMultiplexer>>(() => CreateConnectionAsync(serverGroup));

            var lazyConnection = _connections.GetOrAdd(groupId, _ => connectionFactory);

            try
            {
                return await lazyConnection.Value.ConfigureAwait(false);
            }
            catch
            {
                _connections.TryRemove(groupId, out _);
                throw;
            }
        }

        private RedisServerGroupConfiguration ResolveGroup(string groupId)
        {
            var configuration = _redisConfiguration.Value ?? throw new InvalidOperationException("Redis configuration is not available.");

            var serverGroup = configuration.ServerGroups.FirstOrDefault(group =>
                group.Name.ToMd5Hash().Equals(groupId, StringComparison.OrdinalIgnoreCase));

            if (serverGroup is null)
            {
                throw new KeyNotFoundException($"Redis server group with id '{groupId}' was not found.");
            }

            return serverGroup;
        }

        private static Task<IConnectionMultiplexer> CreateConnectionAsync(RedisServerGroupConfiguration serverGroup)
        {
            if (serverGroup is null)
            {
                throw new ArgumentNullException(nameof(serverGroup));
            }

            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                AllowAdmin = true,
                ConnectRetry = 3,
                KeepAlive = 5,
                ResolveDns = true
            };

            foreach (var endpoint in serverGroup.Endpoints)
            {
                options.EndPoints.Add(endpoint);
            }

            if (serverGroup.GroupType == GroupType.Cluster)
            {
                options.TieBreaker = string.Empty;
            }

            return ConnectionMultiplexer.ConnectAsync(options);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var lazyConnection in _connections.Values)
            {
                if (!lazyConnection.IsValueCreated)
                {
                    continue;
                }

                var connection = await lazyConnection.Value.ConfigureAwait(false);
                connection.Dispose();
            }

            _connections.Clear();
        }
    }
}
```

- [ ] Register the new services in DI by copying the code below into `src/Roman.RedisManager.Web/Program.cs` (this replaces the entire file to keep the order intact):

```csharp
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using Wolverine;

namespace Roman.RedisManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddOptions<RedisConfiguration>()
                .Bind(builder.Configuration.GetSection(RedisConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();
            builder.Services.AddSingleton<IRedisServerGroupRepository, RedisServerGroupRepository>();
            builder.Services.AddSingleton<IRedisRepository, RedisRepository>();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
```

##### Step 2 Verification Checklist
- [ ] Run `dotnet build Roman.RedisManager.slnx` (ensures DI registrations compile across the solution).
- [ ] Confirm the application starts without DI errors by running `dotnet run --project src/Roman.RedisManager.Web` and stopping after successful startup.

#### Step 2 STOP & COMMIT
**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 3: Infrastructure Layer - Repository Implementation & Exception
- [ ] Add the custom Redis connection exception by copying the code below into `src/Roman.RedisManager.Infrastructure/Exceptions/RedisConnectionFailureException.cs`:

```csharp
using System;

namespace Roman.RedisManager.Infrastructure.Exceptions
{
    public class RedisConnectionFailureException : Exception
    {
        public RedisConnectionFailureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
```

- [ ] Update the Redis repository to use the connection manager and expose server nodes by copying the code below into `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`:

```csharp
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisconfiguration));
        }

        public RedisSearchResult SearchForKeys(string predicate)
        {
            var defaultGroup = _redisConfiguration.Value.ServerGroups.FirstOrDefault()
                ?? throw new InvalidOperationException("No Redis server groups are configured.");

            var connection = _connectionManager.GetConnectionAsync(defaultGroup.Name.ToMd5Hash()).GetAwaiter().GetResult();
            var endpoint = connection.GetEndPoints().FirstOrDefault()
                ?? throw new InvalidOperationException("No Redis endpoints are available for the configured server group.");

            var server = connection.GetServer(endpoint);
            var searchResult = server.Keys(pattern: predicate);
            var cursor = (IScanningCursor)searchResult;
            var keys = searchResult.Select(e => new RedisKey(e.ToString())).ToList();

            return new RedisSearchResult(keys, cursor.Cursor);
        }

        public async Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            var serverGroup = ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var server = GetServer(connection);

                var nodes = serverGroup.GroupType == GroupType.Cluster
                    ? await ReadClusterNodesAsync(server).ConfigureAwait(false)
                    : await ReadStandaloneNodesAsync(server).ConfigureAwait(false);

                return nodes
                    .GroupBy(node => new { node.Host, node.Port, node.Role })
                    .Select(group => group.First())
                    .ToList();
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to connect to the Redis topology for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while retrieving Redis server nodes.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while retrieving server nodes.", ex);
            }
        }

        private RedisServerGroupConfiguration ResolveServerGroup(string groupId)
        {
            var configuration = _redisConfiguration.Value ?? throw new InvalidOperationException("Redis configuration is unavailable.");

            var serverGroup = configuration.ServerGroups.FirstOrDefault(group =>
                group.Name.ToMd5Hash().Equals(groupId, StringComparison.OrdinalIgnoreCase));

            if (serverGroup is null)
            {
                throw new KeyNotFoundException($"Redis server group with id '{groupId}' was not found.");
            }

            return serverGroup;
        }

        private static IServer GetServer(IConnectionMultiplexer connection)
        {
            var endpoint = connection.GetEndPoints().FirstOrDefault()
                ?? throw new InvalidOperationException("Redis connection does not expose endpoints.");

            return connection.GetServer(endpoint);
        }

        private static async Task<List<RedisServerNode>> ReadStandaloneNodesAsync(IServer server)
        {
            var nodes = new List<RedisServerNode>();
            var (host, port) = ResolveEndpoint(server.EndPoint);
            nodes.Add(new RedisServerNode(host, port, "master"));

            var sections = await server.InfoAsync("replication").ConfigureAwait(false);
            var replicationSection = sections.FirstOrDefault();

            if (replicationSection is null)
            {
                return nodes;
            }

            foreach (var entry in replicationSection.Where(kvp => kvp.Key.StartsWith("slave", StringComparison.OrdinalIgnoreCase)))
            {
                var attributes = entry.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var ipValue = attributes.FirstOrDefault(value => value.StartsWith("ip=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
                var portValue = attributes.FirstOrDefault(value => value.StartsWith("port=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];

                if (string.IsNullOrWhiteSpace(ipValue) ||
                    string.IsNullOrWhiteSpace(portValue) ||
                    !int.TryParse(portValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort))
                {
                    continue;
                }

                nodes.Add(new RedisServerNode(ipValue, parsedPort, "slave"));
            }

            return nodes;
        }

        private static async Task<List<RedisServerNode>> ReadClusterNodesAsync(IServer server)
        {
            var clusterNodes = await server.ClusterNodesAsync().ConfigureAwait(false);

            return clusterNodes
                .Select(node =>
                {
                    var (host, port) = ResolveEndpoint(node.EndPoint);
                    var role = node.IsReplica ? "slave" : "master";
                    return new RedisServerNode(host, port, role);
                })
                .ToList();
        }

        private static (string Host, int Port) ResolveEndpoint(EndPoint endpoint)
        {
            return endpoint switch
            {
                DnsEndPoint dnsEndPoint => (dnsEndPoint.Host, dnsEndPoint.Port),
                IPEndPoint ipEndPoint => (ipEndPoint.Address.ToString(), ipEndPoint.Port),
                _ => (endpoint.ToString() ?? string.Empty, 0)
            };
        }
    }
}
```

##### Step 3 Verification Checklist
- [ ] Run `dotnet build Roman.RedisManager.slnx` and ensure `RedisRepository` compiles with the new dependencies.
- [ ] (Optional) Attach a debugger to `dotnet run --project src/Roman.RedisManager.Web` and verify that `RedisConnectionManager` resolves without throwing to validate configuration wiring.

#### Step 3 STOP & COMMIT
**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 4: Application & Web Layers - CQRS Handler and Controller
- [ ] Add the Wolverine handler by copying the code below into `src/Roman.RedisManager.Application/CQRS/RedisServerGroupDetailQueryHandler.cs`:

```csharp
using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServerGroupDetailQuery
    {
        public string Id { get; set; } = string.Empty;
    }

    public record RedisServerNodeDto(string Host, int Port, string Role);

    public record RedisServerGroupDetailQueryResult(IReadOnlyCollection<RedisServerNodeDto> Nodes);

    public static class RedisServerGroupDetailQueryHandler
    {
        public static async Task<RedisServerGroupDetailQueryResult> Handle(RedisServerGroupDetailQuery query, IRedisRepository repository)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var nodes = await repository.GetServerNodesAsync(query.Id).ConfigureAwait(false);

            var dto = nodes
                .Select(node => new RedisServerNodeDto(node.Host, node.Port, node.Role))
                .ToList();

            return new RedisServerGroupDetailQueryResult(dto);
        }
    }
}
```

- [ ] Extend the controller to expose the new endpoint with explicit HTTP status handling by copying the code below into `src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Web.Wolverine;
using System.Collections.Generic;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    [Route("api/redis-server-groups")]
    [ApiController]
    public class RedisServerGroupsController : ControllerBase
    {
        private readonly IMessageBus _bus;

        public RedisServerGroupsController(IMessageBus bus) => _bus = bus;

        [HttpGet]
        public async Task<RedisServerGroupsQueryResult> GetServerGroups([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await _bus.InvokeAsync<RedisServerGroupsQueryResult>(
                new RedisServerGroupsQuery { PageNumber = pageNumber, PageSize = pageSize });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RedisServerGroupDetailQueryResult>> GetServerGroupDetail(string id)
        {
            try
            {
                var result = await _bus.InvokeAsync<RedisServerGroupDetailQueryResult>(
                    new RedisServerGroupDetailQuery { Id = id });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisConnectionFailureException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
```

##### Step 4 Verification Checklist
- [ ] Run `dotnet build src/Roman.RedisManager.Application/Roman.RedisManager.Application.csproj` and `dotnet build src/Roman.RedisManager.Web/Roman.RedisManager.Web.csproj` to ensure the handler and controller compile.
- [ ] Execute `dotnet run --project src/Roman.RedisManager.Web` and hit `GET /api/redis-server-groups/{id}` (using a valid MD5 id) to confirm 200 OK responses, 404 on bad ids, and 500 on simulated connection failures.

#### Step 4 STOP & COMMIT
**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 5: Testing and HTTP Documentation
- [ ] Expand the repository tests (still skipped until a Redis topology is available) by copying the code below into `tests/Roman.RedisManager.Tests/RedisRepositoryTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact(Skip = "Requires local Redis instance")]
        public async Task SearchForKeys_ShouldReturnNonNullAndNonEmptyKeys()
        {
            var options = BuildOptions(
                new RedisServerGroupConfiguration
                {
                    Name = "standalone-local",
                    Endpoints = new[] { "localhost:6379" },
                    GroupType = GroupType.Standalone
                });

            await using IRedisConnectionManager connectionManager = new RedisConnectionManager(options);
            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);

            var result = redisRepository.SearchForKeys(string.Empty);

            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }

        [Fact(Skip = "Requires standalone Redis primary with replicas configured.")]
        public async Task GetServerNodesAsync_ValidStandaloneGroup_ReturnsMasterAndSlaves()
        {
            var options = BuildOptions(
                new RedisServerGroupConfiguration
                {
                    Name = "standalone-local",
                    Endpoints = new[] { "localhost:6379" },
                    GroupType = GroupType.Standalone
                });

            var groupId = options.Value.ServerGroups.First().Name.ToMd5Hash();

            await using IRedisConnectionManager connectionManager = new RedisConnectionManager(options);
            IRedisRepository repository = new RedisRepository(connectionManager, options);

            var nodes = await repository.GetServerNodesAsync(groupId);

            nodes.ShouldContain(node => node.Role == "master");
            nodes.ShouldContain(node => node.Role == "slave");
        }

        [Fact(Skip = "Requires Redis cluster with master/replica nodes configured.")]
        public async Task GetServerNodesAsync_ValidClusterGroup_ReturnsAllClusterNodes()
        {
            var options = BuildOptions(
                new RedisServerGroupConfiguration
                {
                    Name = "cluster-local",
                    Endpoints = new[] { "localhost:7000", "localhost:7001", "localhost:7002" },
                    GroupType = GroupType.Cluster
                });

            var groupId = options.Value.ServerGroups.First().Name.ToMd5Hash();

            await using IRedisConnectionManager connectionManager = new RedisConnectionManager(options);
            IRedisRepository repository = new RedisRepository(connectionManager, options);

            var nodes = await repository.GetServerNodesAsync(groupId);

            nodes.ShouldContain(node => node.Role == "master");
            nodes.ShouldContain(node => node.Role == "slave");
        }

        [Fact]
        public async Task GetServerNodesAsync_InvalidGroupId_ThrowsKeyNotFoundException()
        {
            var options = BuildOptions(
                new RedisServerGroupConfiguration
                {
                    Name = "placeholder",
                    Endpoints = new[] { "localhost:6379" },
                    GroupType = GroupType.Standalone
                });

            await using IRedisConnectionManager connectionManager = new RedisConnectionManager(options);
            IRedisRepository repository = new RedisRepository(connectionManager, options);

            await Should.ThrowAsync<KeyNotFoundException>(() => repository.GetServerNodesAsync("missing-group-id"));
        }

        private static IOptions<RedisConfiguration> BuildOptions(params RedisServerGroupConfiguration[] groups)
        {
            return Options.Create(new RedisConfiguration
            {
                ServerGroups = groups
            });
        }
    }
}
```

- [ ] Add integration-style coverage for the handler by copying the code below into `tests/Roman.RedisManager.Tests/RedisServerGroupDetailQueryHandlerTests.cs`:

```csharp
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web.Wolverine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests
{
    public class RedisServerGroupDetailQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingGroup_ReturnsNodes()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<string, IReadOnlyCollection<RedisServerNode>>
            {
                ["group-a"] = new List<RedisServerNode>
                {
                    new RedisServerNode("127.0.0.1", 6379, "master"),
                    new RedisServerNode("127.0.0.1", 6380, "slave")
                }
            });

            var query = new RedisServerGroupDetailQuery { Id = "group-a" };

            var result = await RedisServerGroupDetailQueryHandler.Handle(query, repository);

            result.Nodes.ShouldNotBeNull();
            result.Nodes.Count.ShouldBe(2);
            result.Nodes.ShouldContain(node => node.Role == "master");
            result.Nodes.ShouldContain(node => node.Role == "slave");
        }

        [Fact]
        public async Task Handle_MissingGroup_ThrowsKeyNotFoundException()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<string, IReadOnlyCollection<RedisServerNode>>());
            var query = new RedisServerGroupDetailQuery { Id = "missing" };

            await Should.ThrowAsync<KeyNotFoundException>(() => RedisServerGroupDetailQueryHandler.Handle(query, repository));
        }

        private sealed class InMemoryRedisRepository : IRedisRepository
        {
            private readonly IReadOnlyDictionary<string, IReadOnlyCollection<RedisServerNode>> _nodes;

            public InMemoryRedisRepository(IReadOnlyDictionary<string, IReadOnlyCollection<RedisServerNode>> nodes)
            {
                _nodes = nodes;
            }

            public RedisSearchResult SearchForKeys(string predicate)
            {
                return new RedisSearchResult(Array.Empty<RedisKey>(), 0);
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId)
            {
                if (!_nodes.TryGetValue(groupId, out var nodes))
                {
                    throw new KeyNotFoundException($"Group '{groupId}' was not found.");
                }

                return Task.FromResult(nodes);
            }
        }
    }
}
```

- [ ] Document the HTTP request by copying the code below into `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` (replace the placeholder MD5 id before executing):

```http
@Roman.RedisManager.Web_HostAddress = https://localhost:7244
@RedisServerGroupId = 00000000000000000000000000000000

GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-server-groups

###

GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-server-groups/{{RedisServerGroupId}}

###
```

##### Step 5 Verification Checklist
- [ ] Run `dotnet test Roman.RedisManager.slnx` (all non-skipped tests must pass; skipped tests remain skipped until a Redis topology is available).
- [ ] Launch `dotnet run --project src/Roman.RedisManager.Web`, update `@RedisServerGroupId` with a real MD5 id, and issue the new request from `Roman.RedisManager.Web.http` to confirm 200 responses; verify 404 for invalid ids and observe 500 if Redis is unreachable.

#### Step 5 STOP & COMMIT
**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.
