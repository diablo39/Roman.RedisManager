# Copilot Instructions — Roman.RedisManager

## 1. Overview

This file enables AI coding assistants (GitHub Copilot, etc.) to generate features that are **consistent with the existing architecture, conventions, and patterns** of the Roman.RedisManager codebase.

Everything documented here is derived from **actual observed patterns** in the codebase — not invented best practices or external recommendations.

**Project summary:** A .NET 10.0 backend API for managing and inspecting Redis server instances. Built with Clean Architecture (Domain → Application → Infrastructure → Web), using Wolverine as an in-process CQRS mediator and StackExchange.Redis for Redis connectivity. Serves an Angular SPA frontend via static files.

---

## 2. File Category Reference

### Controllers
**What:** ASP.NET Core API controllers — the HTTP entry point for all API requests.
**Examples:** `src/Roman.RedisManager.Web/Controllers/RedisServersController.cs`
**Key conventions:**
- Inherit from `ControllerBase` with `[ApiController]` attribute.
- Routes use `[Route("api/{kebab-case-resource}")]`.
- Inject `IMessageBus` (Wolverine) — **never** inject repositories directly.
- Expression-bodied constructors for single-dependency controllers.
- Return DTOs directly (not `IActionResult` or `ActionResult<T>`).
- Dispatch all logic via `await _bus.InvokeAsync<TResult>(new QueryOrCommand { ... })`.

### CQRS Handlers
**What:** Wolverine message handlers implementing the CQRS pattern.
**Examples:** `src/Roman.RedisManager.Application/CQRS/RedisServersQueryHandler.cs`
**Key conventions:**
- **Static class** with a **static `Handle` method** (Wolverine convention).
- Named `{MessageType}Handler`.
- First parameter is the message (query/command), subsequent parameters are DI-resolved dependencies.
- Queries are plain classes with `{ get; set; }` properties; results and DTOs are C# `record` types.
- All types (query, DTOs, result, handler) are **co-located in the same file**.
- Entity-to-DTO mapping uses LINQ `.Select()` inline — no AutoMapper.

### Domain Entities
**What:** Core domain objects representing business concepts.
**Examples:** `src/Roman.RedisManager.Domain/Entities/RedisServer.cs`, `RedisKey.cs`, `RedisSearchResult.cs`
**Key conventions:**
- Block-scoped namespaces (not file-scoped).
- Properties use `internal set` or `protected set` to restrict mutation.
- Simple entities use C# primary constructors; complex ones use traditional constructors.
- Private `const` fields for magic values (underscore-prefixed: `_emptyCursor`).
- Collection properties typed as `IEnumerable<T>`.
- Zero external dependencies.

### Repository Interfaces
**What:** Abstractions for data access, defined in the Domain layer.
**Examples:** `src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs`, `IRedisServerRepository.cs`
**Key conventions:**
- Named `I{Entity}Repository`.
- Return domain entities or `IReadOnlyCollection<T>`.
- Synchronous methods (no `Task<T>` currently).
- Descriptive verb method names: `SearchForKeys`, `ListRedisServers`.

### Repository Implementations
**What:** Concrete data access using StackExchange.Redis and the Options pattern.
**Examples:** `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`, `RedisServerRepository.cs`
**Key conventions:**
- Implement interfaces from `Domain/Repositories/`.
- Constructor injection with `private readonly` underscore-prefixed fields.
- Use `using` type alias when StackExchange.Redis types conflict with domain types.
- Map infrastructure types to domain entities via LINQ `.Select()`.
- Registered as **singletons** in DI.

### Configuration Classes
**What:** Strongly-typed Options pattern classes for `appsettings.json` binding.
**Examples:** `src/Roman.RedisManager.Infrastructure/Configuration/RedisConfiguration.cs`, `RedisServerConfiguration.cs`
**Key conventions:**
- Root class defines `public const string SectionName`.
- Properties use **both** `required` keyword and `[Required]` + `[MinLength(1)]` attributes.
- Every property has explicit `[ConfigurationKeyName("...")]`.
- Optional properties are nullable (`string?`).
- Namespace: `Roman.RedisManager.Infrastructure.Configuration`.
- Validated at startup: `.ValidateDataAnnotations().ValidateOnStart()`.

### Extension Methods
**What:** Reusable utility methods in a standalone project with zero dependencies.
**Examples:** `src/Roman.RedisManager.Extensions/StringExtensions.cs`
**Key conventions:**
- Static class named `{Type}Extensions`.
- Pure functions — no side effects, no state.
- `using var` for disposables.
- Consumed via `global using Roman.RedisManager.Extensions;` in `GlobalUsings.cs`.

### Tests
**What:** xUnit facts with Shouldly assertions.
**Examples:** `tests/Roman.RedisManager.Tests/RedisServerRepositoryTests.cs`, `RedisRepositoryTests.cs`
**Key conventions:**
- Method naming: `MethodName_Condition_ExpectedBehavior`.
- `[Fact]` only — no `[Theory]`.
- Shouldly assertions: `.ShouldNotBeNull()`, `.ShouldNotBeEmpty()`, `.ShouldAllBe()`, `.ShouldContain()`.
- **No mocking framework** — instantiate real implementations.
- Config via `Options.Create(...)` with private helper methods.
- Declare repositories as interface types: `IRedisServerRepository repo = new RedisServerRepository(...)`.
- Global usings for `Xunit` and `Shouldly`.

### Startup & Hosting
**What:** Application bootstrap in `Program.cs`.
**Examples:** `src/Roman.RedisManager.Web/Program.cs`
**Key conventions:**
- Minimal hosting model (`WebApplication.CreateBuilder`).
- Service registration order: Options → Singletons → Controllers → OpenApi → Wolverine.
- Wolverine scans all `Roman.RedisManager*` assemblies, `MediatorOnly` mode.
- Middleware: HTTPS redirect → Auth → Static files → Controllers → SPA fallback.
- Swagger/OpenAPI only in Development.

### Configuration Files
**What:** JSON config files for app settings and launch profiles.
**Examples:** `src/Roman.RedisManager.Web/appsettings.json`, `appsettings.Development.json`, `Properties/launchSettings.json`

### Global Usings
**What:** Per-project `GlobalUsings.cs` files for shared imports.
**Examples:** `src/Roman.RedisManager.Application/GlobalUsings.cs`, `tests/Roman.RedisManager.Tests/GlobalUsings.cs`

### HTTP Test Files
**What:** `.http` files for manual REST API testing.
**Examples:** `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

---

## 3. Feature Scaffold Guide

When implementing a new feature, follow this decision tree to determine which files to create and where to place them.

### Adding a New API Endpoint (e.g., "Get Redis Keys for a Server")

| Step | File to Create | Location | Convention |
|------|---------------|----------|------------|
| 1 | Domain entity (if new) | `src/Roman.RedisManager.Domain/Entities/` | `{EntityName}.cs` |
| 2 | Repository interface (if new) | `src/Roman.RedisManager.Domain/Repositories/` | `I{Entity}Repository.cs` |
| 3 | Repository implementation | `src/Roman.RedisManager.Infrastructure/Repositories/` | `{Entity}Repository.cs` |
| 4 | CQRS handler + query + DTOs | `src/Roman.RedisManager.Application/CQRS/` | `{Feature}QueryHandler.cs` (all types co-located) |
| 5 | Controller (or add action to existing) | `src/Roman.RedisManager.Web/Controllers/` | `{Resource}Controller.cs` |
| 6 | DI registration (if new repo) | `src/Roman.RedisManager.Web/Program.cs` | `builder.Services.AddSingleton<I..., ...>()` |
| 7 | Unit tests | `tests/Roman.RedisManager.Tests/` | `{Repository}Tests.cs` |
| 8 | HTTP test request | `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` | Append new request |

### Adding a New Extension Method

| Step | File to Create | Location |
|------|---------------|----------|
| 1 | Extension class | `src/Roman.RedisManager.Extensions/` → `{Type}Extensions.cs` |
| 2 | Global using (if new namespace) | `src/Roman.RedisManager.Application/GlobalUsings.cs` |

### Adding New Configuration

| Step | File to Create | Location |
|------|---------------|----------|
| 1 | Configuration class | `src/Roman.RedisManager.Infrastructure/Configuration/` → `{Feature}Configuration.cs` |
| 2 | JSON section | `src/Roman.RedisManager.Web/appsettings.json` |
| 3 | Options binding | `src/Roman.RedisManager.Web/Program.cs` → `.AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()` |

### Naming Conventions Summary
- **Namespaces:** Block-scoped (not file-scoped).
- **Entities:** `Roman.RedisManager.Domain.Entities`
- **Repository interfaces:** `Roman.RedisManager.Domain.Repositories`
- **Repository implementations:** `Roman.RedisManager.Infrastructure.Repositories`
- **CQRS handlers:** `Roman.RedisManager.Web.Wolverine`
- **Configuration:** `Roman.RedisManager.Web.Configuration`
- **Extensions:** `Roman.RedisManager.Extensions`
- **Controllers:** `Roman.RedisManager.Web.Controllers`
- **Tests:** `Roman.RedisManager.Tests`

---

## 4. Integration Rules

These constraints **must** be followed when generating code. Violating them will produce architecturally inconsistent output.

### CQRS / Wolverine
- ✅ Controllers dispatch via `IMessageBus.InvokeAsync<T>()`.
- ✅ Handlers are **static classes** with **static `Handle`/`HandleAsync` methods**.
- ✅ First parameter is the message; additional parameters are DI-injected by Wolverine.
- ✅ Handlers live in `src/Roman.RedisManager.Application/CQRS/`.
- ✅ Handlers use namespace `Roman.RedisManager.Web.Wolverine`.
- ❌ Controllers must **never** call repositories or services directly.
- ❌ Do not use `[Handler]` attributes — Wolverine uses convention-based discovery.

### Clean Architecture / Dependency Direction
- ✅ Web → Application, Infrastructure.
- ✅ Application → Domain, Extensions.
- ✅ Infrastructure → Domain.
- ❌ Domain must **never** reference Infrastructure, Application, Web, or any NuGet package.
- ❌ Application must **never** reference Infrastructure or Web.

### Repository Pattern
- ✅ Interfaces in `Domain/Repositories/`, implementations in `Infrastructure/Repositories/`.
- ✅ Return domain entities — never StackExchange.Redis types.
- ✅ Register as singletons in `Program.cs`.
- ❌ Do not use EF Core, Dapper, or other ORMs — this project uses StackExchange.Redis directly.

### Configuration
- ✅ Use Options pattern with `IOptions<T>`.
- ✅ Validate with `[Required]`, `[MinLength]`, `ValidateDataAnnotations()`, `ValidateOnStart()`.
- ✅ Every property gets `[ConfigurationKeyName("...")]`.
- ❌ Do not read `IConfiguration` directly in repositories or services.

### Testing
- ✅ Use xUnit `[Fact]` + Shouldly assertions.
- ✅ Instantiate real implementations — no mocks.
- ✅ Use `Options.Create(...)` for configuration in tests.
- ❌ Do not introduce Moq, NSubstitute, or other mocking frameworks.

### Code Style
- ✅ Block-scoped namespaces.
- ✅ `private readonly` fields with `_camelCase` prefix.
- ✅ Expression-bodied constructors for single-dependency classes.
- ✅ `required` keyword + `[Required]` attribute on mandatory config properties.
- ✅ `IEnumerable<T>` for collection properties; `IReadOnlyCollection<T>` for return types.
- ❌ Do not use file-scoped namespaces.

---

## 5. Example Prompt Usage

### Example 1: "Add an endpoint to get Redis keys for a specific server"

Copilot should generate:

1. **`src/Roman.RedisManager.Application/CQRS/RedisKeysQueryHandler.cs`**
   - `RedisKeysQuery` class (with `ServerId`, `Pattern`, `PageNumber`, `PageSize` properties).
   - `RedisKeyDto` record.
   - `RedisKeysQueryResult` record.
   - `RedisKeysQueryHandler` static class with `Handle(RedisKeysQuery query, IRedisRepository repository)`.
   - Namespace: `Roman.RedisManager.Web.Wolverine`.

2. **`src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`**
   - `[Route("api/redis-keys")]`, `[ApiController]`.
   - Constructor: `public RedisKeysController(IMessageBus bus) => _bus = bus;`.
   - `[HttpGet]` action dispatching `RedisKeysQuery` via `_bus.InvokeAsync<RedisKeysQueryResult>(...)`.

3. **DI registration in `Program.cs`** (if `IRedisRepository` isn't registered yet):
   - `builder.Services.AddSingleton<IRedisRepository, RedisRepository>();`

4. **`tests/Roman.RedisManager.Tests/RedisKeysQueryHandlerTests.cs`** (optional):
   - `[Fact]` tests using Shouldly.
   - Real `RedisRepository` or in-memory substitute.

5. **Append to `Roman.RedisManager.Web.http`:**
   ```
   GET {{Roman.RedisManager.Web_HostAddress}}/api/redis-keys?serverId=abc&pattern=*&pageNumber=1&pageSize=50
   ```

### Example 2: "Add a health check for Redis connectivity"

Copilot should generate:

1. **`src/Roman.RedisManager.Domain/Repositories/IRedisHealthRepository.cs`**
   - `bool IsConnected()` method.

2. **`src/Roman.RedisManager.Infrastructure/Repositories/RedisHealthRepository.cs`**
   - Implements `IRedisHealthRepository` using `IConnectionMultiplexer.IsConnected`.

3. **`src/Roman.RedisManager.Application/CQRS/RedisHealthQueryHandler.cs`**
   - `RedisHealthQuery`, `RedisHealthQueryResult` record, `RedisHealthQueryHandler` static class.
   - Namespace: `Roman.RedisManager.Web.Wolverine`.

4. **`src/Roman.RedisManager.Web/Controllers/RedisHealthController.cs`**
   - `[Route("api/redis-health")]`.
   - Dispatches via `IMessageBus`.

5. **`tests/Roman.RedisManager.Tests/RedisHealthRepositoryTests.cs`**

---

## Appendix: Project Structure

```
Roman.RedisManager.slnx
src/
├── Roman.RedisManager.Domain/           # Entities, repository interfaces (zero dependencies)
├── Roman.RedisManager.Application/      # CQRS handlers (depends on Domain, Extensions)
├── Roman.RedisManager.Infrastructure/   # Repository implementations, config classes (depends on Domain)
├── Roman.RedisManager.Extensions/       # Utility extension methods (zero dependencies)
└── Roman.RedisManager.Web/              # Controllers, Program.cs, static files (depends on Application, Infrastructure)
tests/
└── Roman.RedisManager.Tests/            # xUnit + Shouldly tests (depends on Domain, Infrastructure)
```

### Tech Stack Summary
| Technology | Version | Purpose |
|---|---|---|
| .NET | 10.0 | Runtime & SDK |
| ASP.NET Core | 10.0 | Web API framework |
| Wolverine | 5.9.2 | In-process CQRS mediator |
| StackExchange.Redis | 2.10.1 | Redis client |
| xUnit | 2.9.3 | Test framework |
| Shouldly | 4.3.0 | Assertion library |
| Swashbuckle | 10.1.0 | Swagger UI |
