# Phase 0 Research - Mutation Testing Quality Improvements

## Decision 1: Mutation framework selection

- Decision: Use `Stryker.NET` (`dotnet-stryker`) as the mutation testing tool for this .NET repository.
- Rationale: It is the native mutation testing solution for .NET test projects, integrates with xUnit workflows, and supports policy-based quality thresholds.
- Alternatives considered: Generic mutation tools outside the .NET ecosystem were rejected due to weaker integration and higher setup friction.

## Decision 2: Report outputs per run

- Decision: Generate `html` and `json` reports on every mutation run; optionally include `markdown` for PR summarization.
- Rationale: HTML is human-readable for triage; JSON is machine-readable for automation and comparison workflows.
- Alternatives considered: Console-only output was rejected because it is not durable for historical comparison.

## Decision 3: Previous-run comparison strategy

- Decision: Use baseline comparison with Stryker's `with-baseline` workflow and disk-backed baseline storage for local/CI continuity.
- Rationale: Baseline mode enables current-vs-previous comparison while reducing PR runtime by reusing unchanged mutant results.
- Alternatives considered: Full-run-only comparisons were rejected because they increase runtime and reduce practicality for iterative loops.

## Decision 4: Weak and duplicate test signal extraction

- Decision: Treat surviving mutants, no-coverage mutants, and repeated surviving mutant clusters as primary indicators of weak assertions or duplicated test intent.
- Rationale: These signals directly map to tests that do not fail under behavioral perturbation or provide redundant coverage with low differentiation.
- Alternatives considered: Line coverage percentage alone was rejected because high coverage can still hide weak assertions.

## Decision 5: Iterative assertion hardening loop

- Decision: Enforce an iterative cycle: run mutation tests -> analyze survivors -> strengthen assertions -> rerun until quality threshold or documented exception.
- Rationale: This directly operationalizes the spec requirement to decrease surviving mutants through incremental test improvements.
- Alternatives considered: One-pass mutation checks were rejected because they do not guarantee assertion strengthening before completion.

## Decision 6: Initial quality gate policy

- Decision: Start with progressive thresholds (`high`, `low`, `break`) appropriate for brownfield adoption and tighten over time.
- Rationale: Progressive thresholds prevent delivery blockage while still preventing quality regression.
- Alternatives considered: Immediate strict threshold enforcement was rejected due to adoption risk in an existing codebase.

## Decision 7: Flaky and non-actionable mutant handling

- Decision: Add explicit handling guidance for timeouts/flaky outcomes and require documented rationale for accepted non-actionable survivors.
- Rationale: This keeps mutation metrics trustworthy while allowing pragmatic exceptions.
- Alternatives considered: Ignoring flaky behavior was rejected because it creates unstable quality signals.

## Decision 8: Durable AI governance placement

- Decision: Persist mutation-testing requirements in both runtime guidance (`backend/.github/copilot-instructions.md`) and framework-level governance (`backend/.specify/memory/constitution.md` and relevant templates/prompts).
- Rationale: Dual placement prevents behavior drift when one artifact is refreshed or regenerated.
- Alternatives considered: Updating only one guidance file was rejected because it is vulnerable to future overwrite.
