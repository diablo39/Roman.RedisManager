# Implementation Plan: Collection Key Details (Sets, Sorted Sets, Lists)

**Branch**: `001-collection-key-details` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-collection-key-details/spec.md`

## Summary

Add detail dialogs for Redis sets, sorted sets, and lists — enabling users to open any key type from the Keys tab, view/edit/add/remove members with pagination, format JSON values, deep link to dialogs, and auto-close after save. Three new dialog components follow the established HashKeyDetailDialog pattern, with six new API client functions consuming existing backend endpoints.

## Technical Context

**Language/Version**: TypeScript 5.9, Vue 3.5
**Primary Dependencies**: Vuetify 3.10, Pinia 3.0, Vue Router 4.5, CodeMirror (via existing integration)
**Storage**: N/A (frontend only — backend manages Redis)
**Testing**: Vitest + @vue/test-utils (unit/component), Playwright MCP (visual validation)
**Target Platform**: Web browser (SPA)
**Project Type**: Web application (frontend)
**Performance Goals**: < 2 seconds per page load for collection data
**Constraints**: Must follow existing dialog patterns, Vuetify-only UI, Options API for stores
**Scale/Scope**: 3 new dialog components, 6 new API functions, 3 existing file modifications

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Code Quality Is Enforced | PASS | TypeScript strict mode, ESLint, type-checked API functions |
| II. Testing Is Mandatory | PASS | Playwright MCP visual validation planned; component tests for dialogs |
| III. User Experience Consistency Is Required | PASS | All three dialogs follow HashKeyDetailDialog patterns — same header/body/footer, same loading/error/dirty states, same deep link mechanism |
| IV. Performance Budgets Are Non-Negotiable | PASS | SC-002 defines < 2s per page load target; pagination prevents loading all data at once |
| V. Simplicity, Traceability, and Maintainability | PASS | Three focused components (one per type) rather than one complex generic; each traces to specific user stories |

**Engineering Standards**:
- Vue 3 + TypeScript + Vuetify conventions: PASS
- Type safety end-to-end: PASS (new interfaces for all API responses)
- Accessibility in UI acceptance criteria: PASS (aria-labels on dialogs, keyboard navigation via Vuetify)
- Performance criteria as measurable outcomes: PASS (SC-001 through SC-006)

**Gate Result**: PASS — no violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-collection-key-details/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research output
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
├── contracts/
│   └── api-contracts.md # Phase 1 API contracts
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── api/
│   └── redisKeys.ts              # MODIFY: Add 6 new functions + interfaces
├── components/
│   ├── SetKeyDetailDialog.vue     # CREATE: Set member detail dialog
│   ├── ListKeyDetailDialog.vue    # CREATE: List item detail dialog
│   ├── SortedSetKeyDetailDialog.vue # CREATE: Sorted set entry detail dialog
│   ├── RedisKeysExplorer.vue      # MODIFY: Make set/list/zset clickable
│   ├── HashKeyDetailDialog.vue    # REFERENCE: Pattern template
│   ├── StringKeyDetailDialog.vue  # REFERENCE: Pattern template
│   ├── TtlPicker.vue             # REUSE: TTL picker (no changes)
│   └── CreateKeyDialog.vue        # REFERENCE: Create patterns
├── pages/
│   └── redis/
│       └── [id].vue               # MODIFY: Mount dialogs, extend deep linking
└── styles/
    └── settings.scss              # REUSE: Card styles (no changes)

tests/
├── component/                     # Component tests for new dialogs
└── unit/                          # Unit tests for new API functions
```

**Structure Decision**: Single frontend project. Three new component files, three existing file modifications. No new directories or structural changes needed.

## Implementation Phases

### Phase 1: API Client Layer (FR-001 groundwork)

Add TypeScript interfaces and API functions for reading and removing collection data.

**New interfaces in `src/api/redisKeys.ts`**:
- `GetSetMembersResult` — `{ members: string[], cursor: number, hasMoreResults: boolean }`
- `GetListRangeResult` — `{ values: string[] }`
- `SortedSetEntryDto` — `{ member: string, score: number }`
- `GetSortedSetRangeResult` — `{ entries: SortedSetEntryDto[] }`
- `RemoveFromSetRequest` — `{ groupId: string, key: string, members: string[] }`
- `RemoveFromListRequest` — `{ groupId: string, key: string, value: string, count: number }`
- `RemoveFromSortedSetRequest` — `{ groupId: string, key: string, members: string[] }`

**New functions**:
- `getSetMembers(groupId, key, cursor?, pageSize?, signal?)` → cursor-based pagination
- `getListRange(groupId, key, start?, stop?, signal?)` → index-based range
- `getSortedSetRange(groupId, key, start?, stop?, signal?)` → index-based range
- `removeFromSet(request, signal?)` → batch member removal
- `removeFromList(request, signal?)` → value-based removal
- `removeFromSortedSet(request, signal?)` → batch member removal

**Traces to**: FR-001, FR-002, FR-003, FR-004, FR-007, FR-011

---

### Phase 2: Set Key Detail Dialog (US-1, US-4)

Create `SetKeyDetailDialog.vue` following the HashKeyDetailDialog pattern.

**Key design decisions**:
- Cursor-based pagination (identical to hash fields pattern)
- Single-column member table (value only, no field names)
- Edit = remove old member + add new member (sets are value-based)
- Add member form: single text input with "Add" button
- Inline CodeMirror editor for JSON-formatted values
- Batch save: `createSetKey()` for additions + `removeFromSet()` for removals via `Promise.all`

**Dialog structure**:
- Header: key name (read-only) + type chip ("set", purple) + member count
- Body: loading skeleton → error alert → member table with edit/delete actions
- Footer: "Add Member" button (left) + Cancel/Save buttons (right)
- TTL picker above footer

**Traces to**: FR-002, FR-005, FR-006, FR-007, FR-008, FR-011, FR-012, FR-016, FR-017, FR-018, FR-019, FR-020

---

### Phase 3: List Key Detail Dialog (US-2, US-5)

Create `ListKeyDetailDialog.vue` with index-based pagination.

**Key design decisions**:
- Index-based pagination: load items in pages (e.g., 0–99, 100–199)
- Two-column table: index + value
- Add item form: text input + direction selector (Head/Tail)
- Remove uses LREM with count=1 (remove first occurrence)
- Items displayed with their zero-based index position
- "Load More" fetches next range and appends

**Dialog structure**:
- Header: key name + type chip ("list", green) + item count
- Body: loading skeleton → error alert → items table with index, value, edit/delete
- Footer: "Add Item" button with direction toggle (left) + Cancel/Save (right)
- TTL picker above footer

**Traces to**: FR-003, FR-005, FR-006, FR-007, FR-008, FR-010, FR-011, FR-012, FR-016, FR-017, FR-018, FR-019, FR-020

---

### Phase 4: Sorted Set Key Detail Dialog (US-3, US-6)

Create `SortedSetKeyDetailDialog.vue` with score display and editing.

**Key design decisions**:
- Index-based pagination (entries returned in score order)
- Three-column table: member + score + actions
- Add entry form: member text input + score number input
- Score editing: inline number input for score changes
- Member editing: CodeMirror editor (same as other types)
- Batch save: `createSortedSetKey()` for additions/updates + `removeFromSortedSet()` for removals

**Dialog structure**:
- Header: key name + type chip ("zset", teal) + entry count
- Body: loading skeleton → error alert → entries table with member, score, edit/delete
- Footer: "Add Entry" button (left) + Cancel/Save (right)
- TTL picker above footer

**Traces to**: FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, FR-011, FR-012, FR-016, FR-017, FR-018, FR-019, FR-020

---

### Phase 5: Integration — Keys Explorer + Page + Deep Linking (US-7)

Modify existing files to connect the new dialogs.

**`RedisKeysExplorer.vue` changes**:
- Expand clickable types from `['string', 'hash']` to `['string', 'hash', 'set', 'list', 'zset']`

**`redis/[id].vue` changes**:
- Import and mount `SetKeyDetailDialog`, `ListKeyDetailDialog`, `SortedSetKeyDetailDialog`
- Add template refs with `InstanceType<typeof XxxDialog>` typing
- Extend `onOpenKey()` with `set`, `list`, `zset` cases
- Extend `checkDeepLink()` with `set`, `list`, `zset` cases
- Wire `@close="onKeyDialogClose"` for all three new dialogs

**Traces to**: FR-001, FR-013, FR-014, FR-015

---

### Phase 6: Testing & Validation

**Unit tests** (Vitest):
- API functions: test request construction, URL building, and response parsing for all 6 new functions

**Component tests** (Vitest + @vue/test-utils):
- Dialog components: test open/close lifecycle, dirty tracking, save behavior for each dialog

**Visual validation** (Playwright MCP):
- Screenshot each dialog with sample data
- Verify deep link navigation
- Test loading/error/empty states
- Validate dialog close after save
- Measure page load time for SC-002 performance target
- Verify keyboard accessibility for dialog navigation

**Traces to**: Constitution Principle II (Testing Is Mandatory), SC-001 through SC-006

## Complexity Tracking

> No constitution violations to justify — all gates pass.

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| 3 separate dialogs vs 1 generic | 3 separate | Each type has unique columns, pagination, and edit semantics; simpler to maintain |
| No new Pinia store | Component-local state | Follows existing pattern — hash/string dialogs don't use stores either |
| Reuse existing TtlPicker/CodeMirror | Yes | No modifications needed to shared components |
