# Research: Endpoint Authorisation Rules

**Date**: 2026-03-15 | **Branch**: `003-endpoint-auth-rules`

## Decision 1: Policy Replacement Strategy

**Decision**: Replace existing ReadKeys/DeleteKeysByGroup policies and GroupPermission infrastructure entirely with two simple role-based policies: "reader" and "editor".

**Rationale**: The current system has:
- `ReadKeys` policy requiring roles `reader`, `editor`, or `admin`
- `DeleteKeysByGroup` policy with a custom `GroupPermissionAuthorizationHandler` that checks per-group authorization overrides
- A `GroupPermissionRequirement`, `IGroupContextAccessor`, `RouteGroupContextAccessor`, and `AuthorizationDecisionLogger` supporting group-level permissions
- `PermissionAction` enum with 5 actions (ReadKeys, ReadMetadata, ReadValues, DeleteKey, WriteKey)

The user explicitly chose full replacement. The new model is simpler: "reader" for GET, "editor" for POST/PUT/DELETE/PATCH.

**Alternatives considered**:
- Layering new policies on top of existing group model — rejected (adds complexity, user chose replacement)
- Mapping existing granular permissions into reader/editor — rejected (user wants clean replacement)

## Decision 2: Components to Remove

**Decision**: Remove the following group-based authorization infrastructure:
- `GroupPermissionRequirement.cs`
- `GroupPermissionAuthorizationHandler.cs`
- `IGroupContextAccessor.cs`
- `RouteGroupContextAccessor.cs`
- `AuthorizationPolicyTypes.cs` (PermissionAction enum)
- `AuthorizationPermissionConfiguration.cs`
- `AuthorizationOverrides` from `RedisServerGroupConfiguration`

**Rationale**: These are all artifacts of the group-based permission model being replaced. The `AuthorizationDecisionLogger` should be retained and adapted since it provides useful observability.

**Alternatives considered**:
- Keep group-based infrastructure dormant — rejected (dead code, confusing)

## Decision 3: Components to Retain and Adapt

**Decision**: Keep and adapt:
- `NormalizedRoleClaimsTransformation` — still maps OIDC claims to roles
- `RoleClaimMappingEvaluator` — still evaluates claim-to-role mappings
- `AuthorizationRoleMappingConfiguration` — still configures claim mappings per provider
- `AuthorizationDecisionLogger` — adapt to log reader/editor policy decisions
- `BearerHeaderAuthenticationHandler` — unchanged
- All OIDC provider profiles — unchanged

**Rationale**: The claim-to-role mapping infrastructure is exactly what supports "custom claim names configurable per deployment" (FR-013/FR-014). The role mapping config already allows mapping arbitrary claims from any OIDC provider to application roles.

## Decision 4: Policy Definition Approach

**Decision**: Define two ASP.NET Core authorization policies:
- `Reader`: `RequireAuthenticatedUser()` + `RequireRole("reader", "editor", "admin")`
- `Editor`: `RequireAuthenticatedUser()` + `RequireRole("editor", "admin")`

**Rationale**:
- "admin" role implicitly has both reader and editor access (standard RBAC practice)
- "editor" role implicitly has reader access is NOT assumed (per FR-006, policies are independent). However, in practice an editor likely needs read access too. The policy definitions allow admin to have both, but editor only gets write. Users must explicitly assign both roles if an editor needs read access.
- Wait — re-reading FR-006: "reader and editor policies MUST be independent — possessing one does not imply the other." This means the Reader policy should only require `reader` or `admin`, and Editor should only require `editor` or `admin`. An `editor` role should NOT grant read access.

Updated definition:
- `Reader`: `RequireAuthenticatedUser()` + `RequireRole("reader", "admin")`
- `Editor`: `RequireAuthenticatedUser()` + `RequireRole("editor", "admin")`

**Alternatives considered**:
- Single combined "readwrite" policy — rejected (doesn't match spec requirement for independent policies)
- Custom IAuthorizationHandler for each — rejected (RequireRole is sufficient, simpler)

## Decision 5: OpenAPI Security Scheme

**Decision**: Use ASP.NET Core's built-in OpenAPI transformer to add a Bearer security scheme and per-endpoint security requirements.

**Rationale**: .NET 10's `AddOpenApi()` supports `AddDocumentTransformer` and `AddOperationTransformer` to inject security schemes and per-operation requirements. This integrates cleanly with the existing setup without additional packages.

**Alternatives considered**:
- Swashbuckle security filters — rejected (project uses .NET 10 built-in OpenAPI, not Swashbuckle for schema generation)
- Manual OpenAPI document editing — rejected (fragile, doesn't auto-update)

## Decision 6: Fail-Closed Default Authorization

**Decision**: Use a global authorization fallback policy so all endpoints require authentication by default, then explicitly opt out the TestController with `[AllowAnonymous]`.

**Rationale**: FR-007 requires fail-closed approach for all current and future endpoints. ASP.NET Core's `FallbackPolicy` applies to any endpoint without an explicit `[Authorize]` or `[AllowAnonymous]` attribute. This is the standard .NET pattern for fail-closed.

**Alternatives considered**:
- Per-controller [Authorize] attributes only — rejected (new controllers could accidentally skip auth)
- Global filter — rejected (FallbackPolicy is the recommended ASP.NET Core approach)

## Decision 7: Configuration Structure

**Decision**: Update `appsettings.json` to:
- Remove `Security:Authorization:Permissions` section (global permission actions)
- Remove `AuthorizationOverrides` from server group configurations
- Keep `Security:Authorization:Roles` and `Security:Authorization:RoleClaimMappings` (these are the configurable custom claims per FR-013/FR-014)

**Rationale**: The role claim mapping configuration already provides the "custom claim names configurable per deployment" requirement. Administrators configure which OIDC claims map to which application roles (reader, editor, admin) per provider.

## Decision 8: Controller Authorization Mapping

**Decision**: Apply authorization attributes as follows:

| Controller | Endpoints | Policy |
|-----------|-----------|--------|
| RedisKeysController | SearchKeys, GetKeyMetadata, GetKeyValue (GET) | Reader |
| RedisKeysController | DeleteKey (DELETE) | Editor |
| RedisServerGroupsController | GetServerGroups, GetServerGroupDetail (GET) | Reader |
| RedisInfoController | GetInfo (GET) | Reader |
| RedisStringsController | GetString (GET) | Reader |
| RedisStringsController | SetString (POST) | Editor |
| RedisHashesController | GetHashFields (GET) | Reader |
| RedisHashesController | SetHashFields, RemoveHashFields (POST) | Editor |
| RedisListsController | GetListRange (GET) | Reader |
| RedisListsController | PushToList, RemoveFromList (POST) | Editor |
| RedisSetsController | GetSetMembers (GET) | Reader |
| RedisSetsController | AddToSet, RemoveFromSet (POST) | Editor |
| RedisSortedSetsController | GetSortedSetRange (GET) | Reader |
| RedisSortedSetsController | AddToSortedSet, RemoveFromSortedSet (POST) | Editor |
| TestController | All (GET) | [AllowAnonymous] |
| ErrorController | All | [AllowAnonymous] |

**Rationale**: Direct mapping from HTTP method to policy. ErrorController also needs [AllowAnonymous] since it handles exception middleware redirects and must be accessible regardless of auth state.
