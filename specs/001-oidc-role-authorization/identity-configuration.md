# Identity Configuration Instructions

This guide defines how to configure identity providers for the OIDC AuthN/AuthZ feature.

## Goal

Enable three provider profiles:
- EntraId
- Google
- GenericOidc

The application authenticates API callers via bearer token and then normalizes claims into internal roles.

## 1. Configuration Sections

Add these sections to `backend/src/Roman.RedisManager.Web/appsettings.json` (or environment-specific override files):
- `Security:Authentication:Providers`
- `Security:Authorization:Roles`
- `Security:Authorization:RoleClaimMappings`
- `Security:Authorization:Permissions:Global`
- `Redis:ServerGroups[*]:AuthorizationOverrides`

## 2. Provider Entry Shape

Each provider entry in `Security:Authentication:Providers` should include:
- `ProviderKey` (unique key, for example `entra`, `google`, `generic`)
- `DisplayName`
- `Enabled`
- `Kind` (`EntraId | Google | GenericOidc`)
- `Authority`
- `ClientId`
- `ClientSecret` (when required)
- `MetadataAddress` (optional)

## 3. EntraId Profile

Recommended values:
- `ProviderKey`: `entra`
- `Kind`: `EntraId`
- `Authority`: Microsoft tenant authority URL
- `ClientId`: application registration client id
- `ClientSecret`: registration secret (if confidential client flow)

Common claim mapping patterns:
- `ClaimKey`: `groups`
- `AllowedValues`: Azure AD group object ids or agreed aliases

Example mapping intent:
- Internal role `redis-reader` maps to Entra group claim value `admin_group` (or your tenant-specific group id).

## 4. Google Profile

Recommended values:
- `ProviderKey`: `google`
- `Kind`: `Google`
- `Authority`: Google OIDC authority URL
- `ClientId`: Google OAuth client id
- `ClientSecret`: OAuth client secret

Common claim mapping patterns:
- `ClaimKey`: `groups` or custom mapped claim (depending on identity broker)
- `AllowedValues`: group names like `readers`, `admins`

Example mapping intent:
- Internal role `redis-reader` maps to claim value `readers` for the Google provider.

## 5. GenericOidc Profile

Recommended values:
- `ProviderKey`: `generic`
- `Kind`: `GenericOidc`
- `Authority`: issuer authority URL
- `MetadataAddress`: optional custom discovery endpoint
- `ClientId` and optional `ClientSecret`: based on issuer requirements

Use this profile for any standards-compliant OIDC issuer not covered by EntraId or Google presets.

## 6. Internal Roles

Define provider-agnostic role names in `Security:Authorization:Roles`, for example:
- `redis-reader`
- `editor`
- `admin`

These role names are the only values used in endpoint authorization attributes and policies.

## 7. Role Claim Mappings

Use `Security:Authorization:RoleClaimMappings` to translate external claims into internal roles.

Each mapping row should include:
- `RoleName`
- `ProviderKey`
- `ClaimKey`
- `AllowedValues`
- `MatchMode` (`Any` or `All`)

Guidance:
- Keep mappings explicit by provider.
- Prefer stable provider values (ids over display names when possible).
- Avoid overlapping mappings that create unintended elevated role assignment.

## 8. Permissions and Group Overrides

Configure baseline permissions in `Security:Authorization:Permissions:Global`.

Use `Redis:ServerGroups[*]:AuthorizationOverrides` for stricter group-specific rules:
- Example: allow `editor` globally for key mutation.
- Override `prod-cache` to allow only `admin` for key mutation.

Evaluation order:
1. Group override for `groupId` + action (if present)
2. Global permission rule
3. Deny if no allow rule matches

## 9. Validation Rules Before Running

Confirm all of the following:
- Provider keys are unique.
- At least one provider is enabled.
- Every mapping references existing `RoleName` and `ProviderKey`.
- Every permission role exists in `Roles`.
- Every group override references a known Redis group id.

Startup should fail fast on invalid configuration via options validation.

## 10. Rollout Checklist

1. Add provider entries for EntraId, Google, and GenericOidc.
2. Add internal roles.
3. Add role-claim mappings for each provider.
4. Add global permissions.
5. Add group-specific overrides for sensitive groups.
6. Run build and tests.
7. Validate one bearer-token flow per provider.
8. Validate allow/deny behavior for at least one overridden group.
