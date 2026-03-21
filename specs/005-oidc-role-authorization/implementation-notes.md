# Implementation Notes: 001-oidc-role-authorization

## Baseline

- Feature branch: `001-oidc-role-authorization`
- Scope: OIDC authn + role normalization + group-aware authorization
- Build status: `dotnet build` successful
- Test status: `246 passed / 0 failed`

## Test Convention Verification

- New tests MUST follow `MethodName_Condition_ExpectedBehavior` naming.
- Each test method MUST include: `// Arrange`, `// Act`, `// Assert`.

## FR-to-Task Traceability

| Requirement | Implementation Tasks | Test/Validation Tasks |
|---|---|---|
| FR-001 | T012, T018, T022, T023 | T016, T017 |
| FR-002 | T019, T020, T021 | T049 |
| FR-003 | T010, T011 | T050 |
| FR-004 | T001, T006, T009 | T042 |
| FR-005 | T006, T027 | T024 |
| FR-006 | T028, T029 | T025, T026 |
| FR-007 | T007, T037 | T033 |
| FR-008 | T007, T035, T036 | T032 |
| FR-009 | T013, T014, T035, T036 | T034 |
| FR-010 | T035, T036 | T033, T034 |
| FR-011 | T030, T031, T038 | T026 |
| FR-012 | T041 | T052 |
| FR-013 | T039 | T051 |

## Operational Validation

- Completed: mutation governance loop evidence captured in `mutation-report-notes.md`.

### T052: Non-Redeploy Operational Change Evidence

- Validation command:
```powershell
dotnet test tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj --filter "FullyQualifiedName~RoleMappingOperationalUpdateTests|FullyQualifiedName~GroupOverridePrecedenceTests.DeleteKey_WhenGroupOverrideUpdatedInConfiguration_AllowsEditorWithoutCodeChange"
```
- Result: `2 passed, 0 failed`.
- Evidence 1 (role mapping update): `RoleMappingOperationalUpdateTests.ReadEndpoint_WhenRoleClaimMappingUpdatedInConfiguration_AllowsWithoutCodeChange`
: Baseline with `groups=ops_readers` returned `403`, then config-only update (`Security:Authorization:RoleClaimMappings:0:AllowedValues:0=ops_readers`) returned `200`.
- Evidence 2 (group override update): `GroupOverridePrecedenceTests.DeleteKey_WhenGroupOverrideUpdatedInConfiguration_AllowsEditorWithoutCodeChange`
: Baseline `editor` delete in restricted group returned `403`, then config-only update (`Redis:ServerGroups:1:AuthorizationOverrides:Rules:0:AllowedRoles:1=editor`) returned `200`.

### T053: Explicit Test Convention Audit

- Audit command:
```powershell
$files = Get-ChildItem tests/Roman.RedisManager.Tests/Web/Authentication/*.cs, tests/Roman.RedisManager.Tests/Web/Authorization/*.cs
$results = foreach ($file in $files) {
	$content = Get-Content $file.FullName -Raw
	$factMethods = [regex]::Matches($content, '\[Fact\][\s\r\n]*public\s+(async\s+)?(Task|void)\s+([A-Za-z0-9_]+)\s*\(') | ForEach-Object { $_.Groups[3].Value }
	$namingViolations = @($factMethods | Where-Object { $_ -notmatch '^[A-Za-z0-9]+_[A-Za-z0-9]+_[A-Za-z0-9]+$' }).Count
	$hasArrange = $content -match '// Arrange'
	$hasAct = $content -match '// Act'
	$hasAssert = $content -match '// Assert'
	[pscustomobject]@{ File = $file.Name; FactTests = $factMethods.Count; NamingViolations = $namingViolations; AllTestsHaveAAA = ($hasArrange -and $hasAct -and $hasAssert) }
}
$results | Format-Table -AutoSize
```
- Result summary:
	- Files audited: `13`
	- `[Fact]` tests audited: `18`
	- Naming violations (`MethodName_Condition_ExpectedBehavior`): `0`
	- Files with `// Arrange`, `// Act`, `// Assert`: `13/13`

### T055: Authorization Decision Latency Evidence (p95 <= 1s)

- Validation command:
```powershell
dotnet test tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj --filter "FullyQualifiedName~GroupOverridePrecedenceTests.DeleteKey_AuthorizationDecisionLatency_OneHundredRequestsMeetP95UnderOneSecond" --logger "trx;LogFileName=authz-latency.trx"
```
- Result: `1 passed, 0 failed`.
- Artifact: `backend/tests/Roman.RedisManager.Tests/TestResults/authz-latency.trx`
- Recorded metric (`StdOut` in TRX): `Authorization decision latency validation: requests=100, p95Ms=0`
- Acceptance check: `p95Ms=0 <= 1000` (PASS).
