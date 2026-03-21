# Data Model: Endpoint Authorisation Rules

**Date**: 2026-03-15 | **Branch**: `003-endpoint-auth-rules`

## Entities

### Authorization Policy (conceptual)

This feature does not introduce new persisted entities. The authorization model is configuration-driven.

| Concept | Description | Values |
|---------|-------------|--------|
| Policy Name | Named authorization policy applied to endpoints | `Reader`, `Editor` |
| Role | Application role mapped from OIDC claims | `reader`, `editor`, `admin` |
| Claim Mapping | Maps OIDC provider claims to application roles | Configured per deployment |

### Policy-to-Role Mapping

| Policy | Grants Access To Roles |
|--------|----------------------|
| Reader | `reader`, `admin` |
| Editor | `editor`, `admin` |

### Policy-to-HTTP-Method Mapping

| HTTP Method | Required Policy |
|-------------|----------------|
| GET | Reader |
| POST | Editor |
| PUT | Editor |
| DELETE | Editor |
| PATCH | Editor |

## Configuration Entities (existing, modified)

### AuthorizationRoleMappingConfiguration (retained)

| Field | Type | Description |
|-------|------|-------------|
| Roles | Collection | Available application roles with descriptions |
| RoleClaimMappings | Collection | Per-provider claim-to-role mappings |

### RoleClaimMapping (retained)

| Field | Type | Description |
|-------|------|-------------|
| RoleName | string (required) | Application role name (reader, editor, admin) |
| ProviderKey | string (required) | OIDC provider identifier |
| ClaimKey | string (required) | Claim name to inspect in token |
| AllowedValues | string[] (required) | Values that grant the role |
| MatchMode | enum | Any (default) or All |

## Entities Removed

| Entity | Reason |
|--------|--------|
| PermissionAction enum | Replaced by simple Reader/Editor policies |
| GroupPermissionRequirement | Group-based authorization removed |
| AuthorizationPermissionConfiguration | Global permission rules replaced by role-based policies |
| AuthorizationOverrides (in RedisServerGroupConfiguration) | Group-level overrides removed |

## State Transitions

No state transitions — authorization is stateless and evaluated per-request from token claims and configuration.
