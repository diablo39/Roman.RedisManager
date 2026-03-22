# Research: SPA OIDC Authentication Bootstrap

## Decision 1: Recommend oidc-client-ts for the Vue 3 SPA

- Decision: Use `oidc-client-ts` as the default recommended client library for the frontend.
- Rationale: It is provider-neutral, browser-focused, supports OIDC/OAuth2 standards, and has clear support for authorization code + PKCE and session/token management patterns needed by a SPA.
- Alternatives considered:
  - `@axa-fr/oidc-client`: Strong option with additional service worker and DPoP features; rejected as default due to larger integration surface and less common baseline usage for simple SPA bootstrap scenarios.
  - Provider-specific SDKs: Rejected because they increase vendor lock-in and conflict with the feature goal of interoperating with any OIDC server.

## Decision 2: Backend bootstrap payload should mirror browser-safe UserManager settings

- Decision: Return only browser-safe fields required by SPA OIDC startup, including authority, client_id, redirect_uri, scope, and optional post-logout/silent renew fields and metadata overrides.
- Rationale: This directly satisfies frontend startup requirements while keeping configuration centrally managed by backend environment settings.
- Alternatives considered:
  - Return only provider key and authority: Rejected because frontend would still need environment-specific hardcoded values.
  - Return full backend provider config: Rejected due to security risk and leakage of backend-only validation settings.

## Decision 3: Explicitly exclude sensitive server-side fields

- Decision: Never return signing keys, private token validation settings, or any secrets in bootstrap payload.
- Rationale: Browser-delivered configuration is public by nature; exposing server-only fields would violate security boundaries.
- Alternatives considered:
  - Masked secret placeholders: Rejected as unnecessary and potentially misleading.
  - Encrypted secret payloads for frontend decryption: Rejected because browser context cannot provide true secrecy.

## Decision 4: Add a dedicated unauthenticated bootstrap endpoint

- Decision: Provide a read-only endpoint for frontend auth bootstrap that is accessible before login.
- Rationale: The frontend must fetch provider options before initiating authentication; fallback auth policy would otherwise block this.
- Alternatives considered:
  - Embed bootstrap in static frontend files: Rejected because environment updates would require frontend rebuild/redeploy.
  - Reuse existing protected endpoints: Rejected because login cannot start without pre-auth configuration.

## Decision 5: Keep bootstrap options aligned with backend accepted providers

- Decision: Only include providers that are enabled and valid from backend configuration and that map to backend token validation acceptance.
- Rationale: Prevents frontend from initiating flows with providers whose tokens the API will reject.
- Alternatives considered:
  - Return all configured providers including disabled entries: Rejected due to inconsistent UX and invalid authentication starts.
  - Frontend-side filtering only: Rejected because backend remains source of truth.
