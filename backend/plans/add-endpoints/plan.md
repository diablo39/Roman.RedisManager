# Plan: Redis Value CRUD Endpoints

## Summary

Add full read/write/delete support for all 5 core Redis data types (String, List, Set, Hash, Sorted Set), plus generic key operations (TYPE, TTL, DELETE, auto-detect value). The feature is split into **3 incremental phases**. Each phase adds new repository interfaces + implementations, CQRS command/query handlers, controllers, and tests — following all existing conventions (static Wolverine handlers, block-scoped namespaces, Shouldly assertions, Testcontainers integration tests, etc.).

All write endpoints accept optional TTL. Bulk operations are supported where applicable. Element-removal operations use `POST .../remove` to avoid non-standard DELETE-with-body.

---

## Decisions

| Decision | Chosen | Alternative Considered |
|----------|--------|----------------------|
| Data types | String, List, Set, Hash, Sorted Set | Streams (deferred) |
| Scope | Full CRUD (read + write + delete) | Write-only |
| TTL | Optional on all write endpoints | No TTL / separate endpoint |
| Bulk operations | Supported (multiple values per request) | Single value only |
| String SET conditions | NX / XX flags supported | Plain SET only |
| List push direction | LPUSH / RPUSH via parameter | RPUSH only |
| Collection pagination | Cursor-based (SSCAN, HSCAN) from the start | Return all elements |
| Generic key value read | Yes — auto-detect type and return value | Separate reads per type only |
| Return state after mutation | No — write endpoints return success/failure only | Return new length/cardinality |
| Controller organization | One controller per data type | Single controller |
| Key DELETE location | Generic DELETE on existing `RedisKeysController` | Per-type DELETE |
| Element removal | `POST .../remove` actions | DELETE with body |
| Phasing | 3 phases (String+Key → List+Set → Hash+SortedSet) | All at once |
| Exception for WRONGTYPE | `RedisTypeMismatchException` → 409 Conflict | Let Redis error propagate as 500 |

---

## API Surface

### Phase 1 — String + Generic Key Operations

| Method | Route | Params / Body | Redis Command(s) | Success | Errors |
|--------|-------|---------------|-------------------|---------|--------|
| `POST` | `api/redis-strings` | Body: `{ groupId, key, value, ttl?, condition? }` | `SET` / `SET NX` / `SET XX` | 200 | 400, 404, 409, 500 |
| `GET` | `api/redis-strings` | `?groupId=...&key=...` | `GET` | 200 | 400, 404, 409, 500 |
| `DELETE` | `api/redis-keys/{key}` | `?groupId=...` | `DEL` | 200 | 400, 404, 500 |
| `GET` | `api/redis-keys/{key}/metadata` | `?groupId=...` | `TYPE` + `PTTL` | 200 | 400, 404, 500 |
| `GET` | `api/redis-keys/{key}/value` | `?groupId=...` | `TYPE` → `GET`/`LRANGE`/`SMEMBERS`/`HGETALL`/`ZRANGE` | 200 | 400, 404, 500 |

### Phase 2 — List + Set

| Method | Route | Params / Body | Redis Command(s) |
|--------|-------|---------------|-------------------|
| `POST` | `api/redis-lists` | Body: `{ groupId, key, values[], direction, ttl? }` | `LPUSH` / `RPUSH` |
| `GET` | `api/redis-lists` | `?groupId=...&key=...&start=0&stop=-1` | `LRANGE` |
| `POST` | `api/redis-lists/remove` | Body: `{ groupId, key, value, count }` | `LREM` |
| `POST` | `api/redis-sets` | Body: `{ groupId, key, members[], ttl? }` | `SADD` |
| `GET` | `api/redis-sets` | `?groupId=...&key=...&cursor=0&pageSize=100` | `SSCAN` |
| `POST` | `api/redis-sets/remove` | Body: `{ groupId, key, members[] }` | `SREM` |

### Phase 3 — Hash + Sorted Set

| Method | Route | Params / Body | Redis Command(s) |
|--------|-------|---------------|-------------------|
| `POST` | `api/redis-hashes` | Body: `{ groupId, key, fields[{field,value}], ttl? }` | `HSET` |
| `GET` | `api/redis-hashes` | `?groupId=...&key=...&cursor=0&pageSize=100` | `HSCAN` |
| `POST` | `api/redis-hashes/remove` | Body: `{ groupId, key, fields[] }` | `HDEL` |
| `POST` | `api/redis-sorted-sets` | Body: `{ groupId, key, entries[{member,score}], ttl? }` | `ZADD` |
| `GET` | `api/redis-sorted-sets` | `?groupId=...&key=...&start=0&stop=-1&ascending=true` | `ZRANGE` / `ZREVRANGE` |
| `POST` | `api/redis-sorted-sets/remove` | Body: `{ groupId, key, members[] }` | `ZREM` |

---

## Architecture

### New Domain Enums — `src/Roman.RedisManager.Domain/Entities/`

- **`RedisDataType.cs`** — enum: `None`, `String`, `List`, `Set`, `Hash`, `SortedSet`, `Stream`, `Unknown`
  - Mirrors `StackExchange.Redis.RedisType` but is a domain concept with zero dependencies.
- **`SetCondition.cs`** — enum: `None`, `NotExists`, `Exists`
  - Maps to `StackExchange.Redis.When.Always` / `When.NotExists` / `When.Exists` at the infrastructure layer.
- **`ListDirection.cs`** — enum: `Left`, `Right`
  - Controls LPUSH vs RPUSH.

### New Domain Entities/Records — `src/Roman.RedisManager.Domain/Entities/`

- **`RedisKeyMetadata.cs`** — class with `RedisDataType Type` and `TimeSpan? Ttl` properties (block-scoped namespace, `internal set`).
- **`RedisSortedSetEntry.cs`** — class with `string Member` and `double Score` properties.
- **`RedisKeyValue.cs`** — class representing a generic key's value. Properties: `RedisDataType Type`, `string? StringValue`, `IEnumerable<string>? ListValues`, `IEnumerable<string>? SetMembers`, `IReadOnlyDictionary<string, string>? HashFields`, `IEnumerable<RedisSortedSetEntry>? SortedSetEntries`.
- **`RedisHashEntry.cs`** — class with `string Field` and `string Value` properties.
- **`RedisScanResult.cs`** — generic class for cursor-based scan results: `long Cursor`, `IEnumerable<T> Items`, computed `bool HasMoreResults`.

### New Repository Interfaces — `src/Roman.RedisManager.Domain/Repositories/`

Following ISP, each data type gets its own interface. All methods are async (`Task<T>`).

- **`IRedisKeyRepository.cs`**
  - `Task<bool> DeleteKeyAsync(Guid groupId, string key)`
  - `Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key)`
  - `Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key)`

- **`IRedisStringRepository.cs`**
  - `Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition)`
  - `Task<string?> StringGetAsync(Guid groupId, string key)`

- **`IRedisListRepository.cs`**
  - `Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl)`
  - `Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop)`
  - `Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count)`

- **`IRedisSetRepository.cs`**
  - `Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl)`
  - `Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize)`
  - `Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members)`

- **`IRedisHashRepository.cs`**
  - `Task HashSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisHashEntry> fields, TimeSpan? ttl)`
  - `Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize)`
  - `Task<long> HashDeleteFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields)`

- **`IRedisSortedSetRepository.cs`**
  - `Task SortedSetAddAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl)`
  - `Task<IReadOnlyCollection<RedisSortedSetEntry>> SortedSetRangeAsync(Guid groupId, string key, long start, long stop, bool ascending)`
  - `Task<long> SortedSetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members)`

### New Repository Implementations — `src/Roman.RedisManager.Infrastructure/Repositories/`

Each interface gets a dedicated implementation class. All follow the existing pattern:
- Constructor injects `IRedisConnectionManager` + `IOptions<RedisConfiguration>`, stored as `private readonly` `_camelCase` fields.
- Input validation: `Guid.Empty` check, null/empty key check, range checks — throw `ArgumentException` / `ArgumentNullException`.
- Resolve group via `_redisConfiguration.Value.ResolveServerGroup(groupId)`.
- Get connection via `_connectionManager.GetConnectionAsync(groupId)`.
- Catch `RedisConnectionException`, `RedisTimeoutException`, `RedisServerException` → wrap in `RedisConnectionFailureException`.
- Catch `RedisServerException` containing "WRONGTYPE" → wrap in new `RedisTypeMismatchException`.

Implementation files:
- **`RedisKeyRepository.cs`** — implements `IRedisKeyRepository`
- **`RedisStringRepository.cs`** — implements `IRedisStringRepository`
- **`RedisListRepository.cs`** — implements `IRedisListRepository`
- **`RedisSetRepository.cs`** — implements `IRedisSetRepository`
- **`RedisHashRepository.cs`** — implements `IRedisHashRepository`
- **`RedisSortedSetRepository.cs`** — implements `IRedisSortedSetRepository`

### New Exception — `src/Roman.RedisManager.Infrastructure/Exceptions/`

- **`RedisTypeMismatchException.cs`** — primary constructor: `(string message, Exception innerException) : Exception(message, innerException)`. Thrown when a Redis WRONGTYPE error is detected. Controllers map this to HTTP 409 Conflict.

### CQRS Handlers — `src/Roman.RedisManager.Application/CQRS/`

Each handler file co-locates: command/query class, DTO record(s), result record, static handler class. All use block-scoped namespace `Roman.RedisManager.Application.CQRS`.

**Phase 1 handlers (5 files):**

1. **`SetStringCommandHandler.cs`**
   - `SetStringCommand` class: `Guid GroupId`, `string Key`, `string Value`, `TimeSpan? Ttl`, `SetCondition Condition` (all `{ get; set; }`)
   - `SetStringCommandResult` record: `bool Success`
   - `SetStringCommandHandler` static class → static `HandleAsync(SetStringCommand command, IRedisStringRepository repository)`

2. **`GetStringQueryHandler.cs`**
   - `GetStringQuery` class: `Guid GroupId`, `string Key`
   - `GetStringQueryResult` record: `string? Value`
   - `GetStringQueryHandler` static class → static `HandleAsync(GetStringQuery query, IRedisStringRepository repository)`

3. **`DeleteKeyCommandHandler.cs`**
   - `DeleteKeyCommand` class: `Guid GroupId`, `string Key`
   - `DeleteKeyCommandResult` record: `bool Deleted`
   - `DeleteKeyCommandHandler` static class → static `HandleAsync(DeleteKeyCommand command, IRedisKeyRepository repository)`

4. **`GetKeyMetadataQueryHandler.cs`**
   - `GetKeyMetadataQuery` class: `Guid GroupId`, `string Key`
   - `RedisKeyMetadataDto` record: `string Type`, `long? TtlMilliseconds`
   - `GetKeyMetadataQueryResult` record: `RedisKeyMetadataDto Metadata`
   - `GetKeyMetadataQueryHandler` static class → static `HandleAsync(GetKeyMetadataQuery query, IRedisKeyRepository repository)`

5. **`GetKeyValueQueryHandler.cs`**
   - `GetKeyValueQuery` class: `Guid GroupId`, `string Key`
   - Multiple DTO records for each type's value representation
   - `GetKeyValueQueryResult` record with the type-discriminated value
   - `GetKeyValueQueryHandler` static class → static `HandleAsync(GetKeyValueQuery query, IRedisKeyRepository repository)`

**Phase 2 handlers (6 files):**

6. **`PushToListCommandHandler.cs`** — `PushToListCommand` (groupId, key, values[], direction, ttl?), `PushToListCommandHandler`
7. **`GetListRangeQueryHandler.cs`** — `GetListRangeQuery` (groupId, key, start, stop), result with `IReadOnlyCollection<string> Values`
8. **`RemoveFromListCommandHandler.cs`** — `RemoveFromListCommand` (groupId, key, value, count), result with `long RemovedCount`
9. **`AddToSetCommandHandler.cs`** — `AddToSetCommand` (groupId, key, members[], ttl?), `AddToSetCommandHandler`
10. **`GetSetMembersQueryHandler.cs`** — `GetSetMembersQuery` (groupId, key, cursor, pageSize), result with `IReadOnlyCollection<string> Members`, `long Cursor`, `bool HasMoreResults`
11. **`RemoveFromSetCommandHandler.cs`** — `RemoveFromSetCommand` (groupId, key, members[]), result with `long RemovedCount`

**Phase 3 handlers (6 files):**

12. **`SetHashFieldsCommandHandler.cs`** — `SetHashFieldsCommand` (groupId, key, fields[{field,value}], ttl?), `SetHashFieldsCommandHandler`
13. **`GetHashFieldsQueryHandler.cs`** — `GetHashFieldsQuery` (groupId, key, cursor, pageSize), result with `IReadOnlyCollection<HashFieldDto> Fields`, `long Cursor`, `bool HasMoreResults`
14. **`RemoveHashFieldsCommandHandler.cs`** — `RemoveHashFieldsCommand` (groupId, key, fields[]), result with `long RemovedCount`
15. **`AddToSortedSetCommandHandler.cs`** — `AddToSortedSetCommand` (groupId, key, entries[{member,score}], ttl?), `AddToSortedSetCommandHandler`
16. **`GetSortedSetRangeQueryHandler.cs`** — `GetSortedSetRangeQuery` (groupId, key, start, stop, ascending), result with `IReadOnlyCollection<SortedSetEntryDto> Entries`
17. **`RemoveFromSortedSetCommandHandler.cs`** — `RemoveFromSortedSetCommand` (groupId, key, members[]), result with `long RemovedCount`

### Controllers — `src/Roman.RedisManager.Web/Controllers/`

All new controllers follow existing conventions:
- `[ApiController]`, `[Route("api/{kebab-case}")]`
- Inject `IMessageBus` only — expression-bodied constructor
- `ProducesResponseType` attributes for all status codes
- `try/catch` for `KeyNotFoundException` → 404, `RedisConnectionFailureException` → 500, `ArgumentException` → 400, `RedisTypeMismatchException` → 409

**Phase 1:**
- **`RedisStringsController.cs`** — `[Route("api/redis-strings")]` — `[HttpPost]` Set, `[HttpGet]` Get
- Add to existing **`RedisKeysController.cs`** — `[HttpDelete("{key}")]` Delete, `[HttpGet("{key}/metadata")]` GetMetadata, `[HttpGet("{key}/value")]` GetValue

**Phase 2:**
- **`RedisListsController.cs`** — `[Route("api/redis-lists")]` — `[HttpPost]` Push, `[HttpGet]` Range, `[HttpPost("remove")]` Remove
- **`RedisSetsController.cs`** — `[Route("api/redis-sets")]` — `[HttpPost]` Add, `[HttpGet]` Scan, `[HttpPost("remove")]` Remove

**Phase 3:**
- **`RedisHashesController.cs`** — `[Route("api/redis-hashes")]` — `[HttpPost]` Set, `[HttpGet]` Scan, `[HttpPost("remove")]` Remove
- **`RedisSortedSetsController.cs`** — `[Route("api/redis-sorted-sets")]` — `[HttpPost]` Add, `[HttpGet]` Range, `[HttpPost("remove")]` Remove

### DI Registration — `src/Roman.RedisManager.Web/Program.cs`

Added per phase, after the existing `IRedisRepository` registration:

```
Phase 1: IRedisKeyRepository → RedisKeyRepository, IRedisStringRepository → RedisStringRepository
Phase 2: IRedisListRepository → RedisListRepository, IRedisSetRepository → RedisSetRepository
Phase 3: IRedisHashRepository → RedisHashRepository, IRedisSortedSetRepository → RedisSortedSetRepository
```

All registered as `AddSingleton<TInterface, TImplementation>()`.

---

## Verification

### Phase 1 Verification

#### 1. Unit Tests — Handler Tests

Create in `tests/Roman.RedisManager.Tests/Application/CQRS/`:

- **`SetStringCommandHandlerTests.cs`**
  - `HandleAsync_ValidCommand_ReturnsSuccess` — stub repository returns `true`, assert `result.Success.ShouldBeTrue()`
  - `HandleAsync_ConditionNotMet_ReturnsFailure` — stub returns `false` (NX on existing key)
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetStringQueryHandlerTests.cs`**
  - `HandleAsync_ExistingKey_ReturnsValue` — stub returns `"hello"`, assert `result.Value.ShouldBe("hello")`
  - `HandleAsync_NonExistentKey_ReturnsNull` — stub returns `null`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`DeleteKeyCommandHandlerTests.cs`**
  - `HandleAsync_ExistingKey_ReturnsDeleted` — stub returns `true`
  - `HandleAsync_NonExistentKey_ReturnsNotDeleted` — stub returns `false`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetKeyMetadataQueryHandlerTests.cs`**
  - `HandleAsync_ExistingKey_ReturnsMetadata` — verify Type and Ttl mapping
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`GetKeyValueQueryHandlerTests.cs`**
  - `HandleAsync_StringKey_ReturnsStringValue`
  - `HandleAsync_ListKey_ReturnsListValues`
  - `HandleAsync_NonExistentKey_ReturnsNoneType`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

Handler tests use hand-written stub/fake inner classes implementing the relevant repository interface (following the existing `StubRedisRepository` pattern in `RedisKeysSearchQueryHandlerTests.cs`).

#### 2. Integration Tests — Repository Tests

Create in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/`:

- **`RedisKeyRepositoryTests.cs`** — `[Collection("Redis")]`, uses `RedisContainerFixture`
  - `DeleteKeyAsync_ExistingKey_ReturnsTrue` — seed a key, delete it, verify `true`
  - `DeleteKeyAsync_NonExistentKey_ReturnsFalse`
  - `DeleteKeyAsync_EmptyGroupId_ThrowsArgumentException`
  - `GetKeyMetadataAsync_StringKey_ReturnsStringType` — seed string, check type = String
  - `GetKeyMetadataAsync_WithTtl_ReturnsTtl` — seed string with expiry, check TTL > 0
  - `GetKeyMetadataAsync_NoTtl_ReturnsNullTtl`
  - `GetKeyValueAsync_StringKey_ReturnsStringValue` — seed string, auto-detect read
  - `GetKeyValueAsync_ListKey_ReturnsListValues` — seed list, auto-detect read
  - `GetKeyValueAsync_NonExistentKey_ReturnsNoneType`

- **`RedisStringRepositoryTests.cs`** — `[Collection("Redis")]`
  - `StringSetAsync_NewKey_ReturnsTrue` — set key, verify true
  - `StringSetAsync_WithTtl_SetsExpiry` — set with TTL, verify PTTL > 0
  - `StringSetAsync_NotExistsCondition_ExistingKey_ReturnsFalse` — set, re-set NX, expect false
  - `StringSetAsync_ExistsCondition_NonExistentKey_ReturnsFalse`
  - `StringGetAsync_ExistingKey_ReturnsValue`
  - `StringGetAsync_NonExistentKey_ReturnsNull`
  - `StringSetAsync_EmptyGroupId_ThrowsArgumentException`

All integration tests follow the existing pattern: instantiate real repository with `SimpleConnectionManager` + `Options.Create(...)`.

#### 3. Build

```powershell
dotnet build
```

Must compile with zero errors. Run after every file is created.

#### 4. Run Tests

```powershell
dotnet test
```

All existing tests must continue to pass. All new tests must pass.

#### 5. Manual HTTP Verification

Start Docker Redis:
```powershell
cd docker
docker compose up -d
```

Start the API:
```powershell
dotnet run --project src/Roman.RedisManager.Web/Roman.RedisManager.Web.csproj
```

Append to `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` and execute each request:

```http
### Set a string value
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-strings
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:string", "value": "hello world" }

###

### Set a string value with TTL (30 seconds)
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-strings
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:string:ttl", "value": "expires soon", "ttl": "00:00:30" }

###

### Set a string value with NX condition (only if not exists)
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-strings
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:string", "value": "should fail", "condition": "NotExists" }

###

### Get a string value
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-strings?groupId={{RedisServerGroupId}}&key=test:string

###

### Get key metadata (type + TTL)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-keys/test:string/metadata?groupId={{RedisServerGroupId}}

###

### Get key value (auto-detect type)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-keys/test:string/value?groupId={{RedisServerGroupId}}

###

### Delete a key
DELETE {{Roman.RedisManager.Web_HostAddress}}/api/redis-keys/test:string?groupId={{RedisServerGroupId}}
```

**Verify:**
- POST string returns 200 with `{ "success": true }`
- POST string with NX on existing key returns 200 with `{ "success": false }`
- GET string returns 200 with `{ "value": "hello world" }`
- GET metadata returns `{ "metadata": { "type": "String", "ttlMilliseconds": null } }`
- GET value (auto-detect) returns string value for string-typed key
- DELETE returns 200 with `{ "deleted": true }`
- DELETE same key again returns `{ "deleted": false }`
- Verify TTL key: GET metadata shows `ttlMilliseconds > 0`, wait 30s, GET returns null

#### 6. Swagger Verification

Open `https://localhost:7244/swagger` — confirm all new endpoints appear with correct schemas.

---

### Phase 2 Verification

#### 1. Unit Tests — Handler Tests

Create in `tests/Roman.RedisManager.Tests/Application/CQRS/`:

- **`PushToListCommandHandlerTests.cs`**
  - `HandleAsync_ValidCommand_Completes` — stub doesn't throw
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetListRangeQueryHandlerTests.cs`**
  - `HandleAsync_WithValues_ReturnsMappedValues`
  - `HandleAsync_EmptyList_ReturnsEmptyCollection`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`RemoveFromListCommandHandlerTests.cs`**
  - `HandleAsync_MatchingElements_ReturnsRemovedCount`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`AddToSetCommandHandlerTests.cs`**
  - `HandleAsync_ValidCommand_Completes`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetSetMembersQueryHandlerTests.cs`**
  - `HandleAsync_WithMembers_ReturnsScanResult`
  - `HandleAsync_EmptySet_ReturnsEmptyWithZeroCursor`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`RemoveFromSetCommandHandlerTests.cs`**
  - `HandleAsync_MatchingMembers_ReturnsRemovedCount`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

#### 2. Integration Tests — Repository Tests

Create in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/`:

- **`RedisListRepositoryTests.cs`** — `[Collection("Redis")]`
  - `ListPushAsync_RightPush_AppendsValues` — RPUSH then LRANGE, verify order
  - `ListPushAsync_LeftPush_PrependsValues` — LPUSH then LRANGE, verify reverse order
  - `ListPushAsync_WithTtl_SetsExpiry`
  - `ListRangeAsync_FullRange_ReturnsAllElements`
  - `ListRangeAsync_SubRange_ReturnsSlice`
  - `ListRangeAsync_NonExistentKey_ReturnsEmpty`
  - `ListRemoveAsync_MatchingElements_ReturnsCount`
  - `ListRemoveAsync_NoMatch_ReturnsZero`
  - `ListPushAsync_EmptyGroupId_ThrowsArgumentException`

- **`RedisSetRepositoryTests.cs`** — `[Collection("Redis")]`
  - `SetAddAsync_NewMembers_AddsThem`
  - `SetAddAsync_DuplicateMembers_Idempotent`
  - `SetAddAsync_WithTtl_SetsExpiry`
  - `SetScanAsync_WithMembers_ReturnsMembers`
  - `SetScanAsync_EmptySet_ReturnsEmpty`
  - `SetRemoveAsync_ExistingMembers_ReturnsCount`
  - `SetRemoveAsync_NonExistentMembers_ReturnsZero`
  - `SetAddAsync_EmptyGroupId_ThrowsArgumentException`

#### 3. Build

```powershell
dotnet build
```

#### 4. Run Tests

```powershell
dotnet test
```

#### 5. Manual HTTP Verification

Append to `.http` file:

```http
### Push to list (right)
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-lists
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:list", "values": ["a", "b", "c"], "direction": "Right" }

###

### Get list range
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-lists?groupId={{RedisServerGroupId}}&key=test:list&start=0&stop=-1

###

### Remove from list
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-lists/remove
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:list", "value": "b", "count": 1 }

###

### Add to set
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-sets
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:set", "members": ["x", "y", "z"] }

###

### Get set members (scan)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-sets?groupId={{RedisServerGroupId}}&key=test:set&cursor=0&pageSize=100

###

### Remove from set
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-sets/remove
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:set", "members": ["y"] }
```

**Verify:**
- LPUSH/RPUSH: elements appear in correct order via LRANGE
- LREM: removed count matches, subsequent LRANGE confirms removal
- SADD: members present via SSCAN
- SREM: removed count correct, subsequent SSCAN confirms removal
- WRONGTYPE: set a string key, then POST to `api/redis-lists` with same key → 409 Conflict

#### 6. Swagger Verification

Open Swagger, confirm all Phase 2 endpoints appear.

---

### Phase 3 Verification

#### 1. Unit Tests — Handler Tests

Create in `tests/Roman.RedisManager.Tests/Application/CQRS/`:

- **`SetHashFieldsCommandHandlerTests.cs`**
  - `HandleAsync_ValidCommand_Completes`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetHashFieldsQueryHandlerTests.cs`**
  - `HandleAsync_WithFields_ReturnsScanResult`
  - `HandleAsync_EmptyHash_ReturnsEmpty`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`RemoveHashFieldsCommandHandlerTests.cs`**
  - `HandleAsync_ExistingFields_ReturnsRemovedCount`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`AddToSortedSetCommandHandlerTests.cs`**
  - `HandleAsync_ValidCommand_Completes`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

- **`GetSortedSetRangeQueryHandlerTests.cs`**
  - `HandleAsync_Ascending_ReturnsOrderedEntries`
  - `HandleAsync_Descending_ReturnsReverseOrderedEntries`
  - `HandleAsync_NullQuery_ThrowsArgumentNullException`

- **`RemoveFromSortedSetCommandHandlerTests.cs`**
  - `HandleAsync_ExistingMembers_ReturnsRemovedCount`
  - `HandleAsync_NullCommand_ThrowsArgumentNullException`

#### 2. Integration Tests — Repository Tests

Create in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/`:

- **`RedisHashRepositoryTests.cs`** — `[Collection("Redis")]`
  - `HashSetAsync_NewFields_SetsThem`
  - `HashSetAsync_OverwriteField_UpdatesValue`
  - `HashSetAsync_WithTtl_SetsExpiry`
  - `HashScanAsync_WithFields_ReturnsFieldValuePairs`
  - `HashScanAsync_EmptyHash_ReturnsEmpty`
  - `HashDeleteFieldsAsync_ExistingFields_ReturnsCount`
  - `HashDeleteFieldsAsync_NonExistentFields_ReturnsZero`
  - `HashSetAsync_EmptyGroupId_ThrowsArgumentException`

- **`RedisSortedSetRepositoryTests.cs`** — `[Collection("Redis")]`
  - `SortedSetAddAsync_NewEntries_AddsThem`
  - `SortedSetAddAsync_UpdateExistingScore_Updates`
  - `SortedSetAddAsync_WithTtl_SetsExpiry`
  - `SortedSetRangeAsync_Ascending_ReturnsOrdered`
  - `SortedSetRangeAsync_Descending_ReturnsReverseOrdered`
  - `SortedSetRangeAsync_SubRange_ReturnsSlice`
  - `SortedSetRemoveAsync_ExistingMembers_ReturnsCount`
  - `SortedSetRemoveAsync_NonExistentMembers_ReturnsZero`
  - `SortedSetAddAsync_EmptyGroupId_ThrowsArgumentException`

#### 3. Build

```powershell
dotnet build
```

#### 4. Run Tests

```powershell
dotnet test
```

#### 5. Manual HTTP Verification

Append to `.http` file:

```http
### Set hash fields
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-hashes
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:hash", "fields": [{"field":"name","value":"Alice"},{"field":"age","value":"30"}] }

###

### Get hash fields (scan)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-hashes?groupId={{RedisServerGroupId}}&key=test:hash&cursor=0&pageSize=100

###

### Remove hash fields
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-hashes/remove
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:hash", "fields": ["age"] }

###

### Add to sorted set
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-sorted-sets
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:zset", "entries": [{"member":"alice","score":1.0},{"member":"bob","score":2.5},{"member":"charlie","score":1.5}] }

###

### Get sorted set range (ascending)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-sorted-sets?groupId={{RedisServerGroupId}}&key=test:zset&start=0&stop=-1&ascending=true

###

### Get sorted set range (descending)
GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-sorted-sets?groupId={{RedisServerGroupId}}&key=test:zset&start=0&stop=-1&ascending=false

###

### Remove from sorted set
POST {{Roman.RedisManager.Web_HostAddress}}/api/redis-sorted-sets/remove
Content-Type: application/json

{ "groupId": "{{RedisServerGroupId}}", "key": "test:zset", "members": ["bob"] }
```

**Verify:**
- HSET: fields present via HSCAN
- HDEL: removed count correct, subsequent HSCAN confirms removal
- ZADD: entries present via ZRANGE in score order
- ZRANGE ascending: alice(1.0), charlie(1.5), bob(2.5)
- ZRANGE descending: bob(2.5), charlie(1.5), alice(1.0)
- ZREM: bob removed, subsequent ZRANGE returns 2 entries
- WRONGTYPE: use a string key with hash/sorted-set endpoint → 409 Conflict

#### 6. Swagger Verification

Open Swagger, confirm all Phase 3 endpoints appear.

---

## File Summary — Complete List

### Phase 1 (16 new files + 2 modified)

| # | Action | Path |
|---|--------|------|
| 1 | Create | `src/Roman.RedisManager.Domain/Entities/RedisDataType.cs` |
| 2 | Create | `src/Roman.RedisManager.Domain/Entities/SetCondition.cs` |
| 3 | Create | `src/Roman.RedisManager.Domain/Entities/RedisKeyMetadata.cs` |
| 4 | Create | `src/Roman.RedisManager.Domain/Entities/RedisKeyValue.cs` |
| 5 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisKeyRepository.cs` |
| 6 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisStringRepository.cs` |
| 7 | Create | `src/Roman.RedisManager.Infrastructure/Exceptions/RedisTypeMismatchException.cs` |
| 8 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisKeyRepository.cs` |
| 9 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisStringRepository.cs` |
| 10 | Create | `src/Roman.RedisManager.Application/CQRS/SetStringCommandHandler.cs` |
| 11 | Create | `src/Roman.RedisManager.Application/CQRS/GetStringQueryHandler.cs` |
| 12 | Create | `src/Roman.RedisManager.Application/CQRS/DeleteKeyCommandHandler.cs` |
| 13 | Create | `src/Roman.RedisManager.Application/CQRS/GetKeyMetadataQueryHandler.cs` |
| 14 | Create | `src/Roman.RedisManager.Application/CQRS/GetKeyValueQueryHandler.cs` |
| 15 | Create | `src/Roman.RedisManager.Web/Controllers/RedisStringsController.cs` |
| 16 | Modify | `src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs` — add DELETE, GET metadata, GET value actions |
| 17 | Modify | `src/Roman.RedisManager.Web/Program.cs` — add DI registrations |
| 18 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/SetStringCommandHandlerTests.cs` |
| 19 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetStringQueryHandlerTests.cs` |
| 20 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/DeleteKeyCommandHandlerTests.cs` |
| 21 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetKeyMetadataQueryHandlerTests.cs` |
| 22 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetKeyValueQueryHandlerTests.cs` |
| 23 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisKeyRepositoryTests.cs` |
| 24 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisStringRepositoryTests.cs` |
| 25 | Modify | `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` — append Phase 1 requests |

### Phase 2 (14 new files + 2 modified)

| # | Action | Path |
|---|--------|------|
| 1 | Create | `src/Roman.RedisManager.Domain/Entities/ListDirection.cs` |
| 2 | Create | `src/Roman.RedisManager.Domain/Entities/RedisScanResult.cs` |
| 3 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisListRepository.cs` |
| 4 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisSetRepository.cs` |
| 5 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisListRepository.cs` |
| 6 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisSetRepository.cs` |
| 7 | Create | `src/Roman.RedisManager.Application/CQRS/PushToListCommandHandler.cs` |
| 8 | Create | `src/Roman.RedisManager.Application/CQRS/GetListRangeQueryHandler.cs` |
| 9 | Create | `src/Roman.RedisManager.Application/CQRS/RemoveFromListCommandHandler.cs` |
| 10 | Create | `src/Roman.RedisManager.Application/CQRS/AddToSetCommandHandler.cs` |
| 11 | Create | `src/Roman.RedisManager.Application/CQRS/GetSetMembersQueryHandler.cs` |
| 12 | Create | `src/Roman.RedisManager.Application/CQRS/RemoveFromSetCommandHandler.cs` |
| 13 | Create | `src/Roman.RedisManager.Web/Controllers/RedisListsController.cs` |
| 14 | Create | `src/Roman.RedisManager.Web/Controllers/RedisSetsController.cs` |
| 15 | Modify | `src/Roman.RedisManager.Web/Program.cs` — add DI registrations |
| 16 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/PushToListCommandHandlerTests.cs` |
| 17 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetListRangeQueryHandlerTests.cs` |
| 18 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/RemoveFromListCommandHandlerTests.cs` |
| 19 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/AddToSetCommandHandlerTests.cs` |
| 20 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetSetMembersQueryHandlerTests.cs` |
| 21 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/RemoveFromSetCommandHandlerTests.cs` |
| 22 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisListRepositoryTests.cs` |
| 23 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisSetRepositoryTests.cs` |
| 24 | Modify | `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` — append Phase 2 requests |

### Phase 3 (14 new files + 2 modified)

| # | Action | Path |
|---|--------|------|
| 1 | Create | `src/Roman.RedisManager.Domain/Entities/RedisSortedSetEntry.cs` |
| 2 | Create | `src/Roman.RedisManager.Domain/Entities/RedisHashEntry.cs` |
| 3 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisHashRepository.cs` |
| 4 | Create | `src/Roman.RedisManager.Domain/Repositories/IRedisSortedSetRepository.cs` |
| 5 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisHashRepository.cs` |
| 6 | Create | `src/Roman.RedisManager.Infrastructure/Repositories/RedisSortedSetRepository.cs` |
| 7 | Create | `src/Roman.RedisManager.Application/CQRS/SetHashFieldsCommandHandler.cs` |
| 8 | Create | `src/Roman.RedisManager.Application/CQRS/GetHashFieldsQueryHandler.cs` |
| 9 | Create | `src/Roman.RedisManager.Application/CQRS/RemoveHashFieldsCommandHandler.cs` |
| 10 | Create | `src/Roman.RedisManager.Application/CQRS/AddToSortedSetCommandHandler.cs` |
| 11 | Create | `src/Roman.RedisManager.Application/CQRS/GetSortedSetRangeQueryHandler.cs` |
| 12 | Create | `src/Roman.RedisManager.Application/CQRS/RemoveFromSortedSetCommandHandler.cs` |
| 13 | Create | `src/Roman.RedisManager.Web/Controllers/RedisHashesController.cs` |
| 14 | Create | `src/Roman.RedisManager.Web/Controllers/RedisSortedSetsController.cs` |
| 15 | Modify | `src/Roman.RedisManager.Web/Program.cs` — add DI registrations |
| 16 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/SetHashFieldsCommandHandlerTests.cs` |
| 17 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetHashFieldsQueryHandlerTests.cs` |
| 18 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/RemoveHashFieldsCommandHandlerTests.cs` |
| 19 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/AddToSortedSetCommandHandlerTests.cs` |
| 20 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/GetSortedSetRangeQueryHandlerTests.cs` |
| 21 | Create | `tests/Roman.RedisManager.Tests/Application/CQRS/RemoveFromSortedSetCommandHandlerTests.cs` |
| 22 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisHashRepositoryTests.cs` |
| 23 | Create | `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisSortedSetRepositoryTests.cs` |
| 24 | Modify | `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` — append Phase 3 requests |

**Total: 44 new files, 6 modifications (Program.cs x3, RedisKeysController x1, .http x3)**
