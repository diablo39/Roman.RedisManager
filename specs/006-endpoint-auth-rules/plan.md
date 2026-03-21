# Implementation Plan: Endpoint Authorisation Rules

**Branch**: `003-endpoint-auth-rules` | **Date**: 2026-03-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-endpoint-auth-rules/spec.md`

## Summary

Replace the existing granular group-based authorization system (ReadKeys, DeleteKeysByGroup policies with per-group overrides) with two simple role-based policies: "reader" for all data-returning endpoints and "editor" for all data-modifying endpoints. Remove group permission infrastructure, apply fail-closed authorization via fallback policy, configure OpenAPI security documentation, and retain the configurable claim-to-role mapping system for deployment flexibility.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: ASP.NET Core 10.0, Wolverine 5.9.2
**Storage**: N/A (authorization is stateless, configuration-driven)
**Testing**: xUnit 2.9.3, Shouldly 4.3.0
**Target Platform**: Windows/Linux server (ASP.NET Core web API)
**Project Type**: Web service (REST API)
**Performance Goals**: <50ms authorization overhead per request (SC-005)
**Constraints**: No new NuGet packages (constitution constraint). Must use .NET 10 built-in OpenAPI support.
**Scale/Scope**: 23 endpoints across 9 controllers

**Mutation Quality Loop**: Tests are in scope. Implementation MUST follow `run -> analyze -> improve -> rerun`. Output artifacts: `specs/003-endpoint-auth-rules/test-reports/` for HTML/JSON mutation reports.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Architecture | PASS | Authorization lives in Web layer (correct). Configuration classes in Domain (correct for Options pattern). No cross-layer violations. |
| II. CQRS via Wolverine | PASS | No changes to CQRS pattern. Controllers still dispatch via Wolverine. Authorization is orthogonal to message handling. |
| III. Repository Pattern | PASS | No repository changes needed. |
| IV. Test Discipline | PASS | Tests will use xUnit/Shouldly with real implementations. AAA section comments required. |
| V. Consistency & Simplicity | PASS | Block-scoped namespaces, Options pattern with validation, kebab-case routes preserved. No new NuGet packages. |

**Post-Phase 1 Re-check**: All gates still pass. The design uses ASP.NET Core built-in authorization (no new packages), Options pattern for configuration, and standard `[Authorize]`/`[AllowAnonymous]` attributes.

## Project Structure

### Documentation (this feature)

```text
specs/003-endpoint-auth-rules/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── authorization-responses.md
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
backend/src/Roman.RedisManager.Web/
├── Authorization/
│   ├── AuthorizationPolicies.cs           # MODIFY: Replace policy names
│   ├── AuthorizationDecisionLogger.cs     # MODIFY: Simplify for reader/editor
│   ├── NormalizedRoleClaimsTransformation.cs  # RETAIN (no changes)
│   ├── RoleClaimMappingEvaluator.cs       # RETAIN (no changes)
│   ├── IGroupContextAccessor.cs           # DELETE
│   ├── RouteGroupContextAccessor.cs       # DELETE
│   ├── Requirements/
│   │   └── GroupPermissionRequirement.cs  # DELETE
│   └── Handlers/
│       └── GroupPermissionAuthorizationHandler.cs  # DELETE
│   └── CustomAuthorizationResultHandler.cs # CREATE: Custom 403 response with policy name
├── Authentication/                         # RETAIN (no changes)
├── Controllers/
│   ├── ErrorController.cs                 # MODIFY: Add [AllowAnonymous]
│   ├── TestController.cs                  # MODIFY: Add [AllowAnonymous]
│   ├── RedisKeysController.cs             # MODIFY: Update [Authorize] policies
│   ├── RedisServerGroupsController.cs     # MODIFY: Update [Authorize] policies
│   ├── RedisInfoController.cs             # MODIFY: Add [Authorize] with Reader
│   └── RedisDataTypes/
│       ├── RedisStringsController.cs      # MODIFY: Add [Authorize] per action
│       ├── RedisHashesController.cs       # MODIFY: Add [Authorize] per action
│       ├── RedisListsController.cs        # MODIFY: Add [Authorize] per action
│       ├── RedisSetsController.cs         # MODIFY: Add [Authorize] per action
│       └── RedisSortedSetsController.cs   # MODIFY: Add [Authorize] per action
├── OpenApi/
│   ├── SecuritySchemeTransformer.cs              # CREATE: OpenAPI document transformer for Bearer scheme
│   └── SecurityRequirementOperationTransformer.cs # CREATE: OpenAPI operation transformer for per-endpoint policy requirements
└── Program.cs                             # MODIFY: Update DI registrations

backend/src/Roman.RedisManager.Domain/
├── Configuration/
│   └── AuthorizationPermissionConfiguration.cs  # DELETE
│   └── RedisServerGroupConfiguration.cs         # MODIFY: Remove AuthorizationOverrides
├── Entities/
│   └── AuthorizationPolicyTypes.cs              # DELETE

backend/tests/Roman.RedisManager.Tests/
└── Authorization/                                # CREATE: Authorization policy tests

backend/src/Roman.RedisManager.Web/appsettings.json  # MODIFY: Remove Permissions section
```

**Structure Decision**: Existing project structure retained. Two new files created in a new `OpenApi/` folder under Web (`SecuritySchemeTransformer.cs` and `SecurityRequirementOperationTransformer.cs`). One new file for custom authorization responses (`CustomAuthorizationResultHandler.cs`). New test files for authorization validation.

## Complexity Tracking

No constitution violations to justify. All changes use existing framework capabilities without new packages.
