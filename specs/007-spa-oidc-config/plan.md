# Implementation Plan: SPA OIDC Authentication Bootstrap

**Branch**: `[007-spa-oidc-config]` | **Date**: 2026-03-22 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-spa-oidc-config/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Provide a backend-managed, unauthenticated authentication bootstrap endpoint that returns browser-safe OIDC configuration for enabled providers plus a deterministic endpoint-level availability state so a Vue 3 SPA can use a generic OIDC client (`oidc-client-ts`) with multiple providers (including Generic OIDC) and no frontend redeploy for environment configuration changes.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: ASP.NET Core 10, Wolverine 5.9.2, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1, StackExchange.Redis 2.10.1  
**Storage**: N/A (configuration-backed read model from Options)  
**Testing**: xUnit 2.9.3 + Shouldly 4.3.0 + Stryker mutation testing (`dotnet dotnet-stryker`)  
**Target Platform**: Linux/Windows container-hosted ASP.NET Core backend serving SPA assets  
**Project Type**: Web API backend (Clean Architecture with CQRS)  
**Performance Goals**: Bootstrap endpoint p95 < 100 ms from in-memory options; zero external network calls on request path  
**Constraints**: Must expose only browser-safe fields; never expose secrets/signing keys; must remain aligned with configured JWT provider validation; normal endpoint responses must stay DTO-based and constitution-compliant; controller must dispatch via Wolverine  
**Scale/Scope**: Single endpoint + one CQRS query handler + DTOs + config model extension + tests for mapping/authorization behavior

**Mutation Quality Loop**: If tests are added/updated for this feature, execute `run -> analyze -> improve -> rerun` with artifacts under `tests/Roman.RedisManager.Tests/StrykerOutput/**` (HTML/JSON) and include analysis of surviving/no-coverage mutants plus explicit non-actionable justifications where applicable.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Clean Architecture: PASS. Planned changes are in Domain configuration model, Application CQRS handler, Web controller, and Program wiring only.
- CQRS via Wolverine: PASS. New endpoint will call `_bus.InvokeAsync<TResult>(new Query { ... })`; handler will be static and co-located with message/DTO/result types.
- Repository Pattern: PASS. No new repository or persistence path is needed.
- Test Discipline: PASS with condition. Tests will use xUnit + Shouldly + `// Arrange` `// Act` `// Assert`; mutation loop required when tests change.
- Consistency & Simplicity: PASS. Block-scoped namespaces, options pattern, and existing route style will be retained.

No constitution violations currently identified.

## Project Structure

### Documentation (this feature)

```text
specs/007-spa-oidc-config/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── auth-bootstrap.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Roman.RedisManager.Domain/
│   │   └── Configuration/
│   ├── Roman.RedisManager.Application/
│   │   └── CQRS/
│   └── Roman.RedisManager.Web/
│       ├── Controllers/
│       └── Program.cs
└── tests/
	└── Roman.RedisManager.Tests/
		├── Application/
		└── Web/
```

**Structure Decision**: Use existing backend Clean Architecture structure; add a new query/handler pair and controller action in-place, plus tests in existing test project.

## Phase 0: Research Plan

Research tasks derived from technical context and spec:

1. Confirm generic SPA OIDC client choice that supports any standards-compliant OIDC server.
2. Identify required `oidc-client-ts` browser configuration fields and optional discovery override fields.
3. Map current backend provider configuration to browser-safe subset and identify fields that must never be returned.
4. Confirm endpoint exposure pattern consistent with existing auth middleware and fallback authorization policy.

Output artifact: `research.md` with decisions, rationale, alternatives.

## Phase 1: Design & Contracts Plan

1. Define feature data model for bootstrap DTOs and endpoint availability semantics in `data-model.md`.
2. Define external API contract in `contracts/auth-bootstrap.openapi.yaml`.
3. Define end-to-end verification flow (manual/API level) in `quickstart.md`.
4. Update agent context by running `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`.
5. Re-run constitution check against design artifacts.

## Post-Design Constitution Check

- Clean Architecture: PASS. Contract and data model map to Application/Web layers without cross-layer violations.
- CQRS via Wolverine: PASS. Contract targets a controller endpoint that dispatches to query handler only.
- Repository Pattern: PASS. Design remains configuration-driven with no repository additions.
- Test Discipline: PASS. Design includes explicit test scope and mutation gate instructions.
- Consistency & Simplicity: PASS. No deviation from existing option binding and route conventions.

## Complexity Tracking

No constitution exceptions required.
