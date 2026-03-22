# Phase 0 Research: OIDC Authentication

## Decision 1: Use oidc-client-ts for SPA OIDC flows

- Decision: Use oidc-client-ts as the frontend OIDC client library.
- Rationale: It is purpose-built for browser-based OIDC/OAuth2 authorization code + PKCE flows, supports redirect/callback processing, token/session lifecycle handling, and cleanly integrates with Vue store-driven orchestration.
- Alternatives considered:
  - Manual OIDC implementation with direct URL composition and token handling: rejected due to higher security and maintenance risk.
  - Generic OAuth helper libraries without OIDC user/session management: rejected because they require additional custom state and protocol handling.

## Decision 2: Provider discovery contract is source-of-truth for sign-in options

- Decision: Build sign-in provider options from GET /api/authentication/bootstrap only, honoring bootstrapState, unavailableReasonCodes, and providers list.
- Rationale: This matches backend configuration truth and supports dynamic enable/disable of provider availability per environment without frontend redeploy.
- Alternatives considered:
  - Hardcoded providers in frontend config: rejected because it diverges from runtime backend capability and increases configuration drift.
  - Feature-flag-only provider toggles in frontend: rejected because backend still controls actual sign-in capability.

## Decision 3: Route protection via global Vue Router guard with explicit public route whitelist

- Decision: Add a global beforeEach guard that allows public auth routes (login + callback) and requires authenticated session for all other routes.
- Rationale: Centralized enforcement prevents inconsistent per-route checks and ensures no protected page is accessible without session state.
- Alternatives considered:
  - Per-page guards only: rejected due to duplication and risk of missed route coverage.
  - Layout-level checks only: rejected because routing should be blocked before protected content renders.

## Decision 4: Session persistence strategy uses browser session storage

- Decision: Configure oidc-client-ts user state persistence to session storage.
- Rationale: Session storage avoids long-lived persistence across browser restarts while still allowing route navigation and reload continuity in the active tab session.
- Alternatives considered:
  - Local storage persistence: rejected due to longer persistence window for sensitive session artifacts.
  - Memory-only session: rejected because full page refresh would immediately lose login state and degrade UX.

## Decision 5: Authentication callback handled by dedicated route page

- Decision: Add a dedicated callback route/page to process OIDC redirect response and then navigate to the intended destination.
- Rationale: Keeps callback-specific behavior isolated, simplifies error handling, and avoids entangling callback logic in unrelated screens.
- Alternatives considered:
  - Process callback in login page only: rejected because callback path and login entry concerns differ.
  - Process callback in router file exclusively: rejected because view-level user feedback on callback errors is still needed.

## Decision 6: Test strategy for this feature

- Decision: Add automated tests covering provider bootstrap state mapping, provider redirect initiation, and global route guard behavior.
- Rationale: These are the highest regression-risk areas and directly map to P1/P2 stories and constitution testing requirements.
- Alternatives considered:
  - Manual-only verification: rejected because it does not satisfy mandatory automated testing requirements.
  - End-to-end only: rejected because unit/integration tests are faster and better for guard/store regression loops.
