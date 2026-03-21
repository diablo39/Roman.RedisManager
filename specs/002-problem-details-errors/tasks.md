# Tasks: RFC 9457 Error Responses

**Input**: Design documents from `/specs/001-problem-details-errors/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/problem-details-schema.md, quickstart.md

**Tests**: Test tasks are included because the feature requires verifiable error-contract behavior and updated test assets (FR-010).

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on unfinished tasks)
- **[Story]**: User story label (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare folders and artifacts used by all stories.

- [X] T001 [P] Create ProblemDetails folder structure in `backend/src/Roman.RedisManager.Web/ProblemDetails/` and `backend/tests/Roman.RedisManager.Tests/Web/ProblemDetails/`
- [X] T002 [P] Create story-specific HTTP test sections in `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core error-handling plumbing required before user-story behavior can be implemented.

**⚠️ CRITICAL**: No user story work starts before this phase is complete.

- [X] T003 Register Problem Details services and middleware pipeline (`AddProblemDetails`, `UseExceptionHandler`, `UseStatusCodePages`) in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T004 Add centralized error endpoint/controller for exception handler route in `backend/src/Roman.RedisManager.Web/Controllers/ErrorController.cs`
- [X] T005 [P] Add shared ProblemDetails response assertion helper in `backend/tests/Roman.RedisManager.Tests/Web/ProblemDetails/ProblemDetailsAssertions.cs`

**Checkpoint**: Application-wide ProblemDetails foundation is ready.

---

## Phase 3: User Story 1 - Receive Standardized Error Payloads (Priority: P1) 🎯 MVP

**Goal**: Return RFC 9457-compliant bodies for representative `400` and `500` responses.

**Independent Test**: Trigger one `400` and one `500`, then verify both include RFC 9457 fields and no sensitive internal details.

### Tests for User Story 1

- [X] T006 [P] [US1] Add integration test for `400` ProblemDetails response shape in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsBadRequestTests.cs`
- [X] T007 [P] [US1] Add integration test for `500` ProblemDetails response shape in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsServerErrorTests.cs`

### Implementation for User Story 1

- [X] T008 [US1] Implement generic `Problem()` response action for unhandled exceptions in `backend/src/Roman.RedisManager.Web/Controllers/ErrorController.cs`
- [X] T009 [US1] Wire exception-handler route and status-code pages behavior in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T010 [US1] Add manual test requests for representative `400` and `500` responses in `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`
- [X] T010a [US1] Add manual test request for unsupported `Accept` header scenario in the same `.http` file

**Checkpoint**: User Story 1 is complete and independently testable (MVP).

---

## Phase 4: User Story 2 - Handle Routing and Validation Failures Uniformly (Priority: P2)

**Goal**: Ensure validation failures and unknown routes return standardized problem details.

**Independent Test**: Send invalid input to a controller action and request a missing route; both must return predictable problem details payloads.

### Tests for User Story 2

- [X] T011a [P] [US2] Add integration test verifying `401` and `403` responses use ProblemDetails format

- [X] T011 [P] [US2] Add integration test for `ValidationProblemDetails` contract on invalid input in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsValidationTests.cs`
- [X] T012 [P] [US2] Add integration test for `404` routing miss ProblemDetails payload in `backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsNotFoundTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Add/adjust request validation attributes for key search input in `backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs`
- [X] T014 [US2] Configure model-validation response behavior (if needed) to preserve predictable `ValidationProblemDetails` payloads in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T015 [US2] Add manual test requests for invalid input and unknown route in `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`

**Checkpoint**: User Story 2 is complete and independently testable.

---

## Phase 5: User Story 3 - Support Faster Diagnosis (Priority: P3)

**Goal**: Include support-oriented correlation metadata with `traceId` as primary key.

**Independent Test**: Trigger an error and verify response includes required `traceId` and `requestPath`, with optional `traceparent` when present.

### Tests for User Story 3

- [X] T016 [P] [US3] Add unit/integration tests for trace metadata enrichment in `backend/tests/Roman.RedisManager.Tests/Web/ProblemDetails/TraceCorrelationProblemDetailsTests.cs`

### Implementation for User Story 3

- [X] T017 [P] [US3] Implement ProblemDetails enrichment component that sets `traceId` (required), optional `traceparent`, and `requestPath` in `backend/src/Roman.RedisManager.Web/ProblemDetails/TraceCorrelationProblemDetailsEnricher.cs`
- [X] T018 [US3] Register and apply the trace enrichment component in `backend/src/Roman.RedisManager.Web/Program.cs`
- [X] T005 [P] [Foundation] Finalize and publish contract schema (`problem-details-schema.md`) before executing story tests (moved from US3)
- [X] T019 [US3] Align response contract with required `traceId` and optional `traceparent` in `specs/001-problem-details-errors/contracts/problem-details-schema.md`

**Checkpoint**: User Story 3 is complete and independently testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency checks and delivery readiness.

- [X] T020 [P] Update implementation guidance examples to match final behavior in `specs/001-problem-details-errors/quickstart.md`
- [X] T021 [P] Update completion details and validation notes in `specs/001-problem-details-errors/plan.md`
- [X] T022 Record final checklist validation after implementation in `specs/001-problem-details-errors/checklists/requirements.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately
- **Phase 2 (Foundational)**: depends on Phase 1, blocks all stories
- **Phase 3 (US1)**: depends on Phase 2
- **Phase 4 (US2)**: depends on Phase 2 (can run after/alongside US1)
- **Phase 5 (US3)**: depends on Phase 2 (recommended after US1 to reuse validated error pipeline)
- **Phase 6 (Polish)**: depends on selected user stories being complete

### User Story Dependencies

- **US1 (P1)**: no dependency on other stories after foundation
- **US2 (P2)**: no hard dependency on US1, integrates cleanly with shared error pipeline
- **US3 (P3)**: depends on shared error pipeline and benefits from US1 being validated first

### Within Each User Story

- Tests first, then implementation
- Middleware/factory wiring before `.http` examples
- Story-specific checkpoint before moving to next priority

---

## Parallel Opportunities

- **Setup**: T001 and T002 can run in parallel
- **Foundational**: T005 can run in parallel with T003/T004
- **US1**: T006 and T007 can run in parallel
- **US2**: T011 and T012 can run in parallel
- **US3**: T016 and T017 can run in parallel
- **Polish**: T020 and T021 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Parallel test authoring
T006 backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsBadRequestTests.cs
T007 backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsServerErrorTests.cs

# Then sequential implementation
T008 backend/src/Roman.RedisManager.Web/Controllers/ErrorController.cs
T009 backend/src/Roman.RedisManager.Web/Program.cs
T010 backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http
```

## Parallel Example: User Story 2

```bash
# Parallel tests
T011 backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsValidationTests.cs
T012 backend/tests/Roman.RedisManager.Tests/Web/Controllers/ProblemDetailsNotFoundTests.cs

# Then implementation
T013 backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs
T014 backend/src/Roman.RedisManager.Web/Program.cs
T015 backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http
```

## Parallel Example: User Story 3

```bash
# Parallel tasks
T016 backend/tests/Roman.RedisManager.Tests/Web/ProblemDetails/TraceCorrelationProblemDetailsTests.cs
T017 backend/src/Roman.RedisManager.Web/ProblemDetails/TraceCorrelationProblemDetailsEnricher.cs

# Then wiring and contract alignment
T018 backend/src/Roman.RedisManager.Web/Program.cs
T019 specs/001-problem-details-errors/contracts/problem-details-schema.md
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2
2. Complete US1 (Phase 3)
3. Validate US1 independently with tests and `.http` requests
4. Demo/ship MVP

### Incremental Delivery

1. Foundation complete
2. Deliver US1 (standardized `400`/`500`)
3. Deliver US2 (validation + `404` uniformity)
4. Deliver US3 (`traceId` primary correlation metadata)
5. Finish polish tasks

### Parallel Team Strategy

1. Team completes Phase 1–2 together
2. Split by story:
   - Dev A: US1
   - Dev B: US2
   - Dev C: US3
3. Rejoin for Phase 6 polish and final validation

---

## Notes

- `[P]` tasks touch different files and can be parallelized safely.
- User-story labels map each task directly to business value.
- Keep changes minimal and consistent with existing project conventions.
- Use `traceId` as the primary support lookup key for SC-003.
