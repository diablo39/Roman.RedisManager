# Phase 1 Data Model: Redis Search Metadata Expansion

## Entity: RedisKey (search projection usage)

- Purpose: Represents a Redis key returned by search with inline metadata needed by clients.
- Existing field:
- `Key` (string, required)
- Planned metadata fields:
- `Type` (RedisDataType or equivalent mapped representation, required in API output)
- `Ttl` (TimeSpan?, nullable for persistent keys)
- `HasExpiration` (bool, derived from TTL semantics)

Validation rules:
- `Key` must be non-empty.
- `Type` must map to a known output value set (fallback to `Unknown` allowed).
- `Ttl` must be null for persistent keys or non-negative for expiring keys in output mapping.

State transitions (metadata lifecycle during one request):
- `Discovered` -> `MetadataResolved` when type/ttl lookups succeed.
- `Discovered` -> `MetadataMissing` when key disappears or metadata retrieval is unavailable.

## Entity: RedisSearchResult

- Purpose: Encapsulates paged key search results and continuation behavior.
- Existing fields:
- `Keys` (IEnumerable<RedisKey>)
- `HasMoreResults` (bool)
- `Cursor`/`ContinuationToken`/`NodeCursors`
- Planned change:
- Keys in `Keys` carry metadata for API projection.

Validation rules:
- `ContinuationToken` only present when `HasMoreResults = true`.
- `Keys.Count` is bounded by effective page size (requested size capped by max allowed size).

## Entity: RedisKeysSearchQuery (application input)

- Purpose: Request contract passed from controller to handler.
- Existing fields:
- `GroupId`, `Pattern`, `ContinuationToken`, `PageSize`
- Planned constraint:
- `PageSize` normalized to safe bounded value using configured max size.

Validation rules:
- `GroupId` required and non-empty.
- `Pattern` defaults to `*` when omitted/empty.
- `PageSize` must be positive; values above max are capped.

## Entity: RedisKeyDto (API output item)

- Purpose: Public API contract per returned key.
- Planned fields:
- `key` (string)
- `type` (string enum value)
- `ttlMilliseconds` (number or null)
- `hasExpiration` (boolean)

Validation rules:
- Every returned item contains all fields above.
- `ttlMilliseconds` null implies no expiration (persistent key) or a non-fatal metadata miss.

## Entity: RedisSearchLimitsConfiguration (new options model)

- Purpose: Bound expensive search requests.
- Proposed fields:
- `MaxPageSize` (required, integer >= 1)

Validation rules:
- Data annotations enforce required and minimum bound.
- Bound in `Program.cs` via options validation on startup.
