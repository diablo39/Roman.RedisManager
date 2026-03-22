# Mutation Analysis

## Scope

- Mutable project: Roman.RedisManager.Application
- Baseline report: tests/Roman.RedisManager.Tests/StrykerOutput/Roman.RedisManager.Application/reports/mutation-report.json
- Rerun report: tests/Roman.RedisManager.Tests/StrykerOutput/reports/mutation-report.json

## Execution Evidence

1. Baseline mutation run completed with score 55.97%.
2. Findings captured in specs/007-spa-oidc-config/checklists/mutation-findings-run1.md and specs/007-spa-oidc-config/checklists/mutation-findings-run1.json.
3. Assertion improvements applied in tests/Roman.RedisManager.Tests/Application/CQRS/AuthenticationBootstrapQueryHandlerTests.cs.
4. Rerun mutation run completed with score 72.33%.
5. Delta report generated in specs/007-spa-oidc-config/checklists/mutation-comparison.md.

## Surviving/No-Coverage Analysis

### Actionable (feature scope)

- AuthenticationBootstrapQueryHandler responseType fallback branch (line 107 in baseline findings).
- AuthenticationBootstrapQueryHandler metadata override branch (lines 119-126 in baseline findings).

### Non-actionable for this feature slice

- Surviving mutants in unrelated Application CQRS handlers (DeleteKey, GetKeyMetadata, RedisDataTypes handlers, RedisKeysSearch, RedisServerGroupDetail).
- Reason: these mutants are outside the SPA OIDC bootstrap feature scope and require separate behavior-focused test hardening in their own feature areas.

## Assertion Updates Applied

- Added response type fallback tests for null, empty, and whitespace values.
- Added explicit response type preservation test for configured non-empty value.
- Added test verifying metadata overrides are null when no override values are provided.
- Added parameterized test verifying metadata override object is created when any single metadata override field is configured.

## Outcome

- Mutation score improved from 55.97% to 72.33% (delta +16.36).
- Baseline feature-scope survivors in AuthenticationBootstrapQueryHandler are no longer listed in run2 findings.
- Remaining survivors are tracked as non-actionable for this feature implementation and should be handled in dedicated follow-up work.
