# Quality Helpers Overview

This directory contains **test‑project helper classes and corresponding unit tests**
that mirror the behaviour of the mutation‑testing orchestration scripts.

The production workflow lives in PowerShell scripts located at:

```
backend/.specify/scripts/powershell/
```

Those scripts do not reference any of the types defined under this folder. The
existence of nearly identical logic in both places is intentional:

* the scripts are what developers and CI actually execute when running
  mutation testing;
* the C# helpers allow us to write fast, deterministic `dotnet test` facts that
  validate the same algorithms without having to shell out or parse script
  output.

### Responsibilities

| File | Description |
|------|-------------|
| `MutationRunSettings` | Configuration used by argument builder and tests. |
| `MutationCliArguments` | Builds cli argument lists identical to `run-mutation-tests.ps1`. |
| `MutationReportSetValidator` | Ensures required report files exist after a run. |
| `MutationFinding` | Data model for classified mutants. |
| `MutationFindingClassifier` | Converts raw mutants into actionable findings. |
| `DuplicateTestHeuristicAnalyzer` | Flags potential duplicate/low-differentiation tests. |
| `AssertionImprovementCycleState` and related tests | Tracks the run‑analyze‑rerun completion cycle. |
| `GovernanceMutationPolicyTests` | Verifies that required governance phrases are present. |
| \\* `*Tests` files | Unit tests exercising the above helpers. |

### Maintenance guidance

* **When you update a script**: find the corresponding helper/test in this
  folder and update it to match. Add a new helper/test pair if you add new
  behaviour.
* **When you modify a helper/test**: consider whether the scripts need the same
  change; keep the two implementations in sync.
* **Avoid running these helpers in production code** – they live in the test
  project only. The scripts are the canonical runtime implementation.
* Refer back to this `README.md` (and the header comments in each file) if
  you're unsure why both implementations exist.

By keeping both sides documented and tested, we get the best of both worlds:
reliable, maintainable test coverage for our mutation workflow, and simple
shell‑based entrypoints for developers and CI.
