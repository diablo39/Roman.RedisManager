# Implementation Plan: String Key Details View

**Branch**: `009-string-key-details` | **Date**: 2026-03-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/009-string-key-details/spec.md`

## Summary

Add a dialog for viewing and editing Redis string keys. The dialog opens from the keys list when clicking a string key name, displaying key name, type, value (with JSON syntax highlighting via CodeMirror 6), and TTL in an editable form. Deep linking is achieved through URL query parameters on the existing server page (`?key=<name>&type=string`), which auto-opens the dialog on page load.

## Technical Context

**Language/Version**: TypeScript 5.9
**Primary Dependencies**: Vue 3.5, Vuetify 3.10, Pinia 3.0, Vue Router 4.5, vue-codemirror (v6), @codemirror/lang-json
**Storage**: N/A (frontend only; Redis accessed via backend API)
**Testing**: Vitest + @vue/test-utils (unit/component tests)
**Target Platform**: Web (modern browsers, SPA)
**Project Type**: Web application (frontend SPA)
**Performance Goals**: Dialog open with value display < 1 second; JSON formatting < 100ms for typical values
**Constraints**: Bundle size increase from CodeMirror ~50-80 KB gzipped; no server-side rendering
**Scale/Scope**: 1 new dialog component + 2 API functions + 1 modified page + 1 modified component

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Is Enforced

- **Post-design**: Plan modifies 2 existing files (`redisKeys.ts`, `RedisKeysExplorer.vue`, `[id].vue`) and adds 1 new component (`StringKeyDetailDialog.vue`). Removes the dedicated page file. All changes TypeScript-strict. **PASS**.

### II. Testing Is Mandatory

- **Post-design**: Test plan covers: JSON detection helper (unit), dialog state management (component). **PASS**.

### III. User Experience Consistency Is Required

- **Post-design**: Dialog follows the existing dialog pattern (card-header-separated inside v-dialog, same as delete confirmation and create key dialogs). Reuses TtlPicker, type chip colors. **PASS**.

### IV. Performance Budgets Are Non-Negotiable

- **Post-design**: CodeMirror loaded on-demand when dialog opens. Two parallel API calls. JSON.parse/stringify < 10ms for typical values. **PASS**.

### V. Simplicity, Traceability, and Maintainability

- **Post-design**: Dialog approach is simpler than a separate page — no new route, no navigation state management. Deep linking via query params on existing route. **PASS**.

## Project Structure

### Source Code (frontend/)

```text
frontend/
├── src/
│   ├── api/
│   │   └── redisKeys.ts                # MODIFIED: added getStringKeyValue(), getKeyMetadata()
│   ├── components/
│   │   ├── RedisKeysExplorer.vue        # MODIFY: add click handler to open dialog
│   │   ├── StringKeyDetailDialog.vue    # NEW: detail dialog component (replaces StringKeyDetail.vue + page)
│   │   └── TtlPicker.vue               # MODIFIED: added parseTimespan for incoming values
│   └── pages/
│       └── redis/
│           └── [id].vue                 # MODIFY: host dialog, handle deep link query params
├── tests/
│   ├── unit/
│   │   └── jsonDetection.spec.ts        # NEW: JSON detection logic tests
│   └── component/
│       └── StringKeyDetailDialog.spec.ts # NEW: dialog state tests
└── package.json                          # MODIFIED: added vue-codemirror, @codemirror/lang-json
```

**Structure Decision**: Dialog-based approach. No new page route needed. The dialog is hosted on the existing `/redis/:id` page and opened via click or deep link query params.

## Complexity Tracking

> No constitution violations. All gates pass. No complexity justifications needed.
