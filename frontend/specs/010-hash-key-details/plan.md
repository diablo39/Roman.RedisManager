# Implementation Plan: Hash Key Details Viewer

**Branch**: `010-hash-key-details` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-hash-key-details/spec.md`

## Summary

Implement a Hash Key Details dialog that allows users to view, edit, add, and delete hash field-value pairs from the Keys tab. The dialog follows the established `StringKeyDetailDialog` pattern, adds cursor-paginated hash field fetching via `GET /api/redis/data/hashes`, inline field editing, JSON formatting with CodeMirror, deep linking via URL query params, and scroll-position preservation on close.

## Technical Context

**Language/Version**: TypeScript 5.9, Vue 3.5
**Primary Dependencies**: Vuetify 3.10, Pinia 3.0, Vue Router 4.5, vue-codemirror 6.1, @codemirror/lang-json 6.0
**Storage**: N/A (frontend-only; data persisted via backend Redis API)
**Testing**: Vitest + @vue/test-utils (unit/component tests in `tests/`)
**Target Platform**: Web (desktop/tablet browsers)
**Project Type**: Single-page web application (Vue 3 SPA)
**Performance Goals**: Dialog open < 2s, up to 1000 fields with initial load < 3s (SC-001, SC-008)
**Constraints**: Must use Vuetify 3 exclusively, Pinia Options API, file-based routing, TypeScript strict mode
**Scale/Scope**: Single dialog component + API client additions + page integration + deep linking

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Code Quality Is Enforced | PASS | TypeScript strict mode, ESLint, `npm run build` / `npm run type-check` gates |
| II. Testing Is Mandatory | PASS | Spec defines test tasks per user story; tests planned in `tests/component/` and `tests/unit/` |
| III. User Experience Consistency | PASS | Dialog follows established `StringKeyDetailDialog` pattern; card-header-separated, card-footer-separated, loading/error/empty states, ArchitectUI design system |
| IV. Performance Budgets | PASS | SC-001 (< 2s open), SC-008 (< 3s for 1000 fields) defined; cursor-based pagination prevents loading all fields at once |
| V. Simplicity & Traceability | PASS | Single component + API additions; all requirements traceable to user stories US-1 through US-6 |

**Engineering Standards**:
- Vue 3 + TypeScript + Vuetify conventions: PASS (following existing codebase patterns)
- No `any` types: PASS (all DTOs fully typed)
- Accessibility in acceptance criteria: PASS (keyboard navigation, ARIA labels planned)
- Performance criteria specified: PASS (SC-001 through SC-009)

**Gate result**: ALL PASS — proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/010-hash-key-details/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
frontend/
├── src/
│   ├── api/
│   │   └── redisKeys.ts          # Add hash field fetch/update/delete functions
│   ├── components/
│   │   ├── HashKeyDetailDialog.vue  # NEW: Main hash key detail dialog
│   │   ├── StringKeyDetailDialog.vue # Existing reference pattern
│   │   ├── RedisKeysExplorer.vue    # MODIFY: Make hash keys clickable
│   │   ├── TtlPicker.vue           # Reuse existing
│   │   └── CreateKeyDialog.vue      # Existing (no changes)
│   └── pages/
│       └── redis/
│           └── [id].vue            # MODIFY: Register dialog, deep linking, close handler
└── tests/
    ├── component/                   # Component tests for HashKeyDetailDialog
    └── unit/                        # Unit tests for API functions, JSON formatting
```

**Structure Decision**: Frontend-only feature within existing `src/components/` + `src/api/` structure. No new directories needed beyond `specs/010-hash-key-details/contracts/`.

## Complexity Tracking

> No constitution violations — table not applicable.
