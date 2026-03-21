# US3 Agent Context And Durability Evidence

## Updated Files

- `.github/copilot-instructions.md`
- `.specify/memory/constitution.md`
- `.specify/templates/spec-template.md`
- `.specify/templates/plan-template.md`
- `.specify/templates/tasks-template.md`
- `.github/prompts/speckit.implement.prompt.md`
- `.github/prompts/speckit.tasks.prompt.md`
- `../.specify/templates/agent-file-template.md`

## Validation

- Command: `pwsh -NoProfile -File .specify/scripts/powershell/validate-mutation-governance-durability.ps1`
- Result: Passed
- Notes: Required mutation-loop phrase `run -> analyze -> improve -> rerun` verified across backend and root governance surfaces.
