# Tasks: OIDC Authentication

**Input**: Design documents from /specs/008-oidc-authentication/
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/authentication-bootstrap.md, quickstart.md

**Tests**: Test tasks are REQUIRED. Every user story includes automated unit/component/integration tests.

**Organization**: Tasks are grouped by user story so each story is independently implementable and testable.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install dependencies and establish test harness required for feature delivery.

- [x] T001 Install OIDC runtime dependency in frontend/package.json (oidc-client-ts)
- [x] T002 Install and configure test tooling in frontend/package.json (vitest, @vue/test-utils, jsdom)
- [x] T003 [P] Add Vitest configuration for Vue component and router tests in frontend/vite.config.mts
- [x] T004 [P] Add test setup bootstrap for jsdom/polyfills in frontend/tests/setup.ts

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create shared authentication primitives that all user stories depend on.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [x] T005 Create authentication domain types for bootstrap/provider/session in frontend/src/api/authentication.ts
- [x] T006 [P] Implement OIDC manager factory using session storage in frontend/src/plugins/oidc.ts
- [x] T007 Create base authentication store state/actions in frontend/src/stores/authentication.ts
- [x] T008 [P] Add shared auth route constants/helpers in frontend/src/router/auth.ts
- [x] T009 Register any new auth plugin wiring in frontend/src/plugins/index.ts

**Checkpoint**: Foundation complete; user stories can proceed.

---

## Phase 3: User Story 1 - Sign In With Available Provider (Priority: P1) 🎯 MVP

**Goal**: Users can load login, see available providers from bootstrap endpoint, and start OIDC redirect.

**Independent Test**: Open login page, verify provider options from bootstrap data, select provider, and verify redirect initiation call.

### Tests for User Story 1 (REQUIRED) ✅

- [x] T010 [P] [US1] Add bootstrap API success/error parsing unit tests in frontend/tests/unit/authentication.api.spec.ts
- [x] T011 [P] [US1] Add authentication store redirect-initiation tests in frontend/tests/unit/authentication.store.spec.ts
- [x] T012 [P] [US1] Add login page provider-list rendering tests in frontend/tests/component/login.page.spec.ts

### Implementation for User Story 1

- [x] T013 [US1] Implement GET /api/authentication/bootstrap client in frontend/src/api/authentication.ts
- [x] T014 [US1] Extend provider bootstrap and redirect actions in frontend/src/stores/authentication.ts
- [x] T015 [US1] Create login screen with provider actions and retry handling in frontend/src/pages/login.vue
- [x] T016 [US1] Add login route metadata for public access in frontend/src/pages/login.vue
- [x] T017 [US1] Create OIDC callback processor page in frontend/src/pages/auth/callback.vue
- [x] T018 [US1] Implement callback success/error session finalization in frontend/src/stores/authentication.ts

**Checkpoint**: User Story 1 is fully functional and independently testable.

---

## Phase 4: User Story 2 - Access Protected Screens Only After Sign-In (Priority: P1)

**Goal**: All non-auth routes are protected; unauthenticated users are redirected before protected content appears.

**Independent Test**: Attempt protected-route navigation while logged out and verify redirect to login, then authenticate and verify protected navigation succeeds.

### Tests for User Story 2 (REQUIRED) ✅

- [x] T019 [P] [US2] Add unauthenticated navigation redirect integration tests in frontend/tests/integration/router-auth-guard.spec.ts
- [x] T020 [P] [US2] Add authenticated navigation pass-through tests in frontend/tests/integration/router-auth-guard.spec.ts
- [x] T021 [P] [US2] Add returnUrl restore behavior tests after callback in frontend/tests/integration/router-auth-guard.spec.ts
- [x] T036 [P] [US2] Add session-expiration redirect behavior tests in frontend/tests/integration/router-auth-guard.spec.ts

### Implementation for User Story 2

- [x] T022 [US2] Add global beforeEach auth guard with public-route whitelist in frontend/src/router/index.ts
- [x] T023 [US2] Implement returnUrl capture and restore helpers in frontend/src/stores/authentication.ts
- [x] T024 [US2] Mark callback route as public and prevent guarded redirect loop in frontend/src/pages/auth/callback.vue
- [x] T025 [US2] Ensure existing protected pages rely on guard-only access in frontend/src/pages/index.vue
- [x] T037 [US2] Implement session-expiration handling that invalidates state and redirects to login in frontend/src/stores/authentication.ts

**Checkpoint**: User Stories 1 and 2 work independently and together.

---

## Phase 5: User Story 3 - Clear Authentication State Feedback (Priority: P2)

**Goal**: Users see consistent loading/unavailable/error feedback through authentication flows.

**Independent Test**: Simulate loading and unavailable bootstrap responses and verify UI messaging, disabled controls, and accessibility labels.

### Tests for User Story 3 (REQUIRED) ✅

- [x] T026 [P] [US3] Add login loading/unavailable state component tests in frontend/tests/component/login.states.spec.ts
- [x] T027 [P] [US3] Add callback failure message rendering tests in frontend/tests/component/auth-callback.page.spec.ts

### Implementation for User Story 3

- [x] T028 [US3] Add reason-code to user-message mapper for unavailable states in frontend/src/stores/authentication.ts
- [x] T029 [US3] Add accessible loading/unavailable/error UI states in frontend/src/pages/login.vue
- [x] T030 [US3] Add callback error recovery actions (retry/back to login) in frontend/src/pages/auth/callback.vue

**Checkpoint**: All user stories are independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening across quality, UX consistency, and performance.

- [x] T031 [P] Document authentication flow and route-guard behavior in frontend/README.md
- [x] T032 Validate lint, type-check, and build quality gates for changed artifacts via frontend/package.json scripts
- [x] T033 Validate auth test suite execution from quickstart in specs/008-oidc-authentication/quickstart.md
- [x] T034 Validate sign-in render performance budget and no protected-content flash in frontend/src/router/index.ts
- [x] T035 [P] Validate keyboard navigation and screen-reader labels on auth pages in frontend/src/pages/login.vue
- [x] T038 [P] Validate SC-003 first-attempt start rate using a 20-attempt UAT checklist in specs/008-oidc-authentication/quickstart.md
- [x] T039 [P] Define and run SC-006 support-ticket measurement query (`auth-login-discovery`) for pre/post 30-day windows in specs/008-oidc-authentication/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): Starts immediately.
- Foundational (Phase 2): Depends on Setup and blocks all stories.
- User Story phases (Phase 3-5): Depend on Foundational completion.
- Polish (Phase 6): Depends on completion of selected user stories.

### User Story Dependencies

- US1 (P1): Starts after Phase 2; no dependency on other stories.
- US2 (P1): Starts after Phase 2; depends functionally on foundational auth state but is independently testable via guard behavior.
- US3 (P2): Starts after Phase 2; can run after US1 UI exists, with no blocking dependency on US2 completion.

### Within Each User Story

- Write tests first where feasible (expected to fail initially).
- Implement API/store logic before route/page wiring when behavior depends on state.
- Complete implementation before running story-specific integration checks.

### Parallel Opportunities

- Setup: T003 and T004 can run in parallel after T001-T002.
- Foundational: T006 and T008 can run in parallel after T005.
- US1: T010, T011, and T012 can run in parallel.
- US2 (extended): T019, T020, T021, and T036 can run in parallel.
- US3: T026 and T027 can run in parallel.
- Polish: T031, T035, T038, and T039 can run in parallel with T032-T034 validation.

---

## Parallel Example: User Story 1

- Run T010, T011, and T012 together (different test files under frontend/tests/).
- Run T015 and T017 in parallel after T014 (different page files with shared store contract already in place).

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate US1 independently via bootstrap + provider redirect tests.
4. Demo MVP sign-in entry flow.

### Incremental Delivery

1. Deliver US1 (provider login initiation).
2. Deliver US2 (full route protection).
3. Deliver US3 (state feedback hardening).
4. Execute polish gates and release.

### Parallel Team Strategy

1. Team finishes Setup + Foundational together.
2. Then split by story:
   - Developer A: US1 implementation/tests.
   - Developer B: US2 guard integration/tests.
   - Developer C: US3 UX state refinement/tests.
