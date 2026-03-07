# Implementation Plan: Unified Redis Key Search Cursor

**Branch**: `[001-cursor-token-unification]` | **Date**: 2026-03-07 | **Spec**: `F:\Source\Repos\Roman.RedisManager\specs\001-cursor-token-unification\spec.md`
**Input**: Feature specification from `F:\Source\Repos\Roman.RedisManager\specs\001-cursor-token-unification\spec.md`

## Summary

Unify key-search pagination into a single opaque continuation token contract for both standalone and cluster Redis groups. Keep internal standalone and cluster scan strategies separate, but hide all topology-specific cursor details from API consumers. Replace the current mixed contract (`cursor` as string input, numeric cursor output, optional `nodeCursors`) with one outward token flow and explicit invalid-token behavior.

## Technical Context

**Language/Version**: C# on .NET 10.0  
**Primary Dependencies**: ASP.NET Core Web API, Wolverine 5.9.2, StackExchange.Redis 2.10.1, Microsoft.Extensions.Options, Swashbuckle 10.1.0  
**Storage**: Redis (standalone and cluster topologies) via StackExchange.Redis; app configuration in `appsettings*.json`  
**Testing**: xUnit 2.9.3 + Shouldly 4.3.0; integration-style repository tests with real implementations  
**Target Platform**: ASP.NET Core backend service (Windows/Linux container capable)  
**Project Type**: Clean Architecture backend web-service (`Domain -> Application -> Infrastructure -> Web`)  
**Performance Goals**: Preserve current pagination behavior and avoid additional network round-trips per request beyond the existing SCAN flow; endpoint remains bounded by page size and Redis SCAN latency  
**Constraints**: Single opaque token contract, topology details hidden from clients, Wolverine-mediated controllers only, repository pattern preserved, no new architectural layers, contract breaking allowed  
**Scale/Scope**: One API endpoint contract (`GET /api/redis-keys`) plus supporting query/result models and Redis repository scan state handling for standalone and cluster groups

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clean Architecture**: PASS. Planned changes stay in existing layers: contract models in Application/Web, continuation handling in Infrastructure, and domain abstractions in Domain.
- **CQRS via Wolverine**: PASS. Controller continues dispatching through `IMessageBus.InvokeAsync<T>()`; no direct repository calls from controllers.
- **Repository Pattern**: PASS. Cursor logic remains in repository implementation behind domain interface; no infrastructure type leakage.
- **Test Discipline**: PASS (planned). Add/adjust xUnit `[Fact]` tests with Shouldly and real implementations; no mocking framework.
- **Consistency and Simplicity**: PASS. Maintain block-scoped namespaces, naming conventions, and Options usage.

No constitutional violations identified.

## Project Structure

### Documentation (this feature)

```text
specs/001-cursor-token-unification/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── redis-keys-search-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Roman.RedisManager.Domain/
│   │   ├── Entities/
│   │   └── Repositories/
│   ├── Roman.RedisManager.Application/
│   │   └── CQRS/
│   ├── Roman.RedisManager.Infrastructure/
│   │   └── Repositories/
│   └── Roman.RedisManager.Web/
│       ├── Controllers/
│       ├── Program.cs
│       ├── Roman.RedisManager.Web.http
│       └── Roman.RedisManager.Web.json
└── tests/
	└── Roman.RedisManager.Tests/
		├── Application/
		├── Infrastructure/
		└── Web/
```

**Structure Decision**: Use existing Clean Architecture backend structure in `backend/src` and `backend/tests`; limit feature changes to cursor-related models/handlers/controllers/contracts for key-search flows.

## Complexity Tracking

No constitutional violations requiring justification.

## Post-Design Constitution Check

- **Clean Architecture**: PASS. Data model and contract design keep boundaries intact and do not require cross-layer dependency violations.
- **CQRS via Wolverine**: PASS. Contract updates remain query/handler/controller oriented with Wolverine dispatch unchanged.
- **Repository Pattern**: PASS. Internal cursor state remains behind repository abstractions; no Redis client types are exposed in API contracts.
- **Test Discipline**: PASS. Planned test set covers valid continuation, invalid token, context mismatch, and completion semantics.
- **Consistency and Simplicity**: PASS. Design keeps one outward token field and removes contract complexity (`nodeCursors`) from consumers.

Post-design review confirms no constitution gate failures.
