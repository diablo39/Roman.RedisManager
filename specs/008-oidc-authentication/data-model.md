# Data Model: OIDC Authentication

## Entity: AuthenticationBootstrap

- Purpose: Represents backend bootstrap status and sign-in capability at app startup/login load.
- Fields:
  - bootstrapState: string (expected values: available, unavailable)
  - unavailableReasonCodes: string[]
  - providers: AuthenticationProvider[]
  - generatedAtUtc: string (ISO date-time)
  - version: string
- Validation Rules:
  - bootstrapState must be present.
  - providers must be present (can be empty when unavailable).
  - If bootstrapState is unavailable, unavailableReasonCodes may contain one or more reason identifiers.

## Entity: AuthenticationProvider

- Purpose: Represents a provider option shown to the user for interactive sign-in.
- Fields:
  - providerKey: string (stable identifier)
  - displayName: string
  - kind: number (OIDC provider kind enum from backend contract)
  - oidc: OidcProfile
- Validation Rules:
  - providerKey and displayName are required and non-empty.
  - oidc profile is required for redirect initiation.

## Entity: OidcProfile

- Purpose: Browser-safe OIDC config used by oidc-client-ts for redirect sign-in.
- Fields:
  - authority: string
  - clientId: string
  - redirectUri: string
  - scope: string
  - responseType: string
  - postLogoutRedirectUri: string | null
  - silentRedirectUri: string | null
  - automaticSilentRenew: boolean | null
  - metadataOverrides: OidcMetadataOverrides | null
- Validation Rules:
  - authority, clientId, redirectUri, scope, responseType are required.
  - Optional URI fields may be null.

## Entity: AuthSessionState

- Purpose: Represents frontend-authenticated status and session lifecycle state.
- Fields:
  - isAuthenticated: boolean
  - activeProviderKey: string | null
  - loading: boolean
  - error: string | null
  - returnUrl: string | null
- Validation Rules:
  - isAuthenticated defaults to false when no valid user session exists.
  - returnUrl must resolve to an internal route path.

## Entity: RouteAccessRule

- Purpose: Defines whether a route requires authentication.
- Fields:
  - routePath: string
  - requiresAuth: boolean
- Validation Rules:
  - Login and callback routes must set requiresAuth = false.
  - All other feature routes default to requiresAuth = true.

## Relationships

- AuthenticationBootstrap 1..\* AuthenticationProvider
- AuthenticationProvider 1..1 OidcProfile
- AuthSessionState drives RouteAccessRule enforcement at navigation time

## State Transitions

1. Unauthenticated -> BootstrapLoading
   - Trigger: User opens login or app checks auth state.
2. BootstrapLoading -> ProvidersAvailable
   - Trigger: Bootstrap response with available state and at least one provider.
3. BootstrapLoading -> ProvidersUnavailable
   - Trigger: Bootstrap response with unavailable state or empty providers for sign-in.
4. ProvidersAvailable -> RedirectingToProvider
   - Trigger: User selects provider.
5. RedirectingToProvider -> CallbackProcessing
   - Trigger: IdP redirects back to callback URI.
6. CallbackProcessing -> Authenticated
   - Trigger: oidc-client-ts callback processing succeeds and user session is established.
7. CallbackProcessing -> AuthError
   - Trigger: callback or token processing fails.
8. Authenticated -> Unauthenticated
   - Trigger: session expiration, sign-out, or auth-state invalidation.
