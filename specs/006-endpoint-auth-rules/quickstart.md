# Quickstart: Endpoint Authorisation Rules

**Date**: 2026-03-15 | **Branch**: `003-endpoint-auth-rules`

## Overview

This feature replaces the existing granular group-based authorization system with two simple role-based policies:
- **Reader**: Required for all GET endpoints (except TestController)
- **Editor**: Required for all POST/PUT/DELETE/PATCH endpoints (except TestController)

## What Changes

### Files to Modify
1. **Program.cs** — Replace policy definitions, add fallback policy, remove group-based service registrations
2. **AuthorizationPolicies.cs** — Replace `ReadKeys`/`DeleteKeysByGroup` with `Reader`/`Editor`
3. **All controllers** — Update `[Authorize]` attributes to use new policy names
4. **TestController.cs** — Add `[AllowAnonymous]`
5. **ErrorController.cs** — Add `[AllowAnonymous]`
6. **appsettings.json** — Remove `Permissions` section and `AuthorizationOverrides`
7. **OpenAPI configuration** — Add security scheme and per-endpoint requirements

### Files to Remove
1. `Authorization/Requirements/GroupPermissionRequirement.cs`
2. `Authorization/Handlers/GroupPermissionAuthorizationHandler.cs`
3. `Authorization/IGroupContextAccessor.cs`
4. `Authorization/RouteGroupContextAccessor.cs`
5. `Domain/Entities/AuthorizationPolicyTypes.cs`
6. `Domain/Configuration/AuthorizationPermissionConfiguration.cs`

### Files to Retain (with modifications)
1. `Authorization/AuthorizationDecisionLogger.cs` — Simplify for reader/editor logging
2. `Authorization/NormalizedRoleClaimsTransformation.cs` — No changes needed
3. `Authorization/RoleClaimMappingEvaluator.cs` — No changes needed

## Configuration

Authorization is configured via `appsettings.json` under `Security:Authorization`. The role claim mapping system is unchanged — administrators map OIDC provider claims to application roles (`reader`, `editor`, `admin`).

## Verification

After implementation:
1. Unauthenticated requests to any protected endpoint → 401
2. Authenticated user with `reader` role → can access GET endpoints, gets 403 on mutations
3. Authenticated user with `editor` role → can access mutation endpoints, gets 403 on GET
4. Authenticated user with both roles → full access
5. TestController → accessible without authentication
6. OpenAPI document at `/openapi/v1.json` → includes security scheme and per-endpoint requirements
