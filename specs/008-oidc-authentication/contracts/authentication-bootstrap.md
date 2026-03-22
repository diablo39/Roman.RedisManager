# Contract: Authentication Bootstrap and Frontend Guarding

## Purpose

Define the frontend-consumed authentication bootstrap contract and route access behavior required to support OIDC sign-in.

## Backend Endpoint Consumed

- Method: GET
- Path: /api/authentication/bootstrap
- Source: Roman.RedisManager.Web.json

## Response Shape Used by Frontend

- bootstrapState: string
  - available: at least one provider can be used for interactive sign-in
  - unavailable: no provider currently sign-in-capable
- unavailableReasonCodes: string[]
- providers: AuthenticationBootstrapProviderDto[]
  - providerKey: string
  - displayName: string
  - kind: number
  - oidc:
    - authority: string
    - clientId: string
    - redirectUri: string
    - scope: string
    - responseType: string
    - postLogoutRedirectUri: string | null
    - silentRedirectUri: string | null
    - automaticSilentRenew: boolean | null
    - metadataOverrides: object | null
- generatedAtUtc: string (date-time)
- version: string

## Frontend Behavior Contract

1. Provider discovery

- On login view load, frontend requests GET /api/authentication/bootstrap.
- If bootstrapState is available and providers contains entries, frontend renders selectable provider actions.
- If bootstrapState is unavailable, frontend renders unavailable state and disables sign-in actions.

2. Redirect initiation

- On provider selection, frontend constructs/uses oidc-client-ts configuration from provider.oidc and starts redirect sign-in.
- Frontend stores intended returnUrl before redirect when user came from protected route.

3. Route guarding

- Public routes: login and callback processing route.
- Protected routes: every other route in the SPA.
- If user is unauthenticated on protected navigation, redirect to login.
- After callback success, user is redirected to stored returnUrl or default home route.

4. Error handling

- Bootstrap request failure shows user-friendly retryable error.
- Redirect/callback failure shows user-friendly error and allows retry from login.

## Non-Goals

- Backend identity provider provisioning.
- Backend token issuance or session policy changes.
- Multi-tenant provider selection semantics beyond backend-provided provider list.
