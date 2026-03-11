# Phase 0 Research: Redis Search Metadata Expansion

## Unknown 1: TTL contract shape in search response

- Decision: Use `ttlMilliseconds` as nullable integer (`long?`), where `null` means no expiration.
- Rationale: Existing metadata endpoint already exposes TTL in milliseconds via `RedisKeyMetadataDto(string Type, long? TtlMilliseconds)` in `src/Roman.RedisManager.Application/CQRS/GetKeyMetadataQueryHandler.cs`. Reusing this shape avoids semantic drift and keeps client-side handling consistent.
- Alternatives considered:
- Use `TimeSpan`-formatted string: rejected because JSON string parsing is less predictable and inconsistent with existing endpoint.
- Use seconds (`ttlSeconds`): rejected because existing endpoint uses milliseconds and changing units increases confusion.
- Use absolute expiration timestamp: rejected because Redis natively exposes remaining TTL and not all keys expire.

## Unknown 2: Additional useful attributes beyond type + ttl

- Decision: Add `hasExpiration` (bool) to each key item.
- Rationale: This field is derived from TTL and improves UX by avoiding repeated client-side interpretation logic. It directly supports the user request for additional useful attributes while staying lightweight.
- Alternatives considered:
- Add size/memory attributes (`MEMORY USAGE`): rejected for now due to extra command cost and potential compatibility/permission variance.
- Add idle time or LFU frequency: rejected due to command/version/policy dependencies and higher request overhead.
- Add absolute expiry timestamp in addition to TTL: rejected as redundant for initial scope.

## Unknown 3: How to limit Redis request volume while enriching metadata

- Decision: Enrich metadata in `RedisRepository.SearchForKeysAsync` path using pipelined async calls (`KeyTypeAsync` + `KeyTimeToLiveAsync`) over the page's returned keys, and enforce a configurable max page size.
- Rationale: Repository-level enrichment keeps CQRS handler simple, allows request batching near data source, and minimizes network round trips versus handler-level N+1 metadata calls. Page-size capping bounds command count deterministically.
- Alternatives considered:
- Handler-level per-key metadata retrieval through `IRedisKeyRepository`: rejected because it increases cross-layer orchestration and risks sequential request behavior.
- Lua script to fetch tuples in a single command per node: deferred; potentially faster but adds script management complexity and larger change surface.
- No cap on page size: rejected because it allows unbounded metadata command volume.

## Unknown 4: Handling key volatility during enrichment

- Decision: Treat key disappearance between SCAN and metadata fetch as non-fatal; return a stable response and represent missing/volatile metadata safely (`type=None/Unknown` and null TTL-derived fields as applicable by final mapping rules).
- Rationale: Redis keyspace is mutable; failing the whole page for a single disappearing key degrades endpoint reliability.
- Alternatives considered:
- Fail entire request on any per-key metadata miss: rejected as fragile.
- Drop keys silently from response: rejected due to possible confusion and pagination inconsistency.

## Best Practices Applied to This Feature

- Keep controller thin and dispatch through Wolverine only.
- Keep repository interface in Domain and implementation in Infrastructure.
- Keep DTO mapping in CQRS handler with explicit projection.
- Validate and cap page size early (query/handler boundary) while preserving repository argument safety checks.
- Ensure test updates include mutation loop evidence (`run -> analyze -> improve -> rerun`).
