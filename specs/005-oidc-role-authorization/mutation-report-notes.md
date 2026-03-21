# Mutation Report Notes: 001-oidc-role-authorization

## Run 1

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1`
- HTML report: `tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.html`
- JSON report: `tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json`
- Final score (rerun): `59.26%`

## Analysis

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/analyze-surviving-mutants.ps1`
- Findings markdown: `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.md`
- Findings json: `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.json`
- Actionable survivors: present (predominantly in existing `Application/CQRS` handlers outside this feature's direct modification set)
- No-coverage mutants: included in findings output when present

## Improvements

- Assertion updates applied to:
- `tests/Roman.RedisManager.Tests/Web/Authorization/GroupOverridePrecedenceTests.cs`
- `tests/Roman.RedisManager.Tests/Web/Authorization/RoleClaimMappingEvaluatorTests.cs`
- Mapped findings:
- Strengthened deny/allow assertions for group override behavior (`T045`).
- Added provider mismatch negative assertion and stricter role count assertion (`T046`).

## Re-run

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1`
- Compare command: `pwsh -NoProfile -File .specify/scripts/powershell/compare-mutation-reports.ps1`
- Comparison report: `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-comparison.md`
- Score delta: `+3.26` (`56.00` -> `59.26`, status `improved`)
- Approved exceptions: survivors remaining in broad existing `Application/CQRS` files are recorded as out-of-scope for this feature implementation increment; follow-up hardening should be tracked in a dedicated mutation-improvement change.
