# Data Model: SPA OIDC Authentication Bootstrap

## Entity: AuthenticationBootstrapResponse

- Purpose: Root response returned to unauthenticated frontend clients to initialize authentication UX.
- Fields:
  - bootstrapState: availability state (`available`, `unavailable`) (required)
  - unavailableReasonCodes: list of machine-readable reason codes (optional, populated when `bootstrapState = unavailable`)
  - providers: collection of AuthenticationProviderSummary (required, can be empty when unavailable)
  - generatedAtUtc: timestamp string for observability (optional)
  - version: response schema version (optional)

## Entity: AuthenticationProviderSummary

- Purpose: Represents one sign-in provider option visible to the SPA.
- Fields:
  - providerKey: stable unique key, case-insensitive identity within deployment
  - displayName: human-readable provider name for UI
  - kind: provider category (EntraId, Google, GenericOidc)
  - oidc: FrontendOidcBootstrapProfile (required)

## Entity: FrontendOidcBootstrapProfile

- Purpose: Browser-safe OIDC settings consumed by a generic SPA client.
- Fields:
  - authority: issuer/discovery base URL
  - clientId: public client identifier for SPA
  - redirectUri: frontend callback URI for login completion
  - scope: requested scopes string (space-delimited)
  - responseType: expected response type (default `code`)
  - postLogoutRedirectUri: optional sign-out callback URI
  - silentRedirectUri: optional silent renew callback URI
  - automaticSilentRenew: optional bool hint
  - metadataOverrides: optional OidcMetadataOverrides payload

## Entity: OidcMetadataOverrides

- Purpose: Explicit endpoint metadata for providers where browser discovery is blocked or incomplete.
- Fields:
  - issuer
  - authorizationEndpoint
  - tokenEndpoint
  - userInfoEndpoint
  - endSessionEndpoint

## Validation Rules

- `bootstrapState` must be `available` only when `providers` contains at least one entry.
- `bootstrapState` must be `unavailable` when `providers` is empty.
- `providerKey` values must be unique case-insensitively.
- `authority`, `clientId`, and `redirectUri` are mandatory for every returned provider.
- Sensitive backend fields (e.g., signing keys, server token validation internals) are forbidden in all response entities.
- Providers disabled in backend configuration or not sign-in-capable must not appear in `providers`.

## Relationships

- AuthenticationBootstrapResponse 1..* AuthenticationProviderSummary.
- AuthenticationProviderSummary 0..1 FrontendOidcBootstrapProfile.
- FrontendOidcBootstrapProfile 0..1 OidcMetadataOverrides.

## State Transitions

- Bootstrap state lifecycle:
  - `unavailable` -> `available` when at least one provider is enabled and sign-in-capable.
  - `available` -> `unavailable` when no enabled sign-in-capable providers remain.
  - `available` remains `available` as providers are added, updated, or removed while at least one valid provider remains.
