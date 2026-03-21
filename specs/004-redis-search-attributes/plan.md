# Implementation Plan: Redis Search Metadata Expansion

**Branch**: `001-redis-search-attributes` | **Date**: 2026-03-07 | **Spec**: `F:\Source\Repos\Roman.RedisManager\specs\001-redis-search-attributes\spec.md`  
**Input**: Feature specification from `F:\Source\Repos\Roman.RedisManager\specs\001-redis-search-attributes\spec.md`

## Summary

Extend Redis key search results to return key metadata (`type`, `ttlMilliseconds`) plus derived usability field (`hasExpiration`) while controlling Redis load through request capping and repository-level pipelined metadata retrieval. The design keeps CQRS/controller flow unchanged, enriches data in repository results, and preserves continuation-token behavior.

## Technical Context

**Language/Version**: C# on .NET 10.0  
**Primary Dependencies**: ASP.NET Core 10, Wolverine 5.9.2, StackExchange.Redis 2.10.1  
**Storage**: Redis (standalone and cluster), appsettings-based options for topology and limits  
**Testing**: xUnit 2.9.3, Shouldly 4.3.0, integration-style repository tests against Redis containers, mutation via Stryker scripts  
**Target Platform**: ASP.NET Core backend service on Windows/Linux (local Docker-based Redis for development)  
**Project Type**: Clean Architecture web API backend serving SPA assets  
**Performance Goals**: 95% of capped-size search requests complete within 2 seconds under normal conditions; metadata enrichment avoids unbounded RTT amplification  
**Constraints**: Controller -> Wolverine -> handler flow only; repository interfaces in Domain and implementations in Infrastructure; no backward compatibility requirement; page size must be capped  
**Scale/Scope**: Search endpoint contract update, domain model enrichment for search key items, repository enrichment path, configuration for max page size, and corresponding application/infrastructure/web tests

**Mutation Quality Loop**: Tests are in scope. The implementation will execute `run -> analyze -> improve -> rerun` and capture evidence in:
- `F:\Source\Repos\Roman.RedisManager\backend\tests\Roman.RedisManager.Tests\StrykerOutput\**\reports\mutation-report.html`
- `F:\Source\Repos\Roman.RedisManager\backend\tests\Roman.RedisManager.Tests\StrykerOutput\**\reports\mutation-report.json`
- Comparison/analyzer outputs from:
- `F:\Source\Repos\Roman.RedisManager\backend\.specify\scripts\powershell\analyze-surviving-mutants.ps1`
- `F:\Source\Repos\Roman.RedisManager\backend\.specify\scripts\powershell\compare-mutation-reports.ps1`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate

- **Clean Architecture**: PASS. Planned changes stay within Domain/Application/Infrastructure/Web boundaries.
- **CQRS via Wolverine**: PASS. Endpoint flow remains controller dispatch to static handler via `IMessageBus`.
- **Repository Pattern**: PASS. Metadata enrichment is planned in `IRedisRepository`/`RedisRepository`, returning domain entities.
- **Test Discipline**: PASS. xUnit + Shouldly tests planned; no mocking framework introduced.
- **Consistency & Simplicity**: PASS. Block-scoped namespaces, options pattern for configuration, and existing naming conventions preserved.

### Post-Design Gate (After Phase 1)

- **Clean Architecture**: PASS. Data model and contracts keep infra concerns out of Application/Web DTO mapping concerns.
- **CQRS via Wolverine**: PASS. Contracts reflect existing query/handler/controller boundaries.
- **Repository Pattern**: PASS. Request-volume optimization is repository-driven (pipelined metadata retrieval).
- **Test Discipline**: PASS. Quickstart includes mutation loop and artifact evidence requirements.
- **Consistency & Simplicity**: PASS. Added fields and config remain additive and convention-aligned.

## Project Structure

### Documentation (this feature)

```text
F:/Source/Repos/Roman.RedisManager/specs/001-redis-search-attributes/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── redis-keys-search-response.md
└── tasks.md  # created later by /speckit.tasks
```

### Source Code (repository root)

```text
F:/Source/Repos/Roman.RedisManager/backend/
├── src/
│   ├── Roman.RedisManager.Domain/
│   │   ├── Entities/
│   │   ├── Configuration/
│   │   └── Repositories/
│   ├── Roman.RedisManager.Application/
│   │   └── CQRS/
│   ├── Roman.RedisManager.Infrastructure/
│   │   └── Repositories/
│   └── Roman.RedisManager.Web/
│       ├── Controllers/
│       ├── Program.cs
│       └── appsettings.json
└── tests/
	└── Roman.RedisManager.Tests/
		├── Application/CQRS/
		├── Infrastructure/Repositories/
		└── Web/Controllers/
```

**Structure Decision**: Use the existing four-layer backend structure. The feature is implemented as a vertical slice across Domain entity/contract updates, Application CQRS DTO mapping, Infrastructure repository enrichment, Web contract exposure, and tests.

## Complexity Tracking

No constitution violations are required for this feature.
