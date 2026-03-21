# Final Mutation Summary

## Baseline Run

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1`
- Mutation score: 56.00
- Report paths:
	- `backend/tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.html`
	- `backend/tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json`

## Comparison Run

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/compare-mutation-reports.ps1 -CurrentReportPath tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json -PreviousReportPath tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.previous.json -OutputPath tests/Roman.RedisManager.Tests/StrykerOutput/mutation-comparison.md`
- Mutation score: 56.00
- Delta: 0.00
- Status: stable

## Findings

- Survived mutants: 44
- No-coverage mutants: 0
- Exceptions logged: 0
