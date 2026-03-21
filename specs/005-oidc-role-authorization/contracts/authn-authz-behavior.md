# Contract: Authentication and Authorization Behavior

## Scope

Defines externally observable HTTP behavior for secured endpoints after OIDC authentication and role/group authorization policy enforcement.

## Authentication Contract

- Bearer-token-authenticated principal is accepted for protected endpoints when token is valid.
- Missing, unsupported, or invalid bearer token results in unauthenticated response.

Expected status outcomes:
- `401 Unauthorized` when authentication fails or is missing.
- `403 Forbidden` when authentication succeeds but authorization fails.

## Normalized Role Contract

- Internal application roles are the only roles used by endpoint policy checks.
- Provider-specific claims (`groups`, `roles`, custom claim keys) are transformed into internal roles before policy checks.
- `[Authorize(Roles = "<internal-role>")]` semantics are stable regardless of provider.

## Group-Aware Authorization Contract

For group-scoped operations, policy evaluation uses:
- normalized internal roles
- requested operation category
- target Redis server group identifier

Decision precedence:
1. Apply group override if present for group/action.
2. Else apply global permission rule.
3. Else deny.

## Endpoint Behavior Examples

## Example A: Authenticated and allowed by global rule

- Input: principal normalized to `editor`, action `DeleteKey`, no group override.
- Result: `2xx` response (endpoint-specific success code).

## Example B: Authenticated but denied by group override

- Input: principal normalized to `editor`, action `DeleteKey`, group `prod-cache` override allows only `admin`.
- Result: `403 Forbidden`.

## Example C: Unmapped claims

- Input: valid token but no claim mapping to internal roles.
- Result: `403 Forbidden` for role-protected endpoints.

## Auditing Contract

Each authorization decision should emit structured diagnostics containing:
- user identity reference
- provider key
- normalized roles
- action name
- group id (when present)
- decision and reason code

## Non-Goals

- No provider-specific authorization logic in domain entities.
- No endpoint-specific hardcoded external claim parsing.
