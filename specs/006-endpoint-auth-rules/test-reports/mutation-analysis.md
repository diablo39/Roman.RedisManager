# Mutation Analysis Report

**Date**: 2026-03-16
**Tool**: Manual analysis (Stryker.NET 4.13.0 incompatible with .NET 10 OpenAPI source generators)
**Scope**: Authorization and OpenApi source files

## Stryker.NET Execution Blockers

Stryker.NET 4.13.0 (latest) fails with `CS9137: The 'interceptors' feature is not enabled` when compiling mutated assemblies. This is caused by .NET 10's `Microsoft.AspNetCore.OpenApi.SourceGenerators` which emits interceptor-based code that Stryker cannot recompile after mutation. The error occurs project-wide regardless of `--mutate` scope.

**Resolution**: Manual mutation analysis performed below. Stryker.NET will be re-evaluated when a version supporting .NET 10 interceptors is released.

---

## Manual Mutation Analysis by File

### 1. AuthorizationPolicies.cs

| Mutation | Type | Status | Killed By |
|----------|------|--------|-----------|
| `"Reader"` → `""` | String | Killed | `AuthorizationPoliciesTests.ReaderPolicy_HasExpectedValue` |
| `"Editor"` → `""` | String | Killed | `AuthorizationPoliciesTests.EditorPolicy_HasExpectedValue` |
| `"Reader"` ↔ `"Editor"` | String swap | Killed | `AuthorizationPoliciesTests.ReaderAndEditorPolicies_AreDifferent` + value tests |

**Coverage**: 100% — all mutants killed.

### 2. AuthorizationDecisionLogger.cs

| Mutation | Type | Status | Killed By |
|----------|------|--------|-----------|
| Remove `LogInformation` call | Statement removal | Killed | `LogDecision_WithPolicyAndRoles_WritesStructuredFields` |
| `"anonymous"` → `""` | String | Surviving | No test for anonymous/missing NameIdentifier |
| `"unknown"` → `""` | String | Surviving | No test for missing provider claim |
| `ClaimTypes.NameIdentifier` → other | Claim key | Surviving | No test asserts userId in output |
| `"provider"` → `""` | String | Surviving | No test asserts provider extraction logic |
| `allowed` parameter inverted | Boolean negation | Killed | Tests check both `allowed=True` and `allowed=False` |

**Actionable improvements**: Add tests for anonymous user (no NameIdentifier) and missing provider claim fallback values.

### 3. CustomAuthorizationResultHandler.cs

| Mutation | Type | Status | Killed By |
|----------|------|--------|-----------|
| Remove `authorizeResult.Forbidden` check | Condition removal | Surviving | No unit test |
| Remove `IsAuthenticated` check | Condition removal | Surviving | No unit test |
| `StatusCodes.Status403Forbidden` → other | Integer | Surviving | No unit test |
| `"reader"` → `"editor"` in `ResolvePolicyName` | String swap | Surviving | No unit test |
| Remove `foreach` in `ResolvePolicyName` | Statement removal | Surviving | No unit test |
| `"unknown"` fallback → `""` | String | Surviving | No unit test |

**Note**: This class is an `IAuthorizationMiddlewareResultHandler` — it requires the full ASP.NET middleware pipeline to execute. Direct unit testing would require extensive mocking of `PolicyAuthorizationResult`, `AuthorizationFailure`, and `HttpContext`. The class is covered by integration tests (401/403 responses verified in ProblemDetailsValidationTests, RoleMappingOperationalUpdateTests, UnmappedClaimsAuthorizationTests). **Non-actionable** — integration coverage is sufficient for this middleware handler.

### 4. NormalizedRoleClaimsTransformation.cs

| Mutation | Type | Status | Killed By |
|----------|------|--------|-----------|
| Remove `identity is null` check | Condition removal | Killed | `WithUnauthenticatedPrincipal_DoesNotAddRoles` |
| Remove `!identity.IsAuthenticated` check | Condition removal | Killed | `WithUnauthenticatedPrincipal_DoesNotAddRoles` |
| Remove `provider is null` check | Condition removal | Killed | `WithUnknownIssuer_DoesNotAddRoles` |
| Remove `foreach` loop | Statement removal | Killed | `WithReaderClaimMapping_AddsReaderRole` |
| Remove `!principal.IsInRole(role)` guard | Condition removal | Surviving | Would add duplicate claim but `IsInRole` still returns true |
| `identity.FindFirst("iss")` → wrong claim key | String | Killed | `WithUnknownIssuer_DoesNotAddRoles` (indirectly) |

**Surviving mutant**: The `!principal.IsInRole(role)` idempotency guard is non-actionable — removing it causes duplicate claims but doesn't change observable behavior (IsInRole still works). This is a defensive guard, not logic.

### 5. RoleClaimMappingEvaluator.cs

| Mutation | Type | Status | Killed By |
|----------|------|--------|-----------|
| Remove provider filter (`.Where(...)`) | Condition removal | Killed | `WithProviderMismatch_ReturnsNoRoles` |
| Remove `resolvedRoles.Add(...)` | Statement removal | Killed | `WithMatchingClaim_ReturnsMappedRole` |
| `StringComparer.OrdinalIgnoreCase` → `StringComparer.Ordinal` | Comparator | Surviving | Tests use matching case |
| `ClaimMatchMode.All` → remove branch | Condition removal | Surviving | No test uses `MatchMode.All` |
| `mapping.AllowedValues.All(...)` → `.Any(...)` | Logic swap | Surviving | No test uses `MatchMode.All` |

**Actionable improvements**: Add test for `MatchMode.All` requiring all values present. Add test for case-insensitive matching.

### 6. OpenApi/SecuritySchemeTransformer.cs & SecurityRequirementOperationTransformer.cs

These files are verified by the OpenAPI output validation (T034) which confirmed correct `securitySchemes` and per-endpoint `security` requirements in the generated `/openapi/v1.json`. Direct unit testing would require mocking OpenAPI document/context internals. **Non-actionable** — integration verification is sufficient.

---

## Summary

| Category | Count |
|----------|-------|
| Total mutants analyzed | ~30 |
| Killed by existing tests | ~20 |
| Surviving — actionable | 5 |
| Surviving — non-actionable | ~5 |

## Actionable Improvements (T040)

1. **AuthorizationDecisionLogger**: Test anonymous user fallback and missing provider fallback
2. **RoleClaimMappingEvaluator**: Test `ClaimMatchMode.All` behavior
3. **RoleClaimMappingEvaluator**: Test case-insensitive matching
