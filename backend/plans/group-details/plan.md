# Redis Server Group Details Endpoint

**Branch:** `feature/redis-server-group-details-endpoint`
**Description:** Add GET endpoint to retrieve server topology (master/slave nodes) for Redis server groups

## Goal
Implement `GET api/redis-server-groups/{id}` endpoint that returns a distinct list of Redis server nodes with their role (master/slave), host, and port. For standalone groups, query Redis INFO to discover slave replicas. For cluster groups, query cluster topology to get all nodes (masters and slaves).

## Implementation Steps

### Step 1: Domain Layer - Entities and Repository Interface
**Files:**
- [src/Roman.RedisManager.Domain/Entities/RedisServerNode.cs](src/Roman.RedisManager.Domain/Entities/RedisServerNode.cs) (new)
- [src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs](src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs) (modify)

**What:** Create `RedisServerNode` entity to represent individual Redis server with `Host` (string), `Port` (int), and `Role` (string: "master" or "slave"). Add `GetServerNodesAsync(string groupId)` method to `IRedisRepository` interface that returns `Task<IReadOnlyCollection<RedisServerNode>>`.

**Testing:** Compile Domain project to verify entity structure and interface signature.

---

### Step 2: Infrastructure Layer - Redis Connection Manager
**Files:**
- [src/Roman.RedisManager.Infrastructure/Redis/IRedisConnectionManager.cs](src/Roman.RedisManager.Infrastructure/Redis/IRedisConnectionManager.cs) (new)
- [src/Roman.RedisManager.Infrastructure/Redis/RedisConnectionManager.cs](src/Roman.RedisManager.Infrastructure/Redis/RedisConnectionManager.cs) (new)
- [src/Roman.RedisManager.Web/Program.cs](src/Roman.RedisManager.Web/Program.cs) (modify)

**What:** Create connection manager that lazily creates and caches `IConnectionMultiplexer` instances per server group ID (using MD5 hash of group name). Manager uses `IOptions<RedisConfiguration>` to get endpoints and group type. Register `IRedisConnectionManager` as singleton in DI container.

**Testing:** Run application with debugger to verify connection manager is registered and instantiated without errors.

---

### Step 3: Infrastructure Layer - Repository Implementation
**Files:**
- [src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs](src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs) (modify)

**What:** Implement `GetServerNodesAsync` method in `RedisRepository`:
- Use `IRedisConnectionManager.GetConnectionAsync(groupId)` to get multiplexer
- Throw `KeyNotFoundException` if group ID invalid (for 404)
- Catch StackExchange.Redis connection exceptions and wrap in custom exception (for 500)
- For **Standalone**: Query `server.InfoAsync("replication")` to get master role, parse `slave0`, `slave1` entries to extract slave IP and port as integers
- For **Cluster**: Use `server.ClusterNodesAsync()` to get all cluster nodes with roles (masters and slaves)
- Role values must be exactly `"master"` or `"slave"` (lowercase strings)
- Return distinct list of `RedisServerNode` instances

**Testing:** Write unit test `GetServerNodesAsync_WithValidStandaloneGroupId_ReturnsMasterAndSlaves()` in RedisRepositoryTests.cs to verify method logic.

---

### Step 4: Application & Web Layers - CQRS Handler and Controller
**Files:**
- [src/Roman.RedisManager.Application/CQRS/RedisServerGroupDetailQueryHandler.cs](src/Roman.RedisManager.Application/CQRS/RedisServerGroupDetailQueryHandler.cs) (new)
- [src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs](src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs) (modify)

**What:** 
- Create `RedisServerGroupDetailQuery` class with `Id` (string) property
- Create `RedisServerNodeDto` record with `Host` (string), `Port` (int), `Role` (string) properties  
- Create `RedisServerGroupDetailQueryResult` record containing `IEnumerable<RedisServerNodeDto> Nodes`
- Create static handler class `RedisServerGroupDetailQueryHandler` with async `HandleAsync` method in namespace `Roman.RedisManager.Web.Wolverine`
- Handler calls `IRedisRepository.GetServerNodesAsync(query.Id)` and maps to DTOs using LINQ `.Select()`
- Handle `KeyNotFoundException` → return 404 Not Found
- Handle Redis exceptions → return 500 Internal Server Error
- Add `[HttpGet("{id}")]` action to [RedisServerGroupsController](src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs) that dispatches query via `IMessageBus.InvokeAsync<RedisServerGroupDetailQueryResult>()`

**Testing:** Run application and test endpoint with valid ID (MD5 hash of configured group name) using Swagger UI or manual HTTP request.

---

### Step 5: Testing and Documentation
**Files:**
- [tests/Roman.RedisManager.Tests/RedisRepositoryTests.cs](tests/Roman.RedisManager.Tests/RedisRepositoryTests.cs) (modify)
- [tests/Roman.RedisManager.Tests/RedisServerGroupDetailQueryHandlerTests.cs](tests/Roman.RedisManager.Tests/RedisServerGroupDetailQueryHandlerTests.cs) (new)
- [src/Roman.RedisManager.Web/Roman.RedisManager.Web.http](src/Roman.RedisManager.Web/Roman.RedisManager.Web.http) (modify)

**What:** 
- Add unit tests for repository method following `MethodName_Condition_ExpectedResult` pattern:
  - `GetServerNodesAsync_ValidStandaloneGroup_ReturnsMasterAndSlaves()`
  - `GetServerNodesAsync_ValidClusterGroup_ReturnsAllClusterNodes()`
  - `GetServerNodesAsync_InvalidGroupId_ThrowsKeyNotFoundException()`
- Add integration test for handler
- Add HTTP request to `.http` file: `GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-server-groups/{id}`
- Verify port is returned as integer (not string)
- Verify role is returned as "master" or "slave" (lowercase)

**Testing:** 
- Run `dotnet test` to verify all tests pass
- Execute HTTP request against running application to verify end-to-end flow with real Redis

---

## Verification

**Manual Testing:**
1. Start application: `dotnet run --project src/Roman.RedisManager.Web`
2. Get server groups: `GET http://localhost:<port>/api/redis-server-groups` → Copy ID from response
3. Get group details: `GET http://localhost:<port>/api/redis-server-groups/{id}` → Verify response matches format:
   ```json
   [
     {
       "host": "127.0.0.1",
       "port": 6379,
       "role": "master"
     },
     {
       "host": "127.0.0.1",
       "port": 6380,
       "role": "slave"
     }
   ]