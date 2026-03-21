# Mutation Exceptions

**Date**: 2026-03-16

## Tooling Exception: Stryker.NET Incompatibility

**Stryker.NET 4.13.0** (latest as of 2026-03-16) cannot compile mutated assemblies for this project due to .NET 10's `Microsoft.AspNetCore.OpenApi.SourceGenerators` emitting interceptor-based code (`CS9137`). This is a known limitation — Stryker does not yet support the `InterceptorsNamespaces` MSBuild feature used by .NET 10's built-in OpenAPI source generators.

**Mitigation**: Manual mutation analysis performed. Results documented in `mutation-analysis.md`.

## Approved Non-Actionable Surviving Mutants

### 1. NormalizedRoleClaimsTransformation — Idempotency Guard

**Location**: `!principal.IsInRole(role)` check before `identity.AddClaim()`
**Mutation**: Remove condition — always add claim
**Reason**: Removing this guard adds duplicate `ClaimTypes.Role` claims but `IsInRole()` still returns the same result. The guard is defensive (prevents claim bloat on repeated transformations), not behavioral logic. No observable difference in authorization outcomes.

### 2. CustomAuthorizationResultHandler — Middleware Integration

**Location**: Entire class
**Mutation**: Various (condition removal, status code change, string mutations)
**Reason**: This `IAuthorizationMiddlewareResultHandler` requires the full ASP.NET Core authorization middleware pipeline to execute. Unit testing would require constructing `PolicyAuthorizationResult` with internal `AuthorizationFailure` state, which is not publicly constructable. The class is verified through integration tests that confirm:
- 401 for unauthenticated requests (UnmappedClaimsAuthorizationTests)
- 403 for authenticated users without required role (RoleMappingOperationalUpdateTests)
- Correct endpoint protection (GlobalPermissionFallbackTests)

### 3. OpenApi Transformers — Integration-Verified

**Location**: `SecuritySchemeTransformer.cs`, `SecurityRequirementOperationTransformer.cs`
**Mutation**: Various
**Reason**: These implement OpenAPI document/operation transformer interfaces. Verified by T034 (manual inspection of `/openapi/v1.json` output confirming `securitySchemes` and per-endpoint `security` arrays). Direct unit testing would require mocking OpenAPI internal types (`OpenApiDocument` workspace, `OpenApiOperationTransformerContext`).
