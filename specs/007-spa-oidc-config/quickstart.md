# Quickstart: SPA OIDC Authentication Bootstrap

## Goal

Enable a Vue 3 SPA to authenticate against this backend using backend-provided OIDC bootstrap settings.

## Prerequisites

- Backend running with at least one enabled OIDC provider in `Security:Authentication:Providers`.
- Frontend has a login callback route and (optionally) logout/silent callback routes.
- Frontend uses a generic SPA OIDC client (`oidc-client-ts`).

## Steps

1. Configure backend authentication providers in environment settings.
2. Start backend and verify the bootstrap endpoint returns `bootstrapState = available` with at least one provider.
3. In frontend startup, request bootstrap payload before authentication.
4. Let user select provider (or choose default).
5. Map returned OIDC profile fields into `oidc-client-ts` `UserManager` settings.
6. Start redirect sign-in flow using authorization code + PKCE.
7. Send returned bearer token to API requests.

## Verification

1. Bootstrap request succeeds without existing authentication token.
2. Response contains providerKey/displayName and browser-safe OIDC fields.
3. Response contains no server secrets or signing keys.
4. Sign-in succeeds with at least one Generic OIDC provider and one non-generic provider.
5. Changing backend provider settings changes bootstrap response after app restart/config reload.

## Negative Checks

1. Disabled provider is not returned as available.
2. When no enabled sign-in-capable provider exists, response returns `bootstrapState = unavailable` with deterministic reason codes.
3. Invalid provider selection on frontend is rejected by subsequent bootstrap refresh.

## Performance Check

1. Verify bootstrap endpoint p95 latency stays under 100 ms under normal local test conditions.

## Testing Notes

- Add xUnit + Shouldly tests for CQRS mapping and controller authorization behavior.
- If tests are changed, execute mutation loop:
  - run mutation tests
  - analyze survivors
  - improve assertions
  - rerun and compare

## Execution Notes

1. Unit/integration tests passed after implementation updates (282 passed, 0 failed).
2. Mutation baseline score was 55.97% for Roman.RedisManager.Application.
3. Targeted assertion hardening for AuthenticationBootstrapQueryHandler increased score to 72.33%.
4. Mutation delta is +16.36 and marked as improved.
5. Bootstrap endpoint latency verification completed locally with 250 measured requests after warm-up.
6. Observed latency metrics: p95 = 0.79 ms, average = 0.45 ms, min = 0.28 ms, max = 1.91 ms.
7. Result: p95 is below the <100 ms requirement.
