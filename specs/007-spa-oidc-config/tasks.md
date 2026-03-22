# Tasks: SPA OIDC Authentication Bootstrap

**Input**: Design documents from `/specs/007-spa-oidc-config/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/auth-bootstrap.openapi.yaml

**Tests**: This feature includes test tasks because specification and quickstart require verification plus mutation quality loop evidence.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label for story phases only (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an actionable target file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare feature files, config scaffolding, and API documentation entry points.

- [X] T001 Add feature request example for bootstrap endpoint in `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`
- [X] T002 Add baseline Security.Authentication frontend bootstrap placeholders in `backend/src/Roman.RedisManager.Web/appsettings.json`
- [X] T003 [P] Add development bootstrap placeholders for redirect/scope metadata in `backend/src/Roman.RedisManager.Web/appsettings.Development.json`
- [X] T004 [P] Add testing bootstrap placeholders for deterministic auth bootstrap tests in `backend/src/Roman.RedisManager.Web/appsettings.Testing.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core contracts and configuration model updates required by all stories.

**CRITICAL**: No user story tasks start before this phase is complete.

- [X] T005 Extend OIDC provider configuration with browser-safe bootstrap fields and validation in `backend/src/Roman.RedisManager.Domain/Configuration/OidcProviderConfiguration.cs`
- [X] T006 [P] Add endpoint-level bootstrap availability state/reason model in `backend/src/Roman.RedisManager.Domain/Configuration/OidcProviderConfiguration.cs`
- [X] T007 Create CQRS query/result/provider DTO contract for bootstrap retrieval in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T008 Create bootstrap mapper logic from provider config to browser-safe DTO in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T009 Add unauthenticated bootstrap controller endpoint dispatching via IMessageBus in `backend/src/Roman.RedisManager.Web/Controllers/AuthenticationBootstrapController.cs`
- [X] T010 Register/allow bootstrap route for pre-auth access and keep fallback policy behavior intact in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T011 [P] Add OpenAPI security transformer support for anonymous bootstrap operation in `backend/src/Roman.RedisManager.Web/OpenApi/SecurityRequirementOperationTransformer.cs`

**Checkpoint**: Foundation complete; user stories can now be implemented independently.

---

## Phase 3: User Story 1 - Frontend obtains login bootstrap settings (Priority: P1) 🎯 MVP

**Goal**: Return enabled providers and browser-safe OIDC sign-in settings for SPA startup.

**Independent Test**: Anonymous GET to `/api/authentication/bootstrap` returns enabled providers with required browser-safe fields and no secrets.

### Tests for User Story 1

- [X] T012 [P] [US1] Add query handler tests for enabled-provider filtering and required fields in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`
- [X] T013 [P] [US1] Add controller tests verifying anonymous access and response shape in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/AuthenticationBootstrapControllerTests.cs`

### Implementation for User Story 1

- [X] T014 [US1] Implement enabled-provider filtering in query handler in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T015 [US1] Implement required OIDC fields projection (`authority`, `clientId`, `redirectUri`, `scope`, `responseType`) in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T016 [US1] Ensure sensitive server-only fields are excluded from bootstrap DTO in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T017 [US1] Wire controller GET action to return bootstrap query result DTO in `backend/src/Roman.RedisManager.Web/Controllers/AuthenticationBootstrapController.cs`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Frontend authenticates against different OIDC servers (Priority: P2)

**Goal**: Support provider-neutral startup settings including generic OIDC and discovery override metadata.

**Independent Test**: For mixed provider configurations, bootstrap response contains provider-specific browser-safe metadata and deterministic endpoint-level availability state.

### Tests for User Story 2

- [X] T018 [US2] Add tests for GenericOidc provider mapping and discovery override projection in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`
- [X] T019 [US2] Add tests for deterministic endpoint-level unavailable reason codes in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`

### Implementation for User Story 2

- [X] T020 [US2] Implement provider-kind-neutral mapping path (EntraId, Google, GenericOidc) in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T021 [US2] Implement optional metadata override projection for browser discovery limitations in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T022 [US2] Implement deterministic endpoint-level unavailable reason code mapping in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T023 [US2] Update API contract examples for `bootstrapState` and unavailable reason codes in `specs/007-spa-oidc-config/contracts/auth-bootstrap.openapi.yaml`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Authentication configuration changes without frontend redeploy (Priority: P3)

**Goal**: Ensure backend configuration changes alter bootstrap output without frontend code changes.

**Independent Test**: Changing enabled providers or browser-safe fields in config changes bootstrap response after app restart/config reload.

### Tests for User Story 3

- [X] T024 [US3] Add tests for disabled providers not returned as available in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`
- [X] T025 [US3] Add tests for changed display name/settings reflected in bootstrap result in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`
- [X] T026 [P] [US3] Add web-level test for `bootstrapState = unavailable` DTO response when no providers are available in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/AuthenticationBootstrapControllerTests.cs`

### Implementation for User Story 3

- [X] T027 [US3] Implement no-available-provider handling result in query handler in `backend/src/Roman.RedisManager.Application/CQRS/AuthenticationBootstrapQueryHandler.cs`
- [X] T028 [US3] Implement controller translation to deterministic unavailable DTO contract in `backend/src/Roman.RedisManager.Web/Controllers/AuthenticationBootstrapController.cs`
- [X] T029 [US3] Document config-change verification workflow for operations in `specs/007-spa-oidc-config/quickstart.md`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality, docs, mutation gate evidence, and release readiness.

- [X] T030 [P] Run unit/integration tests for this feature scope and capture output notes in `specs/007-spa-oidc-config/quickstart.md`
- [X] T031 Run mutation tests for affected project and store HTML/JSON outputs under `backend/tests/Roman.RedisManager.Tests/StrykerOutput/Roman.RedisManager.Web`
- [X] T032 Analyze surviving/no-coverage mutants and classify actionable vs non-actionable findings in `specs/007-spa-oidc-config/checklists/mutation-analysis.md`
- [X] T033 Strengthen assertions to kill actionable mutants in `backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs`
- [X] T034 Rerun mutation tests and document score delta/exception justification in `specs/007-spa-oidc-config/checklists/mutation-analysis.md`
- [X] T035 [P] Final documentation sync for endpoint contract and usage examples in `doc/authentication-authorization-configuration.md`
- [X] T036 [P] Verify and document bootstrap endpoint p95 latency (<100 ms) evidence in `specs/007-spa-oidc-config/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 completion; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2 completion.
- **Phase 4 (US2)**: Depends on Phase 2 completion; can proceed after US1 MVP checkpoint if sequencing by priority.
- **Phase 5 (US3)**: Depends on Phase 2 completion; can proceed after US1 MVP checkpoint if sequencing by priority.
- **Phase 6 (Polish)**: Depends on all completed story phases selected for release.

### User Story Dependencies

- **US1 (P1)**: No dependency on other stories after Foundational phase.
- **US2 (P2)**: No strict dependency on US1, but reuses the same query handler and should merge after US1 core mapping.
- **US3 (P3)**: No strict dependency on US2; depends on core bootstrap endpoint path from US1.

### Within Each User Story

- Tests first (write/adjust), then handler logic, then controller/API contract updates, then story checkpoint validation.

### Parallel Opportunities

- Setup: T003 and T004 can run in parallel with T001/T002.
- Foundational: T006 and T011 can run in parallel once T005 starts.
- US1: T012 and T013 can run in parallel.
- US3: T026 can run in parallel with T024 or T025 because it targets a different file.
- Polish: T030, T035, and T036 can run in parallel; mutation loop tasks remain sequential.

---

## Parallel Example: User Story 1

```bash
# Parallel test creation for US1
Task: T012 backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs
Task: T013 backend/tests/Roman.RedisManager.Tests/Web/Controllers/AuthenticationBootstrapControllerTests.cs
```

## Parallel Example: User Story 3

```bash
# Parallel configuration-change behavior tests
Task: T024 backend/tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs
Task: T026 backend/tests/Roman.RedisManager.Tests/Web/Controllers/AuthenticationBootstrapControllerTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Complete Phase 1 and Phase 2.
2. Complete US1 tasks (T012-T017).
3. Validate independent US1 criteria.
4. Demo/deploy MVP bootstrap endpoint.

### Incremental Delivery

1. Deliver US1 (bootstrap core).
2. Add US2 (multi-provider discovery override behavior).
3. Add US3 (configuration change resilience).
4. Complete mutation-quality and documentation polish.

### Parallel Team Strategy

1. Team A: Configuration model and foundational controller/query wiring (Phase 2).
2. Team B: US1 tests and implementation after Phase 2 checkpoint.
3. Team C: US2 and US3 tests/contract/docs in parallel after US1 endpoint baseline stabilizes.

---

## Notes

- `[P]` tasks are parallelizable when editing different files and not blocked by incomplete dependencies.
- User story labels are only used in story phases as required.
- All tasks include explicit file paths and are ready for direct execution.
