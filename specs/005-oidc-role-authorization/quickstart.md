# Quickstart: OIDC AuthN/AuthZ Configuration

Detailed provider setup instructions are documented in `../specs/001-oidc-role-authorization/identity-configuration.md`.

## 1. Configure Providers

1. Open `backend/src/Roman.RedisManager.Web/appsettings.json`.
2. Add authentication settings with provider entries for:
- `entra` (EntraID profile)
- `google` (Google profile)
- `generic` (generic OIDC profile)
3. Ensure each provider has unique `ProviderKey` and is enabled.

## 2. Configure Internal Roles and Claim Mappings

1. Define application roles (for example `redis-reader`, `editor`, `admin`).
2. For each role, add provider-specific claim mapping rules:
- claim key
- accepted claim values
- matching mode
3. Verify at least one mapping exists for each role intended to authorize protected actions.

## 3. Configure Global Permissions and Group Overrides

1. Define global permission rules for protected actions.
2. For each target group, add `AuthorizationOverrides` under `Redis:ServerGroups` where stricter access is required.
3. Set override mode per group (`ReplaceDefaults` or `NarrowDefaults`).

## 4. Build and Run

1. Run build from `backend`:
```powershell
 dotnet build
```
2. Start API:
```powershell
 dotnet run --project src/Roman.RedisManager.Web
```

## 5. Validate Authentication Flow

1. Obtain a bearer token from a configured provider.
2. Call a protected endpoint with `Authorization: Bearer <token>` and confirm authenticated response.
3. Call the same endpoint without the header and confirm `401 Unauthorized`.

## 6. Validate Authorization Normalization and Overrides

1. Test users/tokens that map to `redis-reader`, `editor`, and `admin`.
2. Verify `[Authorize(Roles = ...)]` behavior is identical across different providers for the same normalized role.
3. Call group-scoped endpoints with different `groupId` values:
- confirm global allow behavior where no override exists
- confirm override deny/allow behavior for configured restricted groups

## 7. Validate Non-Redeploy Operational Changes

1. Role mapping update validation (no code changes):
- Baseline expectation: principal with claim `groups=ops_readers` is denied on `GET /api/redis-server-groups` (`403`) with default mapping.
- Operational update: change `Security:Authorization:RoleClaimMappings:0:AllowedValues:0` to `ops_readers` in configuration.
- Post-change expectation: same principal is allowed (`200`) without recompiling/redeploying binaries.
2. Group override update validation (no code changes):
- Baseline expectation: `editor` role is denied deleting in restricted group `33333333-3333-3333-3333-333333333333` (`403`).
- Operational update: add `editor` to `Redis:ServerGroups:1:AuthorizationOverrides:Rules:0:AllowedRoles`.
- Post-change expectation: same request succeeds (`200`) without code changes.

## 8. Validate Authorization Decision Latency (p95 <= 1s)

1. Execute:
```powershell
dotnet test tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj --filter "FullyQualifiedName~GroupOverridePrecedenceTests.DeleteKey_AuthorizationDecisionLatency_OneHundredRequestsMeetP95UnderOneSecond" --logger "trx;LogFileName=authz-latency.trx"
```
2. Inspect `tests/Roman.RedisManager.Tests/TestResults/authz-latency.trx` and confirm output line:
- `Authorization decision latency validation: requests=100, p95Ms=<value>`
3. Accept only if `p95Ms <= 1000`.

## 9. Run Test and Mutation Quality Loop

1. Run tests:
```powershell
 dotnet test
```
2. Run mutation loop:
```powershell
 pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1
 pwsh -NoProfile -File .specify/scripts/powershell/analyze-surviving-mutants.ps1
 # apply assertion improvements
 pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1
 pwsh -NoProfile -File .specify/scripts/powershell/compare-mutation-reports.ps1
```
3. Record HTML/JSON report paths and survivor analysis in implementation notes.
