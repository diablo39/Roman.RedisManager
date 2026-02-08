# Style Guide: Repository Interfaces

> Conventions unique to this project.

## Location and Namespace
Interfaces live in `Domain/Repositories/` with namespace `Roman.RedisManager.Domain.Repositories`.

## Naming Convention
- Interface name: `I{EntityName}Repository` (e.g., `IRedisRepository`, `IRedisServerRepository`).
- One interface per entity concern.

## Return Types
- Single result: Domain entity type (e.g., `RedisSearchResult`).
- Collection: `IReadOnlyCollection<T>` — not `IEnumerable<T>`, `List<T>`, or `IList<T>`.

## Synchronous Methods
All current repository methods are synchronous (no `Task<T>`). This is a deliberate choice in this codebase.

## Method Naming
Descriptive verbs matching the operation:
- `SearchForKeys(string predicate)` — searching with a filter.
- `ListRedisServers()` — listing all items.
