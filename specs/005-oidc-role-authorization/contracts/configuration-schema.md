# Contract: Configuration Schema (AuthN/AuthZ)

## Purpose

Defines the declarative configuration contract for identity providers, role mappings, and permission rules consumed by the authentication/authorization pipeline.

## Top-Level Sections

- `Security:Authentication:Providers`
- `Security:Authorization:Roles`
- `Security:Authorization:RoleClaimMappings`
- `Security:Authorization:Permissions:Global`
- `Redis:ServerGroups[*]:AuthorizationOverrides`

## Schema (Conceptual)

## `Providers[]`

- `ProviderKey` (string, required, unique)
- `DisplayName` (string, required)
- `Enabled` (bool, required)
- `Authority` (string URI, required)
- `ClientId` (string, required)
- `ClientSecret` (string, optional)
- `MetadataAddress` (string URI, optional)
- `Kind` (enum, required): `EntraId | Google | GenericOidc`

Validation:
- `ProviderKey` unique.
- At least one provider enabled.
- URI fields must be valid absolute URIs.

## `Roles[]`

- `RoleName` (string, required, unique)
- `Description` (string, optional)

Validation:
- Non-empty role name.
- Role names are case-insensitive unique in effective policy set.

## `RoleClaimMappings[]`

- `RoleName` (string, required)
- `ProviderKey` (string, required)
- `ClaimKey` (string, required)
- `AllowedValues[]` (string[], required, non-empty)
- `MatchMode` (enum, required): `Any | All`

Validation:
- `RoleName` must exist in `Roles`.
- `ProviderKey` must exist in `Providers`.
- No duplicate exact mapping tuples (`RoleName`,`ProviderKey`,`ClaimKey`,`AllowedValues`).

## `Permissions:Global[]`

- `Action` (string enum, required)
- `AllowedRoles[]` (string[], required)

Validation:
- Every `AllowedRoles` entry must exist in `Roles`.
- `Action` must be a supported protected action.

## `Redis:ServerGroups[]:AuthorizationOverrides`

- `Mode` (enum, required): `ReplaceDefaults | NarrowDefaults`
- `Rules[]` (required)
- `Rules[].Action` (string enum, required)
- `Rules[].AllowedRoles[]` (string[], required)

Validation:
- Each `AuthorizationOverrides` block applies to the containing `ServerGroup`.
- Duplicate action entries within one group override block are invalid.
- Roles in `AllowedRoles[]` must exist in `Roles`.

## Policy Evaluation Contract

1. Identify provider context from authenticated principal.
2. Apply matching `RoleClaimMappings` to derive normalized internal roles.
3. Identify requested protected action and target `GroupId` (if applicable).
4. If a group override exists for `GroupId` and action:
- evaluate override according to `Mode`.
5. Otherwise evaluate global permission rule for action.
6. Deny when no allow rule matches.

## Failure Contract

- Invalid configuration at startup MUST fail options validation and prevent service start.
- Requests with unresolved policy context MUST be denied with authorization failure (not silent allow).
