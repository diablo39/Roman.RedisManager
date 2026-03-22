# Implementation Plan: OIDC Authentication

**Branch**: `008-oidc-authentication` | **Date**: 2026-03-22 | **Spec**: `F:\Source\Repos\Roman.RedisManager\specs\008-oidc-authentication\spec.md`
**Input**: Feature specification from `/specs/008-oidc-authentication/spec.md`

## Summary

Implement OIDC-based login for the Vue 3 SPA by discovering available providers from `GET /api/authentication/bootstrap`, presenting provider sign-in actions, and enforcing authentication guards on all non-auth routes. The frontend OIDC client will be `oidc-client-ts`, with a dedicated authentication store/composable, a sign-in entry screen, a callback handling route, and global router guard enforcement.

## Technical Context

**Language/Version**: TypeScript 5.9.x, Vue 3.5.x  
**Primary Dependencies**: Vue Router 4, Pinia 3, Vuetify 3, `oidc-client-ts` (new), Fetch API  
**Storage**: Browser session storage for OIDC user/session state via `oidc-client-ts` user store  
**Testing**: Vitest + Vue Test Utils for unit/component + router guard coverage (new test dependencies)  
**Target Platform**: Modern desktop/mobile browsers running the frontend SPA  
**Project Type**: Frontend web application (single-page app)  
**Performance Goals**: Provider bootstrap rendered within 2s p95 in normal network conditions; no protected-content flash before guard redirect  
**Constraints**: All non-authentication routes require authentication; failures must show user-friendly UI states; existing Vuetify UX patterns must be preserved  
**Scale/Scope**: 1 new sign-in screen, 1 callback flow, global route guard updates across all existing pages

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Pre-Phase 0 Gate Review

- Code Quality Gate: PASS
  - Required commands: `npm run lint`, `npm run type-check`, `npm run build`
  - All modified Vue/TS files must remain strict-typed and lint clean.
- Testing Gate: PASS
  - User Story 1: provider bootstrap + provider selection tests (API state handling and redirect initiation invocation).
  - User Story 2: router guard tests for unauthenticated redirect and authenticated access.
  - User Story 3: sign-in page loading/unavailable/error state tests.
- UX Consistency Gate: PASS
  - Impacted flows: sign-in screen, auth callback transition, protected route navigation.
  - Must reuse existing layout terminology and Vuetify state components (`v-alert`, loading indicators, disabled actions).
- Performance Gate: PASS
  - Budget: provider list visible within 2s p95 for successful bootstrap responses.
  - Budget: zero protected-content paint before redirect for unauthenticated users.
  - Validation: devtools timing checks + automated guard behavior tests.
- Simplicity and Traceability Gate: PASS
  - Story-to-code mapping is direct: auth API client/store/page/router guard.
  - No additional architectural layer beyond a focused auth module in existing frontend structure.

## Project Structure

### Documentation (this feature)

```text
specs/008-oidc-authentication/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── authentication-bootstrap.md
└── tasks.md
```

### Source Code (repository root)

```text
frontend/
├── src/
│   ├── api/
│   │   └── authentication.ts          # New bootstrap/auth API client
│   ├── stores/
│   │   └── authentication.ts          # New auth state + OIDC orchestration
│   ├── pages/
│   │   ├── login.vue                  # New provider selection screen
│   │   └── auth/
│   │       └── callback.vue           # New OIDC callback handler route
│   ├── router/
│   │   └── index.ts                   # Global auth guard integration
│   └── plugins/
│       └── oidc.ts                    # Central OIDC manager factory
├── tests/
│   ├── unit/
│   │   ├── authentication.store.spec.ts
│   │   └── authentication.api.spec.ts
│   └── integration/
│       └── router-auth-guard.spec.ts
└── package.json                        # Add oidc-client-ts (+ test deps if missing)
```

**Structure Decision**: Keep all implementation inside the existing frontend SPA and add a focused authentication module (api + store + login/callback pages + router guard). This is the smallest viable shape that preserves current project conventions.

## Phase 0 Research Summary

Research decisions are documented in `research.md` and resolve library choice, guard pattern, session handling, and backend contract usage.

## Phase 1 Design Summary

- Data entities and state transitions are documented in `data-model.md`.
- Frontend-backend authentication bootstrap and redirect expectations are documented in `contracts/authentication-bootstrap.md`.
- Developer validation flow is documented in `quickstart.md`.

## Post-Design Constitution Check

- Code Quality Gate: PASS (plan includes lint/type-check/build enforcement points).
- Testing Gate: PASS (unit + integration/component coverage explicitly mapped to all stories).
- UX Consistency Gate: PASS (loading/error/unavailable/auth-redirect behavior defined and aligned with existing patterns).
- Performance Gate: PASS (measurable budget and validation approach captured in plan and quickstart).
- Simplicity and Traceability Gate: PASS (direct mapping from stories to artifacts without unnecessary abstractions).

## Complexity Tracking

No constitution violations or complexity exceptions are required for this feature.
