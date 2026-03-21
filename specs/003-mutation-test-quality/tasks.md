# Tasks: Mutation Testing Quality Improvements

**Input**: Design documents from `specs/001-mutation-test-quality/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/mutation-quality-contract.md`, `quickstart.md`

**Tests**: Test tasks are included because the feature explicitly requires validating and iteratively improving test quality.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish mutation tooling, base config, and artifact handling.

- [x] T001 Create local .NET tool manifest in `backend/.config/dotnet-tools.json`
- [x] T002 Install and pin `dotnet-stryker` tool in `backend/.config/dotnet-tools.json`
- [x] T003 [P] Add mutation artifact ignore rules in `backend/.gitignore`
- [x] T004 [P] Add baseline Stryker configuration in `backend/tests/Roman.RedisManager.Tests/stryker-config.json`
- [x] T005 [P] Create mutation output placeholder directory in `backend/tests/Roman.RedisManager.Tests/StrykerOutput/.gitkeep`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create reusable mutation workflow scripts and shared tracking artifacts required by all stories.

**CRITICAL**: No user story work starts until this phase is complete.

- [x] T006 Create mutation run orchestration script in `backend/.specify/scripts/powershell/run-mutation-tests.ps1`
- [x] T007 [P] Create baseline comparison script in `backend/.specify/scripts/powershell/compare-mutation-reports.ps1`
- [x] T008 [P] Create weak/duplicate finding extraction script in `backend/.specify/scripts/powershell/analyze-surviving-mutants.ps1`
- [x] T009 [P] Create assertion improvement cycle template in `specs/001-mutation-test-quality/checklists/assertion-improvement-template.md`
- [x] T010 Wire reusable mutation commands into VS Code tasks in `backend/.vscode/tasks.json`
- [x] T011 Document script-driven workflow entrypoints in `specs/001-mutation-test-quality/quickstart.md`
- [x] T012 [P] Add root/backend `.specify` path governance note in `specs/001-mutation-test-quality/quickstart.md`

**Checkpoint**: Foundation ready. User stories can start.

---

## Phase 3: User Story 1 - Run Mutation Quality Analysis (Priority: P1) 🎯 MVP

**Goal**: Produce HTML, JSON, and previous-run comparison artifacts for mutation runs.

**Independent Test**: Run mutation workflow once and verify report set generation plus baseline comparison behavior.

### Tests for User Story 1

- [x] T013 [P] [US1] Add mutation CLI argument composition tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationCliArgumentsTests.cs`
- [x] T014 [P] [US1] Add mutation report set validation tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationReportSetValidatorTests.cs`

### Implementation for User Story 1

- [x] T015 [US1] Create mutation run settings model in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationRunSettings.cs`
- [x] T016 [US1] Implement mutation CLI argument builder in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationCliArguments.cs`
- [x] T017 [US1] Implement report set validator in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationReportSetValidator.cs`
- [x] T018 [US1] Integrate HTML and JSON reporter enforcement in `backend/.specify/scripts/powershell/run-mutation-tests.ps1`
- [x] T019 [US1] Integrate previous-run baseline comparison handling in `backend/.specify/scripts/powershell/compare-mutation-reports.ps1`
- [x] T020 [US1] Record independent validation evidence in `specs/001-mutation-test-quality/checklists/us1-independent-test.md`

**Checkpoint**: US1 delivers mutation report generation and comparison behavior.

---

## Phase 4: User Story 2 - Detect Weak Or Duplicate Tests (Priority: P2)

**Goal**: Detect weak assertions and potential duplicate tests from mutation outcomes.

**Independent Test**: Analyze mutation JSON output and verify actionable weak/duplicate findings are produced.

### Tests for User Story 2

- [x] T021 [P] [US2] Add surviving mutant classification tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationFindingClassifierTests.cs`
- [x] T022 [P] [US2] Add duplicate test heuristic tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/DuplicateTestHeuristicAnalyzerTests.cs`

### Implementation for User Story 2

- [x] T023 [US2] Create mutation finding model in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationFinding.cs`
- [x] T024 [US2] Implement surviving mutant classifier in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/MutationFindingClassifier.cs`
- [x] T025 [US2] Implement duplicate test heuristic analyzer in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/DuplicateTestHeuristicAnalyzer.cs`
- [x] T026 [US2] Wire finding extraction and serialization in `backend/.specify/scripts/powershell/analyze-surviving-mutants.ps1`
- [x] T027 [US2] Publish weak/duplicate findings report in `specs/001-mutation-test-quality/checklists/mutation-findings.md`
- [x] T028 [US2] Record independent validation evidence in `specs/001-mutation-test-quality/checklists/us2-independent-test.md`
- [x] T029 [US2] Codify duplicate/low-differentiation heuristics in `specs/001-mutation-test-quality/contracts/mutation-quality-contract.md`

**Checkpoint**: US2 provides actionable weak/duplicate-test detection.

---

## Phase 5: User Story 3 - Enforce Iterative Assertion Hardening For New Tests (Priority: P3)

**Goal**: Ensure future AI-generated tests run mutation checks and iterate on assertion quality until completion gates are met.

**Independent Test**: Create or modify tests via AI workflow and verify mandatory run -> analyze -> improve -> rerun loop is enforced by governance artifacts.

### Tests for User Story 3

- [x] T030 [P] [US3] Add governance policy regression tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/GovernanceMutationPolicyTests.cs`
- [x] T031 [P] [US3] Add assertion improvement cycle transition tests in `backend/tests/Roman.RedisManager.Tests/Infrastructure/Quality/AssertionImprovementCycleTests.cs`

### Implementation for User Story 3

- [x] T032 [US3] Update AI mutation quality policy in `backend/.github/copilot-instructions.md`
- [x] T033 [US3] Update constitutional mutation quality gate in `backend/.specify/memory/constitution.md`
- [x] T034 [US3] Add mutation-loop guardrails to spec template in `backend/.specify/templates/spec-template.md`
- [x] T035 [US3] Add mutation-loop guardrails to plan template in `backend/.specify/templates/plan-template.md`
- [x] T036 [US3] Add mutation-loop guardrails to tasks template in `backend/.specify/templates/tasks-template.md`
- [x] T037 [US3] Add mutation-loop enforcement in `backend/.github/prompts/speckit.implement.prompt.md`
- [x] T038 [US3] Add mutation quality gating to `backend/.github/prompts/speckit.tasks.prompt.md`
- [x] T039 [US3] Add root template durability guardrails in `.specify/templates/agent-file-template.md`
- [x] T040 [US3] Create non-actionable mutant exception log template in `specs/001-mutation-test-quality/checklists/non-actionable-mutant-exceptions.md`
- [x] T041 [US3] Add governance durability validation script in `backend/.specify/scripts/powershell/validate-mutation-governance-durability.ps1`
- [x] T042 [US3] Capture iterative run-improve-rerun cycle evidence in `specs/001-mutation-test-quality/checklists/us3-iterative-cycle.md`
- [x] T043 [US3] Capture agent context update and durability evidence in `specs/001-mutation-test-quality/checklists/us3-agent-context-update.md`

**Checkpoint**: US3 enforces durable mutation quality governance for future AI-authored tests.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across stories and release-ready documentation.

- [x] T044 [P] Run full project test suite and capture output in `specs/001-mutation-test-quality/checklists/final-dotnet-test.md`
- [x] T045 [P] Run baseline and comparison mutation passes and capture deltas in `specs/001-mutation-test-quality/checklists/final-mutation-summary.md`
- [x] T046 Validate quickstart commands against final script names and record sign-off in `specs/001-mutation-test-quality/checklists/quickstart-validation.md`
- [x] T047 [P] Summarize delivered scope and residual risks in `specs/001-mutation-test-quality/checklists/final-release-notes.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) starts immediately.
- Foundational (Phase 2) depends on Setup and blocks all user stories.
- User Stories (Phases 3-5) depend on Foundational completion.
- Polish (Phase 6) depends on all selected user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2; no dependency on US2/US3.
- **US2 (P2)**: Starts after Phase 2 and uses US1 report outputs but remains independently testable with sample mutation JSON.
- **US3 (P3)**: Starts after Phase 2 and references US1/US2 artifacts for governance examples, includes root/backend durability validation, but can be validated independently through policy checks.

### Suggested Completion Order

- T001-T012 -> T013-T020 (MVP) -> T021-T029 -> T030-T043 -> T044-T047

---

## Parallel Execution Examples

### User Story 1

- Run T013 and T014 in parallel because they touch different test files.
- Run T015, T016, and T017 in parallel before integrating scripts in T018.

### User Story 2

- Run T021 and T022 in parallel.
- Run T023 and T025 in parallel, then complete T024 and T026.

### User Story 3

- Run T030 and T031 in parallel.
- Run T034, T035, T036, T037, T038, and T039 in parallel after T032 and T033 start governance baseline updates.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Complete US1 tasks T013-T020.
3. Validate independent test evidence in `specs/001-mutation-test-quality/checklists/us1-independent-test.md`.
4. Demonstrate report generation and baseline comparison before expanding scope.

### Incremental Delivery

1. Deliver MVP (US1) to establish mutation reporting.
2. Deliver US2 to produce actionable weak/duplicate findings.
3. Deliver US3 to lock in durable AI governance and iterative quality loops.
4. Finish with Phase 6 cross-cutting validation.

### Parallel Team Strategy

1. One developer handles scripts (T006-T008, T018-T019, T026, T041).
2. One developer handles analyzer/test utility implementation (T013-T017, T021-T025, T030-T031).
3. One developer handles governance artifacts (T032-T043).
4. Merge streams at Phase 6 validation tasks.
