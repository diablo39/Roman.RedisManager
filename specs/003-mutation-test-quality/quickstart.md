# Quickstart - Mutation Testing Quality Improvements

## Prerequisites

1. .NET 10 SDK installed.
2. Repository dependencies restored and `dotnet test` passing.
3. `dotnet-stryker` available via local tool manifest (`dotnet tool restore`) or installed locally.

## Script entrypoints and path governance

1. Run operational mutation scripts from `backend/.specify/scripts/powershell/`.
2. Treat `backend/.specify/` as the runtime workflow surface for this repository.
3. Treat root `.specify/` as durability source for shared templates that can be copied or regenerated.
4. Keep mutation governance text synchronized across both surfaces when policy updates are made.

## 1. Run baseline mutation test

1. From the `backend/` directory, execute:

```powershell
pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1
```

2. Use reporters `html` and `json` (plus optional `markdown` for PR comments).
3. Confirm artifacts exist under `StrykerOutput/`.

Expected result:
- HTML report generated.
- JSON report generated.
- Baseline state initialized if this is the first run.

## 2. Run comparison against previous results

1. Run mutation testing with baseline enabled:

```powershell
pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1 -WithBaseline
```

2. Use local disk baseline for local development and CI baseline continuity.
3. Compare reports:

```powershell
pwsh -NoProfile -File .specify/scripts/powershell/compare-mutation-reports.ps1 -CurrentReportPath "tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json"
```

4. Verify comparison output indicates improved/regressed/stable mutation quality.

Expected result:
- Comparison output available for current run versus previous baseline.
- If no previous baseline exists, comparison output contains explicit baseline notice.

## 3. Analyze weak or duplicated tests

1. Generate analysis output:

```powershell
pwsh -NoProfile -File .specify/scripts/powershell/analyze-surviving-mutants.ps1 -MutationReportPath "tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json"
```

2. Review surviving and no-coverage mutants in reports.
3. Identify likely weak assertions and low-differentiation/duplicate tests.
4. Create actionable finding list before modifying tests.

Expected result:
- Findings include test location, reason, and recommended assertion improvement.

## 4. Perform iterative assertion hardening

1. Strengthen assertions for prioritized findings.
2. Re-run mutation testing.
3. Compare updated result with prior run.
4. Repeat until threshold reached or approved exception is documented.

Expected result:
- Surviving mutants decrease across iterations for modified behavior.
- Completion reason documented for each iteration cycle.

## 5. Validate governance enforcement

1. Verify updated Copilot and Speckit governance artifacts include mutation-quality requirements.
2. Trigger a sample AI test-generation workflow.
3. Confirm workflow includes run -> analyze -> improve -> rerun loop before completion.

Expected result:
- New AI-authored test changes explicitly include mutation quality validation steps.
