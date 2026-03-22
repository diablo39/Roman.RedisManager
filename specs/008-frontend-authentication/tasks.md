# Tasks: Frontend Authentication

**Feature Branch**: `008-frontend-authentication`  
**Created**: 2026-03-22  
**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

---

## Phase 1 — Setup

Goal: install the OIDC client library and confirm the project builds cleanly.

### Tasks

- [x] T001 [infra] Install `oidc-client-ts` via `npm install oidc-client-ts` in `frontend/`

---

## Phase 2 — Foundational (blocks all user story phases)

Goal: create the shared infrastructure that every user story depends on — the API
module, the auth helper in config, the Pinia store, and the router constants.
These are pure TypeScript files with no UI; they can be verified by running
`npm run type-check` before any page is built.

**Independent Test**: Type-check and lint pass after this phase with zero errors.

### Tasks

- [x] T002 [P] Create `src/api/authentication.ts` — export `AuthenticationBootstrapProvider`, `AuthenticationBootstrapResult` types and `getAuthenticationBootstrap()` function that calls `GET /api/authentication/bootstrap` using `fetch` and the `apiBaseUrl` from `src/api/config.ts`
- [x] T003 [P] Create `src/router/auth.ts` — export `LOGIN_PATH = '/login'`, `CALLBACK_PATH = '/login/callback'`, `DEFAULT_AUTHENTICATED_PATH = '/'`, `isPublicPath(path: string): boolean` (returns true for login and callback paths), and `sanitizeReturnUrl(url: string | null | undefined): string` (returns `/` when url is null, external, matches `LOGIN_PATH` or `CALLBACK_PATH`, or is already the default path)
- [x] T004 Create `src/stores/authentication.ts` — Pinia Options API store (`useAuthenticationStore`) with state fields from data-model.md (including `callbackError: string | null`), getters `availableProviders`, `isSignInAvailable`, `unavailableMessage`, and actions: `loadBootstrap(force?)`, `startSignIn(providerKey, returnUrl?)` (encode `returnUrl` in OIDC `state` extra data so it survives the external redirect round-trip; in `handleCallback`, read it back from the callback state), `handleCallback()`, `ensureSessionValid()`, `setReturnUrl(url)`, `clearReturnUrl()`, `setActiveProviderKey(key)`, `setAuthenticated(flag)`, `invalidateSession()`; use `UserManager` / `WebStorageStateStore` from `oidc-client-ts` internally — depends on T002 and T003
- [x] T005 Modify `src/api/config.ts` — add `getAuthHeaders(): Promise<Record<string, string>>` that reads the current user from the active `UserManager` and returns `{ Authorization: 'Bearer <token>' }` when the user exists and has a non-expired access token, or an empty object otherwise — depends on T004

---

## Phase 3 — User Story 1: Start Sign-In With a Supported Provider (P1)

Goal: A signed-out user can open `/login`, see available providers (loaded from
the bootstrap endpoint), and click one to be redirected to the external identity
provider.

**Independent Test**: Open `/login` while signed out — providers render from the
bootstrap response; clicking a provider triggers a browser redirect to the
identity provider's authorization endpoint.

### Tasks

- [x] T006 [US1] [FR-001] Create `src/layouts/login.vue` — minimal layout (no app-bar, no nav drawer) that renders only `<v-layout><v-main><router-view /></v-main></v-layout>` (do NOT include `<v-app>` — it is already in App.vue)
- [x] T007 [US1] Create `src/pages/login.vue` — page using the `login` layout; on `onMounted` call `authStore.loadBootstrap()` and, if `route.query.returnUrl` is present, call `authStore.setReturnUrl(sanitizeReturnUrl(route.query.returnUrl))`; render: loading spinner while `bootstrapLoading` is true, `v-alert` with retry button when `error` is set, `v-alert` when `!isSignInAvailable` (show `unavailableMessage`), otherwise a `v-btn` for each provider in `availableProviders` that calls `authStore.startSignIn(provider.providerKey)` — depends on T004 and T006
- [ ] T008 [P] [US1] Verify US1 manually: open `/login`, confirm provider buttons appear when bootstrap returns available state, and click a button confirming the browser navigates to the external provider

---

## Phase 4 — User Story 2: Route Protection + Token Injection (P2)

Goal: All pages except `/login` and `/login/callback` require authentication.
Unauthenticated requests are redirected to `/login?returnUrl=<original>`. After
authentication, the user lands on the original destination. Redis API calls carry
the `Authorization` header.

**Independent Test**: Navigate to `/` while signed out — redirected to `/login`;
after sign-in land back on `/`. Network tab shows `Authorization: Bearer ...` on
Redis server group requests.

### Tasks

- [x] T009 [US2] Create `src/router/authGuard.ts` — export `applyAuthenticationGuard(router: Router)` that installs a `router.beforeEach` guard. Logic: (1) call `ensureSessionValid()` once and store the boolean result as `sessionValid`; (2) if (`isPublicPath(to.path)` or `to.meta.public === true`) **and** `to.path === LOGIN_PATH` **and** `sessionValid` → redirect to `authStore.returnUrl ?? DEFAULT_AUTHENTICATED_PATH`; (3) else if public path → allow through; (4) if `sessionValid` → allow through; (5) otherwise → call `authStore.setReturnUrl(sanitizeReturnUrl(to.fullPath))` then redirect to `LOGIN_PATH` with `query.returnUrl` — depends on T003 and T004
- [x] T010 [US2] Create `src/pages/login/callback.vue` — page using the `login` layout; on `onMounted` call `authStore.handleCallback()` and redirect to the returned destination, or on error set `authStore.callbackError` and stay on a simple error display — depends on T004 and T006
- [x] T011 [US2] Modify `src/router/index.ts` — call `applyAuthenticationGuard(router)` before `router.isReady()` — depends on T009
- [x] T012 [US2] Modify `src/api/redisServers.ts` — update both `getRedisServers()` and `getRedisServerGroupDetail()` to `await getAuthHeaders()` and spread the result into the `fetch` init `headers` option — depends on T005
- [ ] T013 [P] [US2] Verify US2 manually: sign out (clear localStorage), navigate to `/`, confirm redirect to `/login`, complete sign-in, confirm return to `/`, open dev tools Network and confirm `Authorization` header on `api/redis-server-groups` requests

---

## Phase 5 — User Story 3: Error Recovery (P3)

Goal: Users see clear, recoverable outcomes for all failure paths: bootstrap
unavailable, bootstrap fetch error, callback error, already-authenticated visit
to login, and expired session.

**Independent Test**: Each edge case can be simulated individually and produces
a visible v-alert with a retry or navigation action — no dead-ends.

### Tasks

- [ ] T014 [US3] **Verification only** — Confirm unavailable-provider display in `src/pages/login.vue` (implemented in T007): when `!isSignInAvailable && !bootstrapLoading && !error`, a Vuetify `v-alert` with `type="warning"` displays `unavailableMessage` with no provider buttons visible
- [ ] T015 [US3] **Verification only** — Confirm bootstrap error display in `src/pages/login.vue` (implemented in T007): when `error` is set, a `v-alert` with `type="error"` and a "Retry" `v-btn` that calls `authStore.loadBootstrap(true)` are rendered
- [ ] T016 [US3] Implement callback error display in `src/pages/login/callback.vue` — when `authStore.callbackError` is set after `handleCallback()` fails, show a `v-alert` with `type="error"` and a "Try again" `v-btn` that navigates to `/login` — depends on T010
- [ ] T017 [US3] Verify US3 edge cases manually: (a) simulate unavailable response — confirm warning alert; (b) simulate network failure on bootstrap — confirm error alert with retry button; (c) simulate bad callback — confirm error alert with try-again button; (d) open `/login` while signed in — confirm immediate redirect to `/` or returnUrl; (e) expire/clear localStorage OIDC user entry while on a protected page, then navigate — confirm guard detects missing session and redirects to `/login`

---

## Phase 6 — Polish & Cross-Cutting Concerns

Goal: Ensure code quality gates pass, verify accessibility baseline, and confirm
performance targets are met.

### Tasks

- [x] T018 [constitution-gate-I] Run `npm run lint` and fix any new warnings introduced by this feature in all modified and new files
- [x] T019 [constitution-gate-I] Run `npm run type-check` and fix any TypeScript errors in all new and modified files
- [ ] T020 [constitution-gate-III] Verify accessibility baseline: all provider buttons have descriptive text (provider display name), all v-alert elements are readable by screen reader, all interactive elements are keyboard-reachable on the login page
- [ ] T021 [SC-002] Verify performance targets: bootstrap fetch + first provider button render completes within 2 s on a throttled 4G connection in Chrome DevTools; route guard decision (localStorage read) completes within 50 ms (measure in Performance tab)

---

## Phase 7 — Testing (Constitution Principle II)

Goal: Establish automated test coverage for each user story's critical paths.
These tasks satisfy Constitution Principle II ("Testing Is Mandatory") and ensure
regression safety for authentication logic.

**Note**: If no test framework is configured yet, phase begins with T022 which
sets up Vitest + Vue Test Utils. Subsequent test tasks depend on T022.

### Tasks

- [x] T022 [constitution-gate-II] Set up test framework: install `vitest`, `@vue/test-utils`, `happy-dom` (or `jsdom`) as dev dependencies; create `vitest.config.ts` with Vue plugin; add `"test": "vitest run"` script to `package.json`. This is the tracked follow-up for the testing-gap exception documented in plan.md.
- [x] T023 [constitution-gate-II] [US1] Write unit tests for `src/stores/authentication.ts`: test `loadBootstrap()` populates providers on success and sets error on failure; test `startSignIn()` calls `UserManager.signinRedirect()` with correct provider config; test state transitions match data-model.md — depends on T004 and T022
- [x] T024 [constitution-gate-II] [US2] Write unit tests for `src/router/authGuard.ts`: test public paths are allowed without session; test protected paths redirect to `/login` when session invalid; test login path redirects to returnUrl when session valid; test `sanitizeReturnUrl` rejects external URLs — depends on T009 and T022
- [x] T025 [constitution-gate-II] [US3] Write unit tests for error recovery paths: test `handleCallback()` sets `callbackError` on failure; test `loadBootstrap(true)` force-reloads and clears previous error; test `ensureSessionValid()` returns false when localStorage user is expired — depends on T004 and T022

---

## Dependencies

```
T001 (install)
  └── T002, T003 (parallel)
        └── T004 (store — needs both T002 and T003)
              └── T005 (auth headers — needs store)
                    └── T012 (redisServers — needs T005)
              └── T007 (login page — needs T004)
              └── T009 (guard — needs T003 + T004)
                    └── T010 (callback page — needs T004)
                    └── T011 (router wiring — needs T009)
                          └── T013 (US2 verification)
T006 (login layout — independent after T001)
  └── T007 (login page — needs T004 + T006)
        └── T008 (US1 verification)
        └── T014, T015 (verification of error states in login page)
  └── T010 (callback page — needs T004 + T006)
        └── T016 (callback error display)
T018, T019, T020, T021 (parallel, after all implementation tasks)
T022 (test framework setup — after T001)
  └── T023 (US1 tests — needs T004 + T022)
  └── T024 (US2 tests — needs T009 + T022)
  └── T025 (US3 tests — needs T004 + T022)
```

## Parallel Execution Examples

**All [P] tasks in Phase 2** can run in parallel once T001 is done:

- T002 (API module) and T003 (router constants) have zero shared dependencies.

**T006** (login layout) can be built in parallel with T002+T003 once T001 is done.

**Polish phase** (T018–T021) are all independent of each other.

**Testing phase** (T022–T025) can run in parallel with polish once implementation is done. T023–T025 are independent of each other after T022.

---

## Implementation Strategy

**MVP scope (just US1)**: T001 → T002 → T003 → T004 → T006 → T007 → T008  
This delivers a working login screen with provider discovery in the fewest steps.

**Full delivery order**: T001 → T002+T003 (parallel) → T004 → T005+T006 (parallel)
→ T007 → T009 → T010+T011+T012 (parallel) → T013 → T008 → T014 → T015 → T016
→ T017 → T018+T019+T020+T021 (parallel) → T022 → T023+T024+T025 (parallel)

**Total tasks**: 25  
**New files**: 6 (`authentication.ts` API, `authentication.ts` store, `login.vue` page, `callback.vue`, `login.vue` layout, `auth.ts` router) + test files  
**Modified files**: 3 (`config.ts`, `redisServers.ts`, `router/index.ts`)  
**New dependencies**: `oidc-client-ts` (runtime), `vitest` + `@vue/test-utils` + `happy-dom` (dev)
