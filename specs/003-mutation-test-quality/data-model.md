# Data Model - Mutation Testing Quality Improvements

## Entity: MutationTestRun

- Description: A single execution of mutation testing against the test suite.
- Fields:
- `runId` (string): Unique run identifier.
- `startedAtUtc` (datetime): Start timestamp.
- `completedAtUtc` (datetime): Completion timestamp.
- `branchName` (string): Source branch under test.
- `baselineReference` (string, nullable): Prior baseline identifier used for comparison.
- `mutationScore` (decimal): Current run mutation score.
- `status` (enum): `Succeeded`, `Failed`, `Partial`.
- Validation:
- `completedAtUtc` must be greater than or equal to `startedAtUtc`.
- `mutationScore` must be in range `0..100` when status is `Succeeded`.

## Entity: MutationReportSet

- Description: Report artifacts generated from a mutation run.
- Fields:
- `runId` (string): Foreign key to `MutationTestRun`.
- `htmlReportPath` (string): Path to HTML report.
- `jsonReportPath` (string): Path to JSON report.
- `comparisonReportPath` (string): Path to previous-run comparison output.
- `generatedAtUtc` (datetime): Artifact generation time.
- Validation:
- All report paths must exist for successful runs.
- `comparisonReportPath` may point to a baseline notice artifact on first run.

## Entity: MutationFinding

- Description: Actionable analysis extracted from mutation output.
- Fields:
- `findingId` (string): Unique identifier.
- `runId` (string): Associated mutation run.
- `category` (enum): `SurvivedMutant`, `NoCoverage`, `PotentialDuplicateTest`, `FlakyOutcome`, `NonActionable`.
- `location` (string): Code/test location reference.
- `severity` (enum): `High`, `Medium`, `Low`.
- `recommendation` (string): Suggested assertion or test-structure improvement.
- `resolutionState` (enum): `Open`, `Addressed`, `AcceptedException`.
- Validation:
- `recommendation` is required for `SurvivedMutant` and `PotentialDuplicateTest`.
- `AcceptedException` requires rationale note.

## Entity: AssertionImprovementCycle

- Description: One iteration of mutation-driven test hardening.
- Fields:
- `cycleId` (string): Unique iteration ID.
- `runIdBefore` (string): Baseline run for iteration.
- `changesetReference` (string): Commit or working-change reference.
- `assertionsChanged` (integer): Number of assertions added/strengthened.
- `runIdAfter` (string): Follow-up run ID.
- `deltaSurvivedMutants` (integer): Numeric difference after iteration.
- `completionReason` (enum): `ThresholdMet`, `NoMoreActionableFindings`, `ExceptionApproved`.
- Validation:
- `runIdBefore` and `runIdAfter` must differ.
- `completionReason` must be present when cycle is closed.

## Entity: AITestingGovernanceRule

- Description: Persistent policy statement that governs AI-driven test creation and validation.
- Fields:
- `ruleId` (string): Unique policy identifier.
- `scope` (enum): `CopilotInstruction`, `Constitution`, `Template`, `Prompt`.
- `policyText` (string): Normative requirement text.
- `enforcementTrigger` (string): Event that activates rule (for example, "new tests created").
- `requiredActions` (string[]): Ordered actions AI must perform.
- `violationHandling` (string): Required remediation loop.
- Validation:
- `requiredActions` must include mutation run and iterative re-run steps.
- `policyText` must be normative (`MUST`/`REQUIRED`).

## Relationships

- `MutationTestRun` 1-to-1 `MutationReportSet`.
- `MutationTestRun` 1-to-many `MutationFinding`.
- `AssertionImprovementCycle` references two `MutationTestRun` instances (`before` and `after`).
- `AITestingGovernanceRule` governs creation/update behavior for tests and cycles but is stored in documentation/config artifacts.

## State Transitions

- `MutationTestRun.status`: `Partial` -> `Succeeded` or `Failed`.
- `MutationFinding.resolutionState`: `Open` -> `Addressed` or `AcceptedException`.
- `AssertionImprovementCycle.completionReason`: unset -> terminal reason at closure.
