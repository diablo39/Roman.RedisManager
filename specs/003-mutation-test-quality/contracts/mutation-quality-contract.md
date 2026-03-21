# Contract: Mutation Quality Workflow

## Purpose

Defines the required interface between contributors/automation and the mutation-testing quality process for this repository.

## Trigger

- Any change that adds or modifies tests.
- Any AI-generated test contribution.
- Any quality-review request focused on test effectiveness.

## Required Inputs

- `branchName`: active branch under validation.
- `testScope`: test project or subset under mutation analysis.
- `baselineRef`: previous run reference (if available).
- `qualityThresholds`: configured high/low/break policy values.
- `exceptionTemplatePath`: path to approved non-actionable mutant exception template.
- `governanceScopePaths`: root and backend guidance/template paths that must pass durability checks.

## Required Actions (Ordered)

1. Execute mutation testing using `Stryker.NET` for selected scope.
2. Generate reports in HTML and JSON formats.
3. Produce comparison output against previous available run (or explicit baseline status).
4. Analyze surviving/no-coverage mutants for weak assertions and potential duplicate-test patterns.
5. Apply assertion hardening updates to tests.
6. Re-run mutation testing and repeat steps 2-5 until completion condition is met.
7. Record completion decision (`ThresholdMet`, `NoMoreActionableFindings`, or approved exception).
8. Run governance durability validation after guidance/template updates.

## Required Outputs

- HTML mutation report artifact path.
- JSON mutation report artifact path.
- Comparison report artifact path.
- Findings list (weak assertions, duplicate candidates, non-actionable mutants).
- Iteration summary with before/after mutation quality deltas.
- Non-actionable mutant exceptions log with required approval fields.
- Durability validation report for root `.specify` and `backend/.specify` guidance assets.

## Classification Rules

- **Potential duplicate test candidate**: flag when mutant outcome overlap with peer test is at least 80% and no unique assertion category is added.
- **Low-differentiation test candidate**: flag when a test contributes no unique mutant kill and no unique assertion intent versus peer tests in the same behavior area.

## Validation Rules

- Missing HTML or JSON report invalidates the run.
- Missing comparison output is only allowed for first-run baseline initialization with explicit baseline note.
- Completion is invalid when actionable surviving mutants remain and no exception is documented.
- Any accepted exception must include rationale and reviewer acknowledgement.
- Accepted non-actionable exceptions MUST include mutant id, location, justification, risk statement, mitigation note, reviewer, and review date.
- Durability validation MUST fail if required mutation-quality policy text is missing from either root-level or backend-level governance surfaces.

## Failure Semantics

- If mutation score is below `break` threshold, workflow returns failure status.
- If mutation execution is flaky, workflow returns partial status and requires re-run or quarantine decision.
- If reports cannot be parsed, workflow returns failure status and must be rerun after correction.

## Governance Mapping

- Copilot instruction scope: AI-generated tests must execute this contract.
- Speckit constitution scope: mutation-quality loop is a project quality gate for test changes.
- Template/prompt scope: generated specs/plans/tasks should include this contract path when test changes are in scope.
