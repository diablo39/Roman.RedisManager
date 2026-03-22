# Data Model: Frontend Authentication

**Feature**: 008-frontend-authentication  
**Date**: 2026-03-22

## Entities

### AuthenticationBootstrapProvider

Represents one OIDC provider the user can sign in with. Returned by the backend
bootstrap endpoint. Used to configure an `oidc-client-ts` `UserManager`.

| Field       | Type        | Required | Description                                  |
| ----------- | ----------- | -------- | -------------------------------------------- |
| providerKey | string      | yes      | Stable unique key for this provider          |
| displayName | string      | yes      | Human-readable name shown on login buttons   |
| kind        | number      | yes      | Provider kind (EntraId, Google, GenericOidc) |
| oidc        | OidcProfile | yes      | OIDC configuration for UserManager           |

### OidcProfile

OIDC settings for one provider. Every field maps directly to an `oidc-client-ts`
`UserManagerSettings` property (see research.md §3).

| Field                 | Type                      | Required | Description                           |
| --------------------- | ------------------------- | -------- | ------------------------------------- |
| authority             | string                    | yes      | Issuer base URL                       |
| clientId              | string                    | yes      | Public client ID                      |
| redirectUri           | string                    | yes      | Post-login redirect URI               |
| scope                 | string                    | yes      | Space-delimited scopes                |
| responseType          | string                    | yes      | OIDC response type ("code")           |
| postLogoutRedirectUri | string \| null            | no       | Post-logout redirect URI              |
| silentRedirectUri     | string \| null            | no       | Silent renew callback URI             |
| automaticSilentRenew  | boolean \| null           | no       | Enable automatic silent renew         |
| metadataOverrides     | MetadataOverrides \| null | no       | Optional discovery metadata overrides |

### MetadataOverrides

Optional endpoint overrides when the provider discovery document is non-standard.

| Field                 | Type           | Required | Description                     |
| --------------------- | -------------- | -------- | ------------------------------- |
| issuer                | string \| null | yes\*    | Issuer identifier override      |
| authorizationEndpoint | string \| null | yes\*    | Authorization endpoint override |
| tokenEndpoint         | string \| null | yes\*    | Token endpoint override         |
| userInfoEndpoint      | string \| null | yes\*    | UserInfo endpoint override      |
| endSessionEndpoint    | string \| null | yes\*    | End-session endpoint override   |

\*Required in the schema but each value may be null (meaning "no override").

### AuthenticationBootstrapResult

Top-level response from `GET /api/authentication/bootstrap`.

| Field                  | Type                              | Required | Description                           |
| ---------------------- | --------------------------------- | -------- | ------------------------------------- |
| bootstrapState         | "available" \| "unavailable"      | yes      | Whether sign-in is currently possible |
| unavailableReasonCodes | string[]                          | yes      | Machine-readable reason codes         |
| providers              | AuthenticationBootstrapProvider[] | yes      | Sign-in-capable providers             |
| generatedAtUtc         | string (ISO 8601)                 | yes      | Response generation timestamp         |
| version                | string                            | yes      | Bootstrap schema version              |

## Frontend-Only State (Pinia Store)

These fields exist only in the Pinia authentication store and are not returned by
the backend.

| Field             | Type                              | Default       | Description                              |
| ----------------- | --------------------------------- | ------------- | ---------------------------------------- |
| providers         | AuthenticationBootstrapProvider[] | []            | Cached providers from bootstrap          |
| bootstrapState    | "available" \| "unavailable"      | "unavailable" | Cached availability state                |
| bootstrapLoading  | boolean                           | false         | Bootstrap request in flight              |
| bootstrapLoaded   | boolean                           | false         | Bootstrap has been fetched at least once |
| activeProviderKey | string \| null                    | null          | Provider used for current sign-in        |
| isAuthenticated   | boolean                           | false         | Whether user has a valid session         |
| returnUrl         | string \| null                    | null          | Preserved return destination             |
| error             | string \| null                    | null          | Bootstrap fetch error message            |
| callbackError     | string \| null                    | null          | Callback processing error message        |

## Relationships

```
AuthenticationBootstrapResult
  └── providers[] ──► AuthenticationBootstrapProvider
                          └── oidc ──► OidcProfile
                                          └── metadataOverrides ──► MetadataOverrides (nullable)
```

## State Transitions

```
[Signed Out] ──(open login page)──► [Bootstrap Loading]
[Bootstrap Loading] ──(success)──► [Providers Available] or [Unavailable]
[Providers Available] ──(select provider)──► [Redirecting to Provider]
[Redirecting to Provider] ──(browser returns)──► [Callback Processing]
[Callback Processing] ──(token ok)──► [Signed In]
[Callback Processing] ──(token missing)──► [Callback Error]
[Signed In] ──(token expired + silent renew fails)──► [Signed Out]
```
