# Tasks: Redis Search Metadata Expansion

**Input**: Design documents from `F:/Source/Repos/Roman.RedisManager/specs/001-redis-search-attributes/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/redis-keys-search-response.md`, `quickstart.md`

**Tests**: Tests are required for this feature because the spec includes mandatory testing and mutation-quality requirements.

**Organization**: Tasks are grouped by user story so each story can be implemented and verified independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare configuration surfaces and manual verification assets used by all stories.

- [x] T001 Add `Redis:Search:MaxPageSize` defaults in `src/Roman.RedisManager.Web/appsettings.json`
- [x] T002 [P] Add `Redis:Search:MaxPageSize` development override in `src/Roman.RedisManager.Web/appsettings.Development.json`
- [x] T003 [P] Add enriched search request examples with oversized `pageSize` in `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement shared domain/config plumbing required before user stories.

**CRITICAL**: No user story work starts until this phase is complete.

- [x] T004 Create validated search limit options model in `src/Roman.RedisManager.Domain/Configuration/RedisSearchLimitsConfiguration.cs`
- [x] T005 Extend `src/Roman.RedisManager.Domain/Configuration/RedisConfiguration.cs` with required `Search` configuration binding
- [x] T006 Bind and validate search limit options in `src/Roman.RedisManager.Web/Program.cs`
- [x] T007 Add domain tests for new search limit configuration validation in `tests/Roman.RedisManager.Tests/Domain/Configuration/RedisSearchLimitsConfigurationTests.cs`

**Checkpoint**: Shared configuration and validation for effective page-size capping are in place.

---

## Phase 3: User Story 1 - See Type and TTL in Search Results (Priority: P1) 🎯 MVP

**Goal**: Return `type` and `ttlMilliseconds` for every key in `GET /api/redis-keys`.

**Independent Test**: Search keys containing both persistent and expiring keys; verify each key includes `type` and `ttlMilliseconds`, with null TTL for persistent keys.

### Tests for User Story 1

- [x] T008 [P] [US1] Add handler mapping tests for `type` and `ttlMilliseconds` in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
- [x] T009 [P] [US1] Add repository integration tests for metadata enrichment (`KeyTypeAsync`, `KeyTimeToLiveAsync`) in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [x] T010 [US1] Add controller/API response shape tests for enriched key items in `tests/Roman.RedisManager.Tests/Web/Controllers/RedisKeysControllerSearchTests.cs`

### Implementation for User Story 1

- [x] T011 [US1] Extend key entity metadata fields (`Type`, `Ttl`) in `src/Roman.RedisManager.Domain/Entities/RedisKey.cs`
- [x] T012 [US1] Enrich repository key collection with pipelined type/TTL retrieval in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [x] T013 [US1] Expand search DTO contract (`type`, `ttlMilliseconds`) and mappings in `src/Roman.RedisManager.Application/CQRS/RedisKeysSearchQueryHandler.cs`
- [x] T014 [US1] Ensure search action returns updated DTO contract in `src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`

**Checkpoint**: User Story 1 is independently functional and returns key type + TTL metadata.

---

## Phase 4: User Story 2 - Get Additional Useful Key Attributes (Priority: P2)

**Goal**: Add `hasExpiration` to each key item to reduce client-side interpretation.

**Independent Test**: Search across mixed persistent and expiring keys; verify each item contains `hasExpiration` with correct true/false values.

### Tests for User Story 2

- [x] T015 [P] [US2] Add handler mapping tests for `hasExpiration` derivation in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
- [x] T016 [P] [US2] Add repository tests for key-disappearance/volatile metadata tolerance in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [x] T017 [US2] Add API contract tests for consistent per-key metadata field set in `tests/Roman.RedisManager.Tests/Web/Controllers/RedisKeysControllerSearchTests.cs`

### Implementation for User Story 2

- [x] T018 [US2] Extend key entity with derived expiration indicator (`HasExpiration`) in `src/Roman.RedisManager.Domain/Entities/RedisKey.cs`
- [x] T019 [US2] Implement stable metadata fallback mapping for volatile keys in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [x] T020 [US2] Add `hasExpiration` field projection in `src/Roman.RedisManager.Application/CQRS/RedisKeysSearchQueryHandler.cs`
- [x] T021 [US2] Align API response contract documentation for `hasExpiration` in `specs/001-redis-search-attributes/contracts/redis-keys-search-response.md`

**Checkpoint**: User Story 2 is independently functional and adds derived expiration usability metadata.

---

## Phase 5: User Story 3 - Keep Search Calls Efficient at Scale (Priority: P3)

**Goal**: Enforce configured page-size cap and keep metadata enrichment bounded for predictable backend load.

**Independent Test**: Request `pageSize` above configured max and verify effective capping, continuation correctness, and enriched metadata still present.

### Tests for User Story 3

- [x] T022 [P] [US3] Add handler tests for `pageSize` normalization/capping behavior in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
- [x] T023 [P] [US3] Add repository tests confirming bounded key-page size and continuation correctness under cap in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [x] T024 [US3] Add API tests for oversized `pageSize` requests returning capped result sets in `tests/Roman.RedisManager.Tests/Web/Controllers/RedisKeysControllerSearchTests.cs`

### Implementation for User Story 3

- [x] T025 [US3] Add effective page-size capping logic using `RedisSearchLimitsConfiguration` in `src/Roman.RedisManager.Application/CQRS/RedisKeysSearchQueryHandler.cs`
- [x] T026 [US3] Ensure repository search uses bounded page size for scan and metadata enrichment in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [x] T027 [US3] Preserve continuation-token context/hash behavior with effective page size in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`

**Checkpoint**: User Story 3 is independently functional with bounded search request volume and preserved pagination semantics.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end verification, mutation quality gate evidence, and final documentation sync.

- [x] T028 Run `dotnet build -warnaserror` and capture zero-warning evidence for the constitution build gate
- [x] T029 Run full unit/integration suite for feature scope in `tests/Roman.RedisManager.Tests`
- [x] T030 Run mutation tests and capture HTML/JSON reports via `.specify/scripts/powershell/run-mutation-tests.ps1`
- [x] T031 Analyze surviving/no-coverage mutants via `.specify/scripts/powershell/analyze-surviving-mutants.ps1`
- [x] T032 Improve actionable assertions in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs` and `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [x] T033 Re-run mutation tests and capture deltas/exceptions via `.specify/scripts/powershell/run-mutation-tests.ps1` and `.specify/scripts/powershell/compare-mutation-reports.ps1`
- [x] T034 Execute scripted performance validation for SC-003 and record latency evidence in `specs/001-redis-search-attributes/checklists/performance-validation.md`
- [x] T035 Execute structured usability validation for SC-004 and record outcomes in `specs/001-redis-search-attributes/checklists/usability-validation.md`
- [x] T036 Validate quickstart scenarios and update execution notes in `specs/001-redis-search-attributes/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) has no dependencies.
- Foundational (Phase 2) depends on Setup and blocks all user stories.
- User Stories (Phases 3-5) depend on Foundational completion.
- Polish (Phase 6) depends on completion of all implemented user stories.

### User Story Dependencies

- US1 (P1) starts immediately after Foundational and defines core metadata fields.
- US2 (P2) depends on US1 metadata fields (`ttlMilliseconds`) to derive and expose `hasExpiration`.
- US3 (P3) depends on Foundational configuration and should run after US1 because it verifies enriched-result behavior under capped requests.

### Dependency Graph

- Phase 1 -> Phase 2 -> US1 -> US2 -> US3 -> Phase 6

---

## Parallel Execution Examples

## Parallel Example: User Story 1

```bash
# Parallel test authoring
T008, T009

# Then parallel-safe implementation split by layer/file boundaries
T012, T013
```

## Parallel Example: User Story 2

```bash
# Parallel tests in separate files
T015, T016

# Parallel implementation with low conflict risk
T019, T020
```

## Parallel Example: User Story 3

```bash
# Parallel tests for handler and repository behavior
T022, T023

# Complete app-layer cap then repository continuation update
T025 -> T026 and T027
```

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate US1 independent test criteria before expanding scope.

### Incremental Delivery

1. Build shared foundation (Phases 1-2).
2. Deliver US1 (core metadata contract).
3. Deliver US2 (derived usability field).
4. Deliver US3 (request capping + bounded load).
5. Execute mutation loop and finalize with Phase 6.

### Quality Gates

- Every story must pass its independent test criteria before moving to next priority.
- Mutation loop is mandatory: run -> analyze -> improve -> rerun with HTML/JSON artifacts and delta tracking.
- Do not close the feature while actionable surviving mutants remain without documented approved exceptions.

---

## Notes

- Task format is strict: `- [x] T### [P] [US#] Description with file path`.
- `[P]` is used only where file-level parallelism is realistic.
- Story labels are applied only to user-story phase tasks.
- All paths are repository-relative to `F:/Source/Repos/Roman.RedisManager/backend` unless under `specs/`.
