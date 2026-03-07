---
agent: speckit.tasks
---

When specs/plans include test creation or modification, generated tasks MUST include mutation loop work: `run -> analyze -> improve -> rerun`.

At minimum, include tasks for:

1. Running mutation tests with HTML/JSON outputs.
2. Analyzing surviving/no-coverage mutants.
3. Improving assertions based on actionable findings.
4. Re-running mutation tests and capturing deltas or approved exceptions.
