# Data Model: OIDC AuthN/AuthZ Configuration

## Entity: OidcProviderDefinition

- Purpose: Defines one external identity provider configuration.
- Fields:
- `ProviderKey` (string, required): Internal unique identifier (for example `entra`, `google`, `generic`).
- `DisplayName` (string, required): Human-readable provider name.
- `Authority` (string, required): OIDC authority/issuer base URL.
- `ClientId` (string, required): Client identifier for the app.
- `ClientSecret` (string, optional): Secret when required by provider flow.
- `MetadataAddress` (string, optional): Explicit OIDC metadata endpoint override.
- `Enabled` (bool, required): Provider activation flag.
- Validation rules:
- `ProviderKey` must be unique across configured providers.
- At least one provider should be enabled for authentication to be operational.
- `Authority` must be absolute URI.

## Entity: ApplicationRoleDefinition

- Purpose: Defines an internal authorization role name used by policies.
- Fields:
- `RoleName` (string, required): Internal role label used by `[Authorize(Roles = ...)]`.
- `Description` (string, optional): Human-readable summary.
- Validation rules:
- `RoleName` unique and non-empty.
- Role names should be stable to avoid policy drift.

## Entity: RoleClaimMappingRule

- Purpose: Maps external provider claim values to one internal role.
- Fields:
- `RoleName` (string, required): Target internal role.
- `ProviderKey` (string, required): Provider this rule applies to.
- `ClaimKey` (string, required): External claim type/key (for example `groups`, `roles`, `custom_group`).
- `AllowedValues` (IEnumerable<string>, required): Values that satisfy mapping.
- `MatchMode` (enum, required): Matching semantics (`Any`, `All`).
- Validation rules:
- `RoleName` must reference existing `ApplicationRoleDefinition`.
- `ProviderKey` must reference existing `OidcProviderDefinition`.
- `AllowedValues` must be non-empty.

## Entity: PermissionAction

- Purpose: Canonical protected operation category.
- Values (initial):
- `ReadKeys`
- `ReadMetadata`
- `ReadValues`
- `DeleteKey`
- `WriteKey` (future-proof placeholder if write endpoints exist)
- Validation rules:
- Values must map to concrete API operation points.

## Entity: GlobalPermissionRule

- Purpose: Default role allow-list for each protected action.
- Fields:
- `Action` (PermissionAction, required)
- `AllowedRoles` (IEnumerable<string>, required)
- Validation rules:
- `AllowedRoles` entries must exist in `ApplicationRoleDefinition`.

## Entity: GroupPermissionOverride

- Purpose: Group-specific permission override replacing or narrowing default permissions.
- Fields:
- `ServerGroupId` (Guid, required): Target Redis server group (inherited from containing `RedisServerGroupConfiguration`).
- `ActionRules` (IEnumerable<PermissionActionRule>, required): Override rules by action.
- `Mode` (enum, required): `ReplaceDefaults` or `NarrowDefaults`.
- Validation rules:
- Override must be defined inside the target server group configuration.
- Duplicate action entries within same `GroupId` are invalid.

## Entity: PermissionActionRule

- Purpose: Role allow-list for one action in an override context.
- Fields:
- `Action` (PermissionAction, required)
- `AllowedRoles` (IEnumerable<string>, required)
- Validation rules:
- Role names must exist in `ApplicationRoleDefinition`.

## Entity: AuthorizationDecisionContext

- Purpose: Runtime evaluation context used by authorization handlers and audit logging.
- Fields:
- `SubjectId` (string, required): User identifier claim value.
- `ProviderKey` (string, required): Resolved identity provider.
- `NormalizedRoles` (IReadOnlyCollection<string>, required)
- `Action` (PermissionAction, required)
- `GroupId` (Guid?, optional): Null for non-group-scoped actions.
- `Decision` (enum, required): `Allow` or `Deny`.
- `DecisionReason` (string, required): Deterministic reason code.

## Relationships

- `ApplicationRoleDefinition` 1..* <- `RoleClaimMappingRule`.
- `OidcProviderDefinition` 1..* <- `RoleClaimMappingRule`.
- `GlobalPermissionRule` references `ApplicationRoleDefinition`.
- `RedisServerGroupConfiguration` 0..1 -> `GroupPermissionOverride` 1..* <- `PermissionActionRule`.
- `AuthorizationDecisionContext` consumes normalized output from `RoleClaimMappingRule` evaluation and permission rules.

## State Transitions

- Principal role state:
- `Authenticated + UnmappedClaims` -> `Authenticated + NormalizedRoles` when at least one rule matches.
- `Authenticated + UnmappedClaims` -> `Authenticated + NoRoles` when no rules match.
- Authorization state:
- `RequestReceived` -> `PolicyContextBuilt` -> `RuleEvaluated` -> `Allow|Deny`.
- Configuration state:
- `ConfigLoaded` -> `ConfigValidated` -> `ActivePolicySet`.
- Invalid configuration transitions to `StartupValidationFailure`.
