# Tasks: Unified Redis Key Search Cursor

**Input**: Design documents from `F:/Source/Repos/Roman.RedisManager/specs/001-cursor-token-unification/`
**Prerequisites**: `plan.md` and `spec.md`; supporting docs: `research.md`, `data-model.md`, `contracts/redis-keys-search-contract.md`, `quickstart.md`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared token infrastructure and documentation surface for the feature.

- [X] T001 Create feature task tracking checklist in `specs/001-cursor-token-unification/checklists/implementation-checklist.md`
- [X] T002 Add token options section (`TokenSecret`, optional TTL) in `src/Roman.RedisManager.Web/appsettings.json`
- [X] T003 [P] Add matching token options defaults in `src/Roman.RedisManager.Web/appsettings.Development.json`
- [X] T004 Register and validate token options binding in `src/Roman.RedisManager.Web/Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build core continuation token primitives and domain contracts used by all stories.

**CRITICAL**: Complete this phase before starting user story implementation.

- [X] T005 Create continuation token error codes model in `src/Roman.RedisManager.Domain/Entities/ContinuationTokenError.cs`
- [X] T006 Create versioned continuation envelope model in `src/Roman.RedisManager.Domain/Entities/ContinuationTokenEnvelope.cs`
- [X] T007 [P] Create standalone cursor state model in `src/Roman.RedisManager.Domain/Entities/StandaloneCursorState.cs`
- [X] T008 [P] Create cluster cursor state model in `src/Roman.RedisManager.Domain/Entities/ClusterCursorState.cs`
- [X] T009 Add continuation token codec abstraction in `src/Roman.RedisManager.Domain/Repositories/IContinuationTokenCodec.cs`
- [X] T010 Implement HMAC-protected Base64URL token codec in `src/Roman.RedisManager.Infrastructure/Redis/ContinuationTokenCodec.cs`
- [X] T011 Add token codec unit tests (encode/decode/signature/version) in `tests/Roman.RedisManager.Tests/Infrastructure/Redis/ContinuationTokenCodecTests.cs`
- [X] T012 Wire token codec DI registration in `src/Roman.RedisManager.Web/Program.cs`

**Checkpoint**: Token infrastructure and contracts are ready for all user stories.

---

## Phase 3: User Story 1 - Continue searches with one cursor contract (Priority: P1) MVP

**Goal**: Deliver one topology-agnostic continuation flow (`continuationToken`) for standalone and cluster key searches.

**Independent Test**: Start search and continue for both standalone and cluster groups using the same request/response fields without any node-specific branching.

### Tests for User Story 1

- [X] T013 [P] [US1] Update handler mapping tests for unified continuation token result in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
- [X] T014 [P] [US1] Add repository continuation flow tests for standalone and cluster token replay in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [X] T036 [P] [US1] Add standalone continuation integration test proving 3 consecutive pages with continuationToken replay in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [X] T037 [P] [US1] Add cluster continuation integration test proving 3 consecutive pages with continuationToken replay in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`

### Implementation for User Story 1

- [X] T015 [US1] Replace query input/output cursor contract with `continuationToken` in `src/Roman.RedisManager.Application/CQRS/RedisKeysSearchQueryHandler.cs`
- [X] T016 [US1] Refactor search result entity to remove outward node cursor exposure in `src/Roman.RedisManager.Domain/Entities/RedisSearchResult.cs`
- [X] T017 [US1] Update repository interface to accept/return unified continuation state in `src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs`
- [X] T018 [US1] Implement standalone + cluster internal state translation to unified token in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [X] T039 [US1] Add repository test assertions that continuation requests remain bounded to existing SCAN flow expectations in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [X] T019 [US1] Update HTTP query parameter and response contract to `continuationToken` in `src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`
- [X] T020 [US1] Refresh API manual examples for unified continuation flow in `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

**Checkpoint**: User Story 1 supports multi-page continuation for both topologies with the same opaque token contract.

---

## Phase 4: User Story 2 - Fail safely on invalid continuation state (Priority: P2)

**Goal**: Reject malformed, mismatched, and non-resumable tokens with explicit client-visible 400 errors.

**Independent Test**: Submit malformed token, context-mismatch token, and non-resumable token; each must return deterministic 400 with documented error code.

### Tests for User Story 2

- [X] T021 [P] [US2] Add token validation failure coverage in repository tests in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- [X] T022 [P] [US2] Add controller bad-request problem-details tests for continuation failures in `tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsBadRequestTests.cs`

### Implementation for User Story 2

- [X] T023 [US2] Add typed continuation token validation exceptions in `src/Roman.RedisManager.Infrastructure/Exceptions/InvalidContinuationTokenException.cs`
- [X] T024 [US2] Enforce request context binding validation during token decode/resume in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [X] T025 [US2] Route continuation token validation exceptions through centralized error handling and emit stable 400 ProblemDetails in `src/Roman.RedisManager.Web/ProblemDetails/` and `src/Roman.RedisManager.Web/Program.cs`
- [X] T026 [US2] Update endpoint contract error examples for invalid/mismatch/not-resumable cases in `specs/001-cursor-token-unification/contracts/redis-keys-search-contract.md`
- [X] T038 [US2] Add structured logging for continuation token rejection codes in `src/Roman.RedisManager.Web/ProblemDetails/`

**Checkpoint**: Invalid continuation inputs fail clearly and never silently restart scans.

---

## Phase 5: User Story 3 - End searches predictably (Priority: P3)

**Goal**: Ensure completion semantics are explicit (`hasMoreResults=false` and no next token) for both empty and terminal pages.

**Independent Test**: Page until completion and verify final response semantics; run empty-pattern/no-match scenario and confirm no follow-up token is required.

### Tests for User Story 3

- [X] T027 [P] [US3] Add completion semantics tests (final page and empty result) in `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
- [X] T028 [P] [US3] Add repository tests validating token omission on completion in `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`

### Implementation for User Story 3

- [X] T029 [US3] Ensure query result emits nullable continuation token only when more results exist in `src/Roman.RedisManager.Application/CQRS/RedisKeysSearchQueryHandler.cs`
- [X] T030 [US3] Align repository completion behavior to clear terminal state for both topologies in `src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs`
- [X] T031 [US3] Update quickstart completion verification steps in `specs/001-cursor-token-unification/quickstart.md`

**Checkpoint**: Completion is explicit and consistent, with no topology-specific interpretation required.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency, documentation, and validation across all stories.

- [X] T032 [P] Add migration notes for breaking cursor-to-continuationToken changes in `specs/001-cursor-token-unification/research.md`
- [X] T033 [P] Add/update API response schema examples in `src/Roman.RedisManager.Web/Roman.RedisManager.Web.json`
- [X] T034 Run quickstart and explicitly record SC-001, SC-002, SC-003, and SC-004 pass/fail evidence in `specs/001-cursor-token-unification/checklists/validation-results.md`
- [X] T035 Run regression test suite for affected components via `tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): starts immediately.
- Foundational (Phase 2): depends on Setup completion; blocks all user stories.
- User Stories (Phases 3-5): depend on Foundational completion.
- Polish (Phase 6): depends on completion of selected user stories.

### User Story Dependencies

- US1 (P1): no dependency on other stories after Phase 2; defines MVP.
- US2 (P2): depends on US1 contract being in place.
- US3 (P3): depends on US1 continuation contract and can run after US2 or in parallel once error mapping surface is stable.

### Within Each User Story

- Test tasks should be implemented first and fail before implementation changes.
- Contract/model updates precede repository/controller changes.
- Repository behavior precedes handler/controller response shaping.
- Documentation updates follow implementation validation.

---

## Parallel Opportunities

- Setup: `T002` and `T003` can be executed in parallel.
- Foundational: `T007` and `T008` can run in parallel; `T011` can start once `T010` exists.
- US1: `T013` and `T014` can run in parallel.
- US1: `T036` and `T037` can run in parallel.
- US2: `T021` and `T022` can run in parallel.
- US3: `T027` and `T028` can run in parallel.
- Polish: `T032` and `T033` can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Run in parallel after Phase 2 completes:
Task T013 - Update handler mapping tests in tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs
Task T014 - Add repository continuation tests in tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs
Task T036 - Add standalone 3-page continuation integration test in tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs
Task T037 - Add cluster 3-page continuation integration test in tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs
```

## Parallel Example: User Story 2

```bash
# Run in parallel after US1 checkpoint:
Task T021 - Add repository validation failure tests in tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs
Task T022 - Add bad-request problem-details tests in tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsBadRequestTests.cs
```

## Parallel Example: User Story 3

```bash
# Run in parallel after US1 contract is stable:
Task T027 - Add completion semantics handler tests in tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs
Task T028 - Add terminal token omission repository tests in tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Finish Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate independent test for US1 on standalone and cluster groups.
4. Demo/deploy MVP with unified continuation contract.

### Incremental Delivery

1. Complete US1 for unified continuation behavior.
2. Add US2 strict error handling for invalid token cases.
3. Add US3 completion semantics hardening.
4. Execute Phase 6 polish and regression checks.

### Suggested MVP Scope

- Include tasks `T001` through `T020` plus `T036`, `T037`, and `T039` (Setup + Foundational + complete US1 validation).
- Defer `T021` onward for post-MVP hardening and documentation completion.

---

## Format Validation

- All tasks follow required checklist format: `- [ ] T### [P?] [US?] Description with file path`.
- Setup, Foundational, and Polish tasks intentionally omit story labels.
- User story tasks include story labels (`[US1]`, `[US2]`, `[US3]`).
- Parallelizable tasks are explicitly marked with `[P]`.
