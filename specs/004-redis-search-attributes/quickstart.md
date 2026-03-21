# Quickstart: Redis Search Metadata Expansion

## Goal

Implement and validate enriched metadata on `GET /api/redis-keys` with bounded Redis request load.

## 1. Implement

1. Extend domain search key model to carry metadata used in search output.
2. Update `IRedisRepository.SearchForKeysAsync` flow to enrich keys with `type` + `ttlMilliseconds` using pipelined async metadata calls.
3. Add/Bind validated search-limit options (`MaxPageSize`) and enforce cap in query handling path.
4. Update `RedisKeysSearchQueryHandler` DTO mapping to output:
- `key`
- `type`
- `ttlMilliseconds`
- `hasExpiration`
5. Keep controller dispatch via Wolverine only; do not call repositories from controller.

## 2. Verify Build and Tests

From `F:\Source\Repos\Roman.RedisManager\backend`:

```powershell
dotnet build -warnaserror
dotnet test
```

## 3. Mutation Quality Loop (Mandatory)

Run the full loop and retain evidence:

```powershell
# run
pwsh -NoProfile -File .specify/scripts/powershell/run-mutation-tests.ps1

# analyze survivors/no-coverage
pwsh -NoProfile -File .specify/scripts/powershell/analyze-surviving-mutants.ps1

# compare reports after improvements
pwsh -NoProfile -File .specify/scripts/powershell/compare-mutation-reports.ps1
```

Required evidence:
- Mutation HTML and JSON reports under `tests/Roman.RedisManager.Tests/StrykerOutput/**/reports/`.
- Surviving/no-coverage analysis notes.
- Assertion updates for actionable findings.
- Follow-up run with improved score or documented non-actionable exceptions.

## 4. Manual Endpoint Check

Use `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` and validate:
- response key items include metadata fields
- persistent keys show `ttlMilliseconds = null`, `hasExpiration = false`
- expiring keys show positive `ttlMilliseconds`, `hasExpiration = true`
- oversized `pageSize` requests are capped safely

## 5. Performance Validation (SC-003)

Run a scripted validation of at least 100 capped-size requests and record p95 latency evidence:

- requests executed
- p95 completion time
- environment notes (Redis topology, host machine)

Store results in `specs/001-redis-search-attributes/checklists/performance-validation.md`.

## 6. Usability Validation (SC-004)

Run a structured checklist over at least 10 representative keys and record whether each was correctly identified as persistent or expiring from one response view:

- key sample id
- expected persistence classification
- observed classification
- pass/fail

Store results in `specs/001-redis-search-attributes/checklists/usability-validation.md`.

## 7. Execution Notes (2026-03-07)

- `dotnet build -warnaserror`: PASS
- `dotnet test`: PASS (`total: 231; failed: 0`)
- Mutation run 1: score `58.33%`
- Mutation analysis artifacts generated:
	- `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.md`
	- `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.json`
- Assertion improvements applied in:
	- `tests/Roman.RedisManager.Tests/Application/CQRS/RedisKeysSearchQueryHandlerTests.cs`
	- `tests/Roman.RedisManager.Tests/Infrastructure/Repositories/RedisRepositoryTests.cs`
- Mutation run 2: score `59.26%`
- Mutation comparison: `tests/Roman.RedisManager.Tests/StrykerOutput/mutation-comparison.md` (delta `+3.26`, status `improved`)
- Performance validation (SC-003): PASS (`requests=100`, `p95=0ms`), recorded in `specs/001-redis-search-attributes/checklists/performance-validation.md`
- Usability validation (SC-004): PASS (`samples=10`, `accuracy=100%`), recorded in `specs/001-redis-search-attributes/checklists/usability-validation.md`
