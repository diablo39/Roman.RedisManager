# Tech Stack Analysis — Roman.RedisManager

## Core Technology Analysis

### Programming Language(s)
- **C# 13** (implied by .NET 10.0 target framework)
- Uses modern C# features: primary constructors (`RedisKey(string key)`), `required` properties, records, collection expressions (`[ ... ]`), global usings, file-scoped namespaces are **not** used (all files use block-scoped namespaces).

### Primary Framework
- **ASP.NET Core 10.0** — Web API project using `Microsoft.NET.Sdk.Web`.
- Controllers with `[ApiController]` attribute; conventional routing via `[Route("api/...")]`.
- OpenAPI document generation enabled (`Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore.SwaggerUI`).

### Secondary / Tertiary Frameworks
- **Wolverine (WolverineFx 5.9.2)** — Used as an in-process mediator (CQRS). Configured with `DurabilityMode.MediatorOnly`. Handlers are discovered via assembly scanning of `Roman.RedisManager.*` assemblies.
- **StackExchange.Redis 2.10.1** — Redis client used in the Infrastructure layer for key scanning via `IConnectionMultiplexer`.
- **Microsoft.Extensions.Options** — Strongly-typed configuration with `IOptions<T>`, data-annotation validation (`ValidateDataAnnotations`, `ValidateOnStart`).

### State Management Approach
- No client-side state management — this is a backend API.
- Server configuration is read from `appsettings.json` via the Options pattern (`IOptions<RedisConfiguration>`).
- Redis connection state is managed through `IConnectionMultiplexer` (singleton).

### Other Relevant Technologies & Patterns
- **Clean Architecture** — Solution is split into Domain, Application, Infrastructure, and Web (presentation) layers with explicit project references enforcing dependency direction.
- **Repository Pattern** — Interfaces in Domain (`IRedisRepository`, `IRedisServerRepository`), implementations in Infrastructure.
- **CQRS via Wolverine** — Query objects, result DTOs, and static handler classes in the Application layer, invoked through `IMessageBus`.
- **SPA Hosting** — `MapFallbackToFile("index.html")` indicates an Angular frontend served from `wwwroot/`.

---

## Domain Specificity Analysis

### Problem Domain
**Redis Server Management Dashboard** — A tool for managing and inspecting multiple Redis server instances. It provides a web API (consumed by an Angular SPA) to list configured Redis servers and search/browse Redis keys.

### Core Business Concepts
- **Redis Server Registry** — Maintaining a list of configured Redis servers with connection details (name, endpoints, credentials).
- **Redis Key Browsing** — Scanning and searching keys on a connected Redis server using pattern matching (`SCAN` command).
- **Pagination** — Server listing supports pagination (page number, page size).
- **Server Identity** — Servers are identified by an MD5 hash of their name (used as an `Id`).

### User Interactions
- Listing all configured Redis servers (paginated).
- Searching/scanning Redis keys by pattern on a specific server.
- Viewing key metadata.

### Primary Data Types and Structures
- `RedisServer` — Entity with `Name` and `Endpoints`.
- `RedisKey` — Value object wrapping a Redis key string.
- `RedisSearchResult` — Contains a list of `RedisKey` items, a cursor for pagination, and a `HasMoreResults` flag.
- `RedisServerDto` — DTO with `Name` and `Id` (MD5 hash).
- `RedisServersQueryResult` — Paginated result record with `Servers`, `TotalCount`, `PageNumber`, `PageSize`.
- `RedisConfiguration` / `RedisServerConfiguration` — Strongly-typed config classes mapping `appsettings.json`.

---

## Application Boundaries

### Features Clearly Within Scope
- Listing and paginating configured Redis servers.
- Connecting to Redis servers and scanning/searching keys.
- Serving an Angular SPA frontend via static files.
- Configuration-driven server registry (no database — servers come from `appsettings.json`).

### Features Architecturally Inconsistent
- **User authentication/authorization** — No auth infrastructure exists; adding it would require significant cross-cutting changes.
- **Real-time features (WebSockets/SignalR)** — Not present; the architecture is request/response only.
- **Multi-tenant or cloud-native patterns** — No tenant isolation, distributed caching layer, or cloud service integrations.
- **Write operations to Redis** — Current design is read-only (SCAN only); adding SET/DEL would require new command handlers and potentially a different safety model.
- **Persistent storage / ORM** — No database; adding EF Core or similar would be a new architectural concern.

### Specialized Libraries & Domain Constraints
- **StackExchange.Redis** — Constrains Redis interactions to what this client supports. The code notes a TODO about cluster support limitations.
- **Wolverine** — Constrains the messaging/CQRS pattern; all new queries/commands must follow Wolverine's handler conventions (static `Handle` methods with message + dependency parameters).
- **MD5 hashing for IDs** — Server IDs are derived from names via MD5; this is a domain convention, not a security measure.
