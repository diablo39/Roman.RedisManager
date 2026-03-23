# Contract: Authentication Bootstrap Endpoint

**Feature**: 008-frontend-authentication  
**Date**: 2026-03-22  
**Source**: `Roman.RedisManager.Web.json` (OpenAPI 3.1.1)

## Endpoint

```
GET /api/authentication/bootstrap
```

No authentication required (public endpoint).

## Response — 200 OK

```jsonc
{
  // "available" → at least one provider can sign in
  // "unavailable" → no provider is sign-in-capable right now
  "bootstrapState": "available",

  // Machine-readable codes explaining why sign-in is unavailable.
  // Empty when bootstrapState is "available".
  // Known codes: NO_ENABLED_PROVIDERS, NO_SIGNIN_CAPABLE_PROVIDERS,
  //              OIDC_CONFIGURATION_INCOMPLETE
  "unavailableReasonCodes": [],

  // Array of sign-in-capable providers. Empty when unavailable.
  "providers": [
    {
      "providerKey": "entra-prod",
      "displayName": "Microsoft (Work Account)",
      "kind": 0, // OidcProviderKind enum (integer)
      "oidc": {
        "authority": "https://login.microsoftonline.com/{tenant}/v2.0",
        "clientId": "00000000-0000-0000-0000-000000000000",
        "redirectUri": "https://app.example.com/login/callback",
        "scope": "openid profile email",
        "responseType": "code",
        "postLogoutRedirectUri": "https://app.example.com/",
        "silentRedirectUri": null,
        "automaticSilentRenew": false,
        "metadataOverrides": null,
      },
    },
  ],

  "generatedAtUtc": "2026-03-22T12:00:00Z",
  "version": "1.0",
}
```

## Frontend Usage

1. Call this endpoint once when the login page mounts.
2. If `bootstrapState === "available"`, render one button per provider using
   `displayName`.
3. If `bootstrapState === "unavailable"`, show a user-friendly message derived
   from `unavailableReasonCodes`.
4. When the user clicks a provider button, create a `UserManager` from the
   provider's `oidc` fields and call `signinRedirect()`.

## TypeScript Types (to be created in `src/api/authentication.ts`)

```typescript
export interface AuthenticationBootstrapProvider {
  providerKey: string;
  displayName: string;
  kind: number;
  oidc: {
    authority: string;
    clientId: string;
    redirectUri: string;
    scope: string;
    responseType: string;
    postLogoutRedirectUri: string | null;
    silentRedirectUri: string | null;
    automaticSilentRenew: boolean | null;
    metadataOverrides: {
      issuer: string | null;
      authorizationEndpoint: string | null;
      tokenEndpoint: string | null;
      userInfoEndpoint: string | null;
      endSessionEndpoint: string | null;
    } | null;
  };
}

export interface AuthenticationBootstrapResult {
  bootstrapState: "available" | "unavailable";
  unavailableReasonCodes: string[];
  providers: AuthenticationBootstrapProvider[];
  generatedAtUtc: string;
  version: string;
}
```
