# Mutation Findings

## Summary

- Survived mutants: 44
- No-coverage mutants: 0
- Duplicate/low-differentiation candidates: 0 (heuristics implemented; no duplicate candidates emitted from current extraction script)

## Findings

- Findings published in `backend/tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.md` and `backend/tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.json`.
- Top impacted area: `src/Roman.RedisManager.Application/CQRS/` command and query handlers.

## Disposition

- Actionable findings addressed: Tooling and governance changes completed in this feature; assertion-hardening backlog remains for follow-up cycles.
- Non-actionable findings logged in `non-actionable-mutant-exceptions.md`: None in this run.
