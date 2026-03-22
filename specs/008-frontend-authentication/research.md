# Research: Frontend Authentication

**Feature**: 008-frontend-authentication  
**Date**: 2026-03-22

## 1. OIDC Client Library Choice

**Decision**: Use `oidc-client-ts`  
**Rationale**: The user explicitly requested this library. It is a maintained,
TypeScript-native OIDC Relying Party library that implements Authorization Code

- PKCE out of the box. It provides `UserManager` which handles redirect, callback,
  silent renew, and user session lifecycle with a minimal API surface.  
  **Alternatives considered**:

* `@azure/msal-browser` — Entra ID-specific; would not support GenericOidc or
  Google providers without extra wrappers.
* `vue-oidc-client` — Thin Vue wrapper over `oidc-client-ts`; adds unnecessary
  indirection for this simple use case.

## 2. Authentication Flow Pattern

**Decision**: Authorization Code flow with PKCE, redirect-based  
**Rationale**: The backend contract returns `responseType: "code"` and the
reference article (vouch.sh/docs/applications/vue/) confirms PKCE is required for
browser clients. `oidc-client-ts` enables PKCE by default.  
**Alternatives considered**:

- Implicit flow — deprecated by OAuth 2.1; not supported by `oidc-client-ts`
  defaults.
- BFF (Backend-For-Frontend) pattern — adds server-side complexity outside the
  scope of this frontend-only feature.

## 3. How Bootstrap Maps to UserManager Config

**Decision**: Create one `UserManager` instance per provider, configured from
the bootstrap OIDC profile fields.  
**Rationale**: The bootstrap response already supplies every field `UserManager`
needs (`authority`, `client_id`, `redirect_uri`, `scope`, `response_type`,
`post_logout_redirect_uri`, `silent_redirect_uri`, `automaticSilentRenew`,
and `metadataOverrides`). The mapping is direct — no transformation layer needed.

| Bootstrap field         | UserManager setting         |
| ----------------------- | --------------------------- |
| `authority`             | `authority`                 |
| `clientId`              | `client_id`                 |
| `redirectUri`           | `redirect_uri`              |
| `scope`                 | `scope`                     |
| `responseType`          | `response_type`             |
| `postLogoutRedirectUri` | `post_logout_redirect_uri`  |
| `silentRedirectUri`     | `silent_redirect_uri`       |
| `automaticSilentRenew`  | `automaticSilentRenew`      |
| `metadataOverrides.*`   | `metadata` (partial object) |

## 4. Login Page Separation Strategy

**Decision**: `src/pages/login.vue` uses a dedicated `login` layout that renders
only a centered card — no app bar, no navigation drawer. The callback page
`src/pages/login/callback.vue` uses the same layout.  
**Rationale**: The user explicitly asked for a separated login page. A distinct
layout prevents the authenticated chrome (sidebar, header) from flashing before
the guard redirects.  
**Alternatives considered**:

- Reuse the default layout and conditionally hide nav elements — adds conditional
  logic to the layout and risks content flash.

## 5. Route Protection Strategy

**Decision**: A single `router.beforeEach` guard in `src/router/authGuard.ts`
checks if the user has a valid `oidc-client-ts` `User` object with an unexpired
access token. Public paths (`/login`, `/login/callback`) are allowed through.
Everything else requires authentication or redirects to `/login`.  
**Rationale**: Simplest possible guard — one function, one check, one redirect.
No per-route metadata needed because the default is "protected".  
**Alternatives considered**:

- Per-route `meta.requiresAuth` — inverted default; every new page would need
  the flag. Error-prone for a project where almost everything is protected.

## 6. Token Injection for API Requests

**Decision**: Add a `getAuthHeaders()` helper in `src/api/config.ts` that reads
the current user from `UserManager` and returns `{ Authorization: 'Bearer <token>' }`.
Each `fetch()` call in the API layer includes these headers.  
**Rationale**: The project already uses plain `fetch()` — no Axios, no interceptor
layer. Adding headers to individual calls is the minimal change. Creating an HTTP
wrapper or interceptor would be over-engineering for two API functions.  
**Alternatives considered**:

- Fetch wrapper/interceptor class — adds an abstraction not needed for 2-3 fetch
  calls.
- Pinia action that injects headers — mixes concerns between state and HTTP.

## 7. Session Persistence

**Decision**: Let `oidc-client-ts` manage session storage. Use
`WebStorageStateStore` backed by `localStorage` so the session survives page
refreshes and new tabs.  
**Rationale**: `oidc-client-ts` handles serialization, expiry checks, and cleanup
internally. No custom session persistence code needed.  
**Alternatives considered**:

- `sessionStorage` — session lost on new tab; poor UX.
- Custom cookie storage — unnecessary complexity; the access token is not
  sensitive to XSS beyond what `localStorage` already exposes (no refresh token
  is issued).

## 8. Error / Unavailable State Handling

**Decision**: The authentication Pinia store tracks `bootstrapState`,
`error`, and `callbackError`. The login page reads these and shows a
Vuetify `v-alert` with a retry button — same pattern used on the Redis
server detail page.  
**Rationale**: Consistent with existing UX patterns; no new error display
mechanism.  
**Alternatives considered**: Toast/snackbar — not used elsewhere in the app.
