# Implementation Plan: OIDC AuthN/AuthZ Configuration

**Branch**: `001-oidc-role-authorization` | **Date**: 2026-03-08 | **Spec**: `F:/Source/Repos/Roman.RedisManager/specs/001-oidc-role-authorization/spec.md`
**Input**: Feature specification from `F:/Source/Repos/Roman.RedisManager/specs/001-oidc-role-authorization/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `f:/Source/Repos/Roman.RedisManager/backend/.specify/templates/plan-template.md` for the execution workflow.

## Summary

Secure all protected Redis-Manager API operations using OpenID Connect-backed bearer-token authentication, normalize provider-specific claims into internal application roles, and enforce authorization using both global permissions and Redis server group-specific overrides configured declaratively in application settings.

## Technical Context

**Language/Version**: C# with .NET 10.0  
**Primary Dependencies**: ASP.NET Core Authentication/Authorization, Wolverine 5.9.2, StackExchange.Redis 2.10.1, Options pattern configuration  
**Storage**: Application configuration (`appsettings*.json`) for identity providers, role mappings, and permission overrides; no new persistent datastore  
**Testing**: xUnit 2.9.3, Shouldly 4.3.0, mutation tooling via Stryker scripts in `.specify/scripts/powershell/`  
**Target Platform**: ASP.NET Core backend API hosted on Windows/Linux containers and local development runtime  
**Project Type**: Web service (backend API serving SPA static assets)  
**Performance Goals**: Authentication and authorization checks must not materially regress existing API behavior; authorization decision latency should support <= 1s for p95 policy evaluation scenarios from spec SC-003  
**Constraints**: Clean Architecture dependency direction, Wolverine-mediated controller flow, config-driven provider and role mapping extensibility, no provider-specific claim parsing in domain layer  
**Scale/Scope**: All protected API endpoints and group-scoped operations; role/claim mapping across EntraID, Google, and generic OIDC plus extensible provider entries

**Mutation Quality Loop**: Tests are expected in scope. Execution plan MUST include `run -> analyze -> improve -> rerun` using:
- `mutation: run` (generates Stryker HTML/JSON reports)
- `mutation: analyze` (survivor/no-coverage triage)
- `mutation: compare` (delta verification)
- documented non-actionable exceptions when applicable

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Clean Architecture**: PASS. Planned changes remain in Web/Infrastructure/Application layers for authentication pipeline and policy orchestration; Domain remains provider-agnostic.
- **II. CQRS via Wolverine**: PASS. Existing controller dispatch via `IMessageBus` remains unchanged; authn/authz concerns are middleware/policy-level and do not bypass CQRS.
- **III. Repository Pattern**: PASS. Feature does not require new data repositories; existing repository boundaries remain intact.
- **IV. Test Discipline**: PASS WITH ACTION. New or changed tests will use `[Fact]`, Shouldly, and mandatory `// Arrange`, `// Act`, `// Assert` comments, plus mutation loop evidence.
- **V. Consistency & Simplicity**: PASS. Planned configuration objects will follow required annotations and options validation patterns.

## Project Structure

### Documentation (this feature)

```text
specs/001-oidc-role-authorization/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── authn-authz-behavior.md
│   └── configuration-schema.md
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Roman.RedisManager.Domain/
│   │   ├── Configuration/
│   │   ├── Entities/
│   │   └── Repositories/
│   ├── Roman.RedisManager.Application/
│   │   └── CQRS/
│   ├── Roman.RedisManager.Infrastructure/
│   │   ├── Configuration/
│   │   ├── Repositories/
│   │   └── Redis/
│   └── Roman.RedisManager.Web/
│       ├── Controllers/
│       ├── ProblemDetails/
│       ├── Program.cs
│       └── appsettings.json
└── tests/
	└── Roman.RedisManager.Tests/
		├── Application/
		├── Infrastructure/
		└── Web/
```

**Structure Decision**: Use existing backend web-service structure under `backend/` and add only feature-focused classes/configuration/tests within current projects to preserve constitution layering and CQRS conventions.

## Complexity Tracking

No constitution violations expected; table not required.

## Constitution Check (Post-Design Re-Check)

- **I. Clean Architecture**: PASS. Designed artifacts keep identity normalization and policy evaluation in Web/Infrastructure boundaries; no domain-provider coupling introduced.
- **II. CQRS via Wolverine**: PASS. Authorization is policy/middleware-based and preserves controller-to-handler dispatch conventions.
- **III. Repository Pattern**: PASS. No repository boundary changes were introduced in design artifacts.
- **IV. Test Discipline**: PASS WITH ACTION. Test plan explicitly includes xUnit/Shouldly style and mutation quality loop evidence requirements.
- **V. Consistency & Simplicity**: PASS. Configuration-driven approach and startup options validation remain the governing design mechanism.
