# Tasks: OIDC AuthN/AuthZ Configuration

**Input**: Design documents from `/specs/001-oidc-role-authorization/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: This feature explicitly requires tests and mutation quality loop execution.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize feature artifacts and config placeholders used by all stories.

- [X] T001 Create security configuration scaffold sections in `backend/src/Roman.RedisManager.Web/appsettings.json`
- [X] T002 [P] Add development security placeholder values in `backend/src/Roman.RedisManager.Web/appsettings.Development.json`
- [X] T003 [P] Create implementation evidence log template in `specs/001-oidc-role-authorization/implementation-notes.md`
- [X] T004 [P] Add authn/authz manual API scenario placeholders in `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core authn/authz configuration and policy infrastructure required before user stories.

**CRITICAL**: No user story implementation starts until this phase is complete.

- [X] T005 Create provider options classes in `backend/src/Roman.RedisManager.Domain/Configuration/OidcProviderConfiguration.cs`
- [X] T006 [P] Create role and claim mapping options classes in `backend/src/Roman.RedisManager.Domain/Configuration/AuthorizationRoleMappingConfiguration.cs`
- [X] T007 [P] Create permission options classes and model group authorization overrides in `backend/src/Roman.RedisManager.Domain/Configuration/RedisServerGroupConfiguration.cs`
- [X] T008 Create policy action and decision reason domain types in `backend/src/Roman.RedisManager.Domain/Entities/AuthorizationPolicyTypes.cs`
- [X] T009 Implement options binding and startup validation in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T010 [P] Add provider profile abstraction interface in `backend/src/Roman.RedisManager.Web/Authentication/IOidcProviderProfile.cs`
- [X] T011 [P] Implement provider profile resolver in `backend/src/Roman.RedisManager.Web/Authentication/OidcProviderProfileResolver.cs`
- [X] T012 Configure shared authentication and authorization service registration in `backend/src/Roman.RedisManager.Web/Authentication/AuthenticationServiceCollectionExtensions.cs`
- [X] T013 [P] Add request group context accessor contract in `backend/src/Roman.RedisManager.Web/Authorization/IGroupContextAccessor.cs`
- [X] T014 Implement request group context accessor in `backend/src/Roman.RedisManager.Web/Authorization/RouteGroupContextAccessor.cs`

**Checkpoint**: Foundation is ready for independent user story implementation.

---

## Phase 3: User Story 1 - Unified Authentication Entry (Priority: P1) 🎯 MVP

**Goal**: Support bearer authentication for protected API endpoints using configured OIDC providers.

**Independent Test**: Authenticate via bearer token flow against a protected API endpoint and verify authenticated access.

### Tests for User Story 1

- [X] T015 [P] [US1] Consolidate authentication integration coverage to bearer-token flow in `backend/tests/Roman.RedisManager.Tests/Web/Authentication/BearerAuthenticationFlowTests.cs`
- [X] T016 [P] [US1] Add bearer authentication integration tests in `backend/tests/Roman.RedisManager.Tests/Web/Authentication/BearerAuthenticationFlowTests.cs`
- [X] T017 [US1] Add invalid provider/misconfiguration rejection tests in `backend/tests/Roman.RedisManager.Tests/Web/Authentication/OidcProviderValidationTests.cs`

### Implementation for User Story 1

- [X] T018 [P] [US1] Configure bearer authentication defaults in `backend/src/Roman.RedisManager.Web/Authentication/AuthenticationServiceCollectionExtensions.cs`
- [X] T019 [P] [US1] Implement EntraId provider profile in `backend/src/Roman.RedisManager.Web/Authentication/Providers/EntraIdProviderProfile.cs`
- [X] T020 [P] [US1] Implement Google provider profile in `backend/src/Roman.RedisManager.Web/Authentication/Providers/GoogleProviderProfile.cs`
- [X] T021 [P] [US1] Implement GenericOidc provider profile in `backend/src/Roman.RedisManager.Web/Authentication/Providers/GenericOidcProviderProfile.cs`
- [X] T022 [US1] Wire bearer handler through profile resolver in `backend/src/Roman.RedisManager.Web/Authentication/AuthenticationServiceCollectionExtensions.cs`
- [X] T023 [US1] Apply default authorization requirement for protected endpoints in `backend/src/Roman.RedisManager.Web/Program.cs`

**Checkpoint**: User Story 1 is independently testable and delivers unified authentication entry.

---

## Phase 4: User Story 2 - Role Normalization Across Providers (Priority: P1)

**Goal**: Normalize provider-specific claims into internal roles used by endpoint role authorization.

**Independent Test**: Validate one internal role resolves correctly from at least two providers with different claim shapes and that unmapped claims deny role-protected operations.

### Tests for User Story 2

- [X] T024 [P] [US2] Add claim-to-role mapping evaluator tests in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/RoleClaimMappingEvaluatorTests.cs`
- [X] T025 [P] [US2] Add principal transformation tests for normalized roles in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/PrincipalRoleTransformationTests.cs`
- [X] T026 [US2] Add unmapped-claims denial tests for role-protected endpoints in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/UnmappedClaimsAuthorizationTests.cs`

### Implementation for User Story 2As a Redis‑Manager administrator
I want the API secured via OpenID Connect so command-line and third-party clients (using JWTs) can authenticate.
I want three out‑of‑the‑box identity providers supported – EntraID, Google and a generic OIDC issuer – and I’d like to be able to plug in others later by adding a configuration section.

I need to declare a set of application‑level roles in appsettings.json and then map those roles to whatever claim keys the external provider emits (groups, roles, custom claims), similar to the groups mapping in AKHQ. This mapping allows me to say “our ‘redis‑reader’ role corresponds to the admin_group claim value in EntraID and to the readers group in Google,” so the middleware can normalize incoming principals and [Authorize(Roles = "redis‑reader")] works consistently no matter who issued the token.

Additionally, permissions shouldn’t be one‑size‑fits‑all across every server group. On the configuration side I want to specify overrides per Redis server group: e.g. users in the “editor” role can modify keys in the global default, but only “admins” can make changes in the prod-cache group. When a request arrives the authorization pipeline will examine both the normalized role and the target group ID and allow or deny the action accordingly.

Acceptance criteria include:

Authentication via bearer tokens, using OIDC for EntraID/Google/generic providers.
A configuration section for defining roles and their claim‑value mappings.
A configuration section for per‑group permission overrides.
Middleware that transforms incoming claims into internal roles and enforces both global and group‑specific policies.
This setup lets me manage all identity and authorization declaratively in config, keep the domain code unconcerned with the source of truth, and change mappings/overrides without redeploying.

- [X] T027 [P] [US2] Implement role-claim mapping evaluator in `backend/src/Roman.RedisManager.Web/Authorization/RoleClaimMappingEvaluator.cs`
- [X] T028 [P] [US2] Implement claims principal transformer in `backend/src/Roman.RedisManager.Web/Authorization/NormalizedRoleClaimsTransformation.cs`
- [X] T029 [US2] Register claims transformation and role mapping services in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T030 [US2] Apply normalized role authorization attributes to read endpoints in `backend/src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs`
- [X] T031 [US2] Apply normalized role authorization attributes to key lookup endpoints in `backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`

**Checkpoint**: User Story 2 is independently testable with provider-agnostic role authorization behavior.

---

## Phase 5: User Story 3 - Group-Specific Authorization Overrides (Priority: P1)

**Goal**: Enforce group-aware authorization using global permissions with group override precedence.

**Independent Test**: Confirm one role is allowed globally but denied for a restricted group while admin role remains allowed for that group.

### Tests for User Story 3

- [X] T032 [P] [US3] Add group override precedence tests in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/GroupOverridePrecedenceTests.cs`
- [X] T033 [P] [US3] Add global fallback authorization tests in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/GlobalPermissionFallbackTests.cs`
- [X] T034 [US3] Add malformed or missing group context denial tests in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/GroupContextValidationTests.cs`

### Implementation for User Story 3

- [X] T035 [P] [US3] Implement group-aware authorization requirement in `backend/src/Roman.RedisManager.Web/Authorization/Requirements/GroupPermissionRequirement.cs`
- [X] T036 [P] [US3] Implement group-aware authorization handler in `backend/src/Roman.RedisManager.Web/Authorization/Handlers/GroupPermissionAuthorizationHandler.cs`
- [X] T037 [US3] Register group authorization policies in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T038 [US3] Apply group-scoped mutation policy to delete endpoint as initial anchor and map remaining mutation actions in policy resolver in `backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`
- [X] T039 [US3] Add authorization decision structured logging in `backend/src/Roman.RedisManager.Web/Authorization/AuthorizationDecisionLogger.cs`

**Checkpoint**: User Story 3 is independently testable with deterministic group override behavior.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, mutation quality gate, and documentation alignment across all stories.

- [X] T040 [P] Add configuration examples for all providers in `specs/001-oidc-role-authorization/identity-configuration.md`
- [X] T041 [P] Update operational validation steps in `specs/001-oidc-role-authorization/quickstart.md`
- [X] T042 Run baseline test suite and record results in `specs/001-oidc-role-authorization/implementation-notes.md`
- [X] T043 Run mutation tests and capture HTML/JSON artifact paths using `backend/.specify/scripts/powershell/run-mutation-tests.ps1` in `specs/001-oidc-role-authorization/mutation-report-notes.md`
- [X] T044 Analyze surviving and no-coverage mutants using `backend/.specify/scripts/powershell/analyze-surviving-mutants.ps1` in `specs/001-oidc-role-authorization/mutation-report-notes.md`
- [X] T045 Improve assertions for actionable mutant findings in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/GroupOverridePrecedenceTests.cs`
- [X] T046 Improve assertions for actionable mutant findings in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/RoleClaimMappingEvaluatorTests.cs`
- [X] T047 Re-run mutation tests and record deltas via `backend/.specify/scripts/powershell/run-mutation-tests.ps1` and `backend/.specify/scripts/powershell/compare-mutation-reports.ps1` in `specs/001-oidc-role-authorization/mutation-report-notes.md`
- [X] T048 Document approved non-actionable survivors or final clean result in `specs/001-oidc-role-authorization/mutation-report-notes.md`
- [X] T049 [P] Add provider matrix authentication tests covering EntraId, Google, and GenericOidc profiles in `backend/tests/Roman.RedisManager.Tests/Web/Authentication/ProviderMatrixAuthenticationTests.cs`
- [X] T050 [P] Add configuration-only provider extensibility test (new provider added by config without code changes) in `backend/tests/Roman.RedisManager.Tests/Web/Authentication/ProviderConfigurationExtensibilityTests.cs`
- [X] T051 [P] Add structured authorization decision log assertion tests for provider key, normalized roles, group id, decision, and reason in `backend/tests/Roman.RedisManager.Tests/Web/Authorization/AuthorizationDecisionLoggingTests.cs`
- [X] T052 Validate non-redeploy operational change process for one role mapping update and one group override update and record evidence in `specs/001-oidc-role-authorization/implementation-notes.md`
- [X] T053 Enforce and verify test conventions (`MethodName_Condition_ExpectedBehavior`, `// Arrange`, `// Act`, `// Assert`) across all new tests in `specs/001-oidc-role-authorization/implementation-notes.md`
- [X] T054 Add FR-to-task traceability matrix (implementation + tests) in `specs/001-oidc-role-authorization/implementation-notes.md`
- [X] T055 Measure authorization decision latency and record p95 <= 1s evidence for representative role/group scenarios in `specs/001-oidc-role-authorization/implementation-notes.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 (Setup): no dependencies.
- Phase 2 (Foundational): depends on Phase 1 and blocks all user story work.
- Phase 3 (US1): depends on Phase 2.
- Phase 4 (US2): depends on Phase 2; can proceed in parallel with US1 after Phase 2.
- Phase 5 (US3): depends on Phase 2 and US2 role normalization outputs.
- Phase 6 (Polish): depends on completion of Phases 3, 4, and 5.

### User Story Dependencies

- US1: independent after foundational setup.
- US2: independent after foundational setup.
- US3: depends on normalized role infrastructure from US2 and foundational group context components.

### Within Each User Story

- Tests first and failing before implementation.
- Configuration/domain contracts before service registration.
- Handler/policy implementation before controller policy application.
- Story checkpoint validation before entering final polish.

## Parallel Opportunities

- Setup tasks `T002`, `T003`, and `T004` can run in parallel.
- Foundational tasks `T006`, `T007`, `T010`, `T011`, and `T013` can run in parallel.
- US1 provider profile tasks `T019`, `T020`, and `T021` can run in parallel.
- US2 test tasks `T024` and `T025` can run in parallel.
- US3 requirement/handler tasks `T035` and `T036` can run in parallel.
- Polish documentation tasks `T040` and `T041` can run in parallel.
- Provider quality tasks `T049`, `T050`, and `T051` can run in parallel.

---

## Parallel Example: User Story 1

```bash
Task: T015 backend/tests/Roman.RedisManager.Tests/Web/Authentication/BearerAuthenticationFlowTests.cs
Task: T016 backend/tests/Roman.RedisManager.Tests/Web/Authentication/BearerAuthenticationFlowTests.cs
Task: T019 backend/src/Roman.RedisManager.Web/Authentication/Providers/EntraIdProviderProfile.cs
Task: T020 backend/src/Roman.RedisManager.Web/Authentication/Providers/GoogleProviderProfile.cs
Task: T021 backend/src/Roman.RedisManager.Web/Authentication/Providers/GenericOidcProviderProfile.cs
```

## Parallel Example: User Story 2

```bash
Task: T024 backend/tests/Roman.RedisManager.Tests/Web/Authorization/RoleClaimMappingEvaluatorTests.cs
Task: T025 backend/tests/Roman.RedisManager.Tests/Web/Authorization/PrincipalRoleTransformationTests.cs
Task: T027 backend/src/Roman.RedisManager.Web/Authorization/RoleClaimMappingEvaluator.cs
Task: T028 backend/src/Roman.RedisManager.Web/Authorization/NormalizedRoleClaimsTransformation.cs
```

## Parallel Example: User Story 3

```bash
Task: T032 backend/tests/Roman.RedisManager.Tests/Web/Authorization/GroupOverridePrecedenceTests.cs
Task: T033 backend/tests/Roman.RedisManager.Tests/Web/Authorization/GlobalPermissionFallbackTests.cs
Task: T035 backend/src/Roman.RedisManager.Web/Authorization/Requirements/GroupPermissionRequirement.cs
Task: T036 backend/src/Roman.RedisManager.Web/Authorization/Handlers/GroupPermissionAuthorizationHandler.cs
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) and verify bearer authentication flow.
3. Validate independently before progressing.

### Incremental Delivery

1. Deliver US1 for secure entry.
2. Add US2 for normalized role semantics.
3. Add US3 for group-aware enforcement.
4. Run Phase 6 mutation loop and documentation closure.

### Parallel Team Strategy

1. Team A: US1 authentication profiles and scheme wiring.
2. Team B: US2 role normalization and endpoint role attributes.
3. Team C: US3 group override requirement and handler.
4. Merge into Phase 6 for mutation hardening and release evidence.

---

## Notes

- All task lines follow required checklist format: `- [ ] T### [P?] [US?] Description with file path`.
- `[US#]` labels are used only for user story phases.
- Mutation loop tasks are explicitly included (`T043` to `T048`).
- FR and success-criteria coverage hardening tasks are included (`T049` to `T055`).
- Commit per task or coherent task group to simplify review and rollback.
