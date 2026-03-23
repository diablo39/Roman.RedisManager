# Implementation Plan: Frontend Authentication

**Branch**: `008-frontend-authentication` | **Date**: 2026-03-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-frontend-authentication/spec.md`

## Summary

Add OIDC-based authentication to the Vue 3 frontend using `oidc-client-ts`. The
backend already exposes `GET /api/authentication/bootstrap` which returns the list
of available sign-in providers with all OIDC configuration the SPA needs. The
frontend will: fetch that list, show a separated login page with provider buttons,
redirect the user to the chosen provider, handle the callback, attach the access
token to every protected API request, and guard all routes except the login flow.

Design principle: **keep it simple enough that a 10-year-old can follow the code**.
Each piece does one obvious thing; no abstractions beyond what is needed right now.

## Technical Context

**Language/Version**: TypeScript 5.9 / Vue 3.5  
**Primary Dependencies**: Vue 3, Vuetify 3, Pinia 3, Vue Router 4, `oidc-client-ts` (new)  
**Storage**: Browser `sessionStorage` (OIDC state) and `localStorage` (user session via `oidc-client-ts`)  
**Testing**: Manual verification (no test framework currently configured)  
**Target Platform**: Modern browsers (Chrome, Edge, Firefox, Safari latest two)  
**Project Type**: Single-page web application (SPA)  
**Performance Goals**: Login screen renders providers within 2 s; route guard decisions < 50 ms  
**Constraints**: No client secrets (public PKCE client); no refresh tokens (re-authenticate on expiry)  
**Scale/Scope**: Single frontend application, ~6 pages, 1–5 OIDC providers

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

- **Code Quality Gate**: `npm run lint` (ESLint + Vuetify config) and `npm run type-check`
  (`vue-tsc`) must pass with zero new warnings for all new/changed files.
- **Testing Gate**: No automated test framework is configured in the project.
  Manual verification steps will be defined for each user story. A follow-up task
  to add component tests is out of scope but noted.
- **UX Consistency Gate**: The login page uses the same Vuetify components
  (v-card, v-btn, v-alert, v-progress-circular) and feedback patterns (loading
  spinner, error alert with retry) already established in the default layout and
  redis detail page. The login page is a **separate page** with its **own layout**
  that omits the navigation drawer and app bar shown to authenticated users.
- **Performance Gate**: Bootstrap fetch + render < 2 s on a 4G connection.
  Route guard token check < 50 ms (local storage read). Validated by manual
  observation in dev tools Network tab.
- **Simplicity and Traceability Gate**:
  - US-1 (provider discovery + sign-in) → `src/api/authentication.ts`, `src/stores/authentication.ts`, `src/pages/login.vue`
  - US-2 (route protection + token injection) → `src/router/authGuard.ts`, `src/api/config.ts`
  - US-3 (error recovery) → error states in `src/stores/authentication.ts`, displayed in `src/pages/login.vue`
  - FR-012 (authenticated Redis requests) → `Authorization` header added in `src/api/config.ts`
  - No new abstractions beyond one store and one API module.

## Project Structure

### Documentation (this feature)

```text
specs/008-frontend-authentication/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── authentication-bootstrap.md
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
frontend/
└── src/
    ├── api/
    │   ├── config.ts              ← MODIFY: add getAuthHeaders() helper
    │   ├── authentication.ts      ← NEW: fetch bootstrap endpoint
    │   └── redisServers.ts        ← MODIFY: attach auth headers to fetch calls
    ├── stores/
    │   └── authentication.ts      ← NEW: Pinia store for auth state
    ├── pages/
    │   ├── login.vue              ← NEW: separated login page
    │   └── login/
    │       └── callback.vue       ← NEW: OIDC redirect callback page
    ├── layouts/
    │   ├── default.vue            ← EXISTING: authenticated layout (no changes)
    │   └── login.vue              ← NEW: minimal layout for login pages
    ├── router/
    │   ├── index.ts               ← MODIFY: register auth guard
    │   ├── auth.ts                ← NEW: public path list + constants
    │   └── authGuard.ts           ← NEW: beforeEach guard
    └── plugins/
        └── index.ts               ← EXISTING: no changes needed
```

**Structure Decision**: Single frontend project; no new top-level directories.
Authentication adds 6 new files and modifies 3 existing files.

## Constitution Check — Post-Design Re-Evaluation

All five gates re-evaluated after Phase 1 design artifacts were produced:

- **Code Quality Gate** ✅: All new files are TypeScript with strict types. No
  `any` usage planned. `npm run lint` and `npm run type-check` will be run before
  any merge.
- **Testing Gate** ⚠️: No automated test framework is configured in the project.
  Manual verification steps are defined in quickstart.md. This is an **accepted
  pre-existing gap** with the following exception record:
  - **Exception**: Automated testing infrastructure is absent.
  - **Target remediation**: Next feature cycle (before feature `010`).
  - **Tracked by**: tasks.md T022 (follow-up task to establish test framework and
    add retroactive component tests for this feature).
- **UX Consistency Gate** ✅: Login page uses the same v-card, v-btn, v-alert,
  v-progress-circular patterns as existing pages. Separated login layout avoids
  flashing the authenticated chrome. Error display follows the retry-alert pattern
  from the Redis detail page.
- **Performance Gate** ✅: Single fetch on login mount (< 2 s target). Guard
  reads from localStorage (< 50 ms). No bundle size concern — `oidc-client-ts`
  is ~30 KB gzipped.
- **Simplicity and Traceability Gate** ✅: 6 new files, 3 modified files. One
  new dependency. No new abstractions beyond one store and one API module. Every
  file maps to a spec requirement (see plan §Constitution Check).

**Result**: All gates pass. No violations requiring justification.

## Complexity Tracking

No constitution violations. No complexity justifications required.
