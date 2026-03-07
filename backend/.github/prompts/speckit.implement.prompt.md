---
agent: speckit.implement
---

When implementation includes creating or changing tests, execution MUST include mutation testing evidence using `run -> analyze -> improve -> rerun`.

Required completion evidence:

1. HTML and JSON mutation reports for the current run.
2. Surviving/no-coverage mutant analysis with actionable recommendations.
3. Assertion improvements mapped to actionable findings.
4. Follow-up mutation run summary showing score delta, or approved non-actionable mutant exception.

Implementation is incomplete until mutation-loop evidence exists for test changes.
