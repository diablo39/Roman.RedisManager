# Tasks: Collection Key Details (Sets, Sorted Sets, Lists)

**Input**: Design documents from `/specs/001-collection-key-details/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-contracts.md, quickstart.md

**Tests**: Playwright MCP visual validation is explicitly requested in the feature specification.

**Organization**: Tasks are grouped by dialog component, each serving multiple user stories. Each dialog (set, list, sorted set) is built as a complete unit with viewing, editing, and TTL management.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Foundational — API Client Layer

**Purpose**: Add all TypeScript interfaces and API functions needed by every dialog. MUST complete before any dialog work begins.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T001 Add `GetSetMembersResult` interface and `getSetMembers(groupId, key, cursor?, pageSize?, signal?)` function in `src/api/redisKeys.ts` — cursor-based pagination matching the `GET api/redis/data/sets` endpoint
- [x] T002 Add `RemoveFromSetRequest` interface and `removeFromSet(request, signal?)` function in `src/api/redisKeys.ts` — posts to `api/redis/data/sets/remove`
- [x] T003 Add `GetListRangeResult` interface and `getListRange(groupId, key, start?, stop?, signal?)` function in `src/api/redisKeys.ts` — index-based range matching the `GET api/redis/data/lists` endpoint
- [x] T004 Add `RemoveFromListRequest` interface and `removeFromList(request, signal?)` function in `src/api/redisKeys.ts` — posts to `api/redis/data/lists/remove`
- [x] T005 Add `SortedSetEntryDto`, `GetSortedSetRangeResult` interfaces and `getSortedSetRange(groupId, key, start?, stop?, signal?)` function in `src/api/redisKeys.ts` — index-based range matching the `GET api/redis/data/sorted-sets` endpoint
- [x] T006 Add `RemoveFromSortedSetRequest` interface and `removeFromSortedSet(request, signal?)` function in `src/api/redisKeys.ts` — posts to `api/redis/data/sorted-sets/remove`
- [x] T007 Run `npm run type-check` and `npm run build` to verify all new interfaces and functions compile without errors

**Checkpoint**: All 6 API functions and 7 interfaces are in place. Dialog implementation can now begin.

---

## Phase 2: US1 + US4 + US8 — Set Key Detail Dialog (Priority: P1/P2/P3)

**Goal**: Users can open a set key from the Keys tab, view all members with cursor-based pagination, add/edit/remove members with batch save, manage TTL, and auto-close dialog after save.

**Independent Test**: Navigate to a server with set keys, click a set key in Keys tab, verify dialog opens with members listed, add a member, remove a member, save, and confirm dialog closes and changes persist.

### Implementation

- [x] T008 [US1] Create `src/components/SetKeyDetailDialog.vue` with dialog shell: `v-dialog` with `v-model="visible"`, `@update:model-value="onDialogChange"`, `max-width="900"`, `v-card` with `aria-label`. Add `open(key: string)` method exposed via `defineExpose`. Add `close()` and `onDialogChange()` with AbortController pattern matching `HashKeyDetailDialog.vue`
- [x] T009 [US1] Add header section in `SetKeyDetailDialog.vue`: `.card-header-separated` with key name (read-only `v-text-field`), type chip (`variant="tonal"`, `size="x-small"`, color purple, label "set"), and member count display with "more available" indicator
- [x] T010 [US1] Add loading state (skeleton loader), error state (`v-alert type="error" variant="tonal"` with Retry button), empty state (centered icon 48px + "No members" title + "Add a member to get started" description), and ready state switching in `SetKeyDetailDialog.vue` body
- [x] T011 [US1] Implement `fetchMembers()` function in `SetKeyDetailDialog.vue`: call `getSetMembers()` with cursor-based pagination, populate `members` ref array, track `cursor` and `hasMoreResults` for Load More
- [x] T012 [US1] Add members table in `SetKeyDetailDialog.vue` ready state: `v-table density="compact" hover` with single "Value" column, truncated display (500 char limit), and row actions column
- [x] T013 [US1] Add "Load More" button below members table in `SetKeyDetailDialog.vue`: visible when `hasMoreResults` is true, calls `fetchMembers()` with current cursor, appends results to existing members array, updates `snapshotOriginals()`. Handle fetch errors with inline error alert and retry — preserve already-loaded members on failure
- [x] T014 [US1] Add JSON detection and formatting: when editing a member value, detect JSON strings and enable CodeMirror with JSON syntax highlighting (follow the pattern from `HashKeyDetailDialog.vue`). Add large value warning (> 1MB) consistent with `StringKeyDetailDialog.vue` behavior
- [x] T015 [US4] Add inline edit mode for set members in `SetKeyDetailDialog.vue`: click edit icon → show CodeMirror editor for that member's value, with JSON format button. Track edits as remove-old + add-new pairs
- [x] T016 [US4] Add "Add Member" functionality in `SetKeyDetailDialog.vue` footer: text input with "Add" button in `.card-footer-separated`, validate non-empty, warn on duplicate member, append to members list marked as new
- [x] T017 [US4] Add delete/restore functionality for set members in `SetKeyDetailDialog.vue`: delete icon marks member in `pendingDeletions` Set with strikethrough styling, restore icon reverses. Filter out pending deletions from display as appropriate
- [x] T018 [US4] Implement dirty tracking in `SetKeyDetailDialog.vue`: computed `isDirty` checks `pendingDeletions.size > 0`, new members added, edited members (old value differs from new), TTL changes
- [x] T019 [US4] Implement `save()` function in `SetKeyDetailDialog.vue`: collect additions (new members + edited member new values) → `createSetKey()`, collect removals (deleted members + edited member old values) → `removeFromSet()`, execute via `Promise.all`, call `close()` on success, show `saveError` on failure
- [x] T020 [US8] Add TtlPicker component to `SetKeyDetailDialog.vue`: place above footer, bind to `currentTtl` ref, fetch initial TTL via `getKeyMetadata()`, include TTL in save operation
- [x] T021 [US4] Add Cancel and Save buttons in `SetKeyDetailDialog.vue` footer: Cancel calls `close()`, Save disabled when `!isDirty`, Save shows `:loading="saving"` state

**Checkpoint**: Set Key Detail Dialog is fully functional — view, edit, add, remove, TTL, batch save with auto-close.

---

## Phase 3: US2 + US5 + US8 — List Key Detail Dialog (Priority: P1/P2/P3)

**Goal**: Users can open a list key, view items with index positions and index-based pagination, add items to head or tail, edit/remove items, manage TTL, and auto-close after save.

**Independent Test**: Click a list key in Keys tab, verify items show with index numbers, add an item to tail, remove an item, save, confirm dialog closes.

### Implementation

- [x] T022 [US2] Create `src/components/ListKeyDetailDialog.vue` with dialog shell: `v-dialog`, `v-card`, `open(key)` via `defineExpose`, `close()`, `onDialogChange()` with AbortController pattern, emit `close` event
- [x] T023 [US2] Add header section in `ListKeyDetailDialog.vue`: `.card-header-separated` with key name, type chip (color green, label "list"), item count display
- [x] T024 [US2] Add loading/error/ready/empty state switching in `ListKeyDetailDialog.vue` body (skeleton loader, error alert with retry, empty state with centered icon + "No items" message, ready content)
- [x] T025 [US2] Implement `fetchItems()` function in `ListKeyDetailDialog.vue`: call `getListRange()` with start/stop indices, compute item indices client-side from start offset. Detect "has more" by checking if returned item count equals the requested page size (if fewer items returned than requested, no more exist)
- [x] T026 [US2] Add items table in `ListKeyDetailDialog.vue`: `v-table density="compact" hover` with Index column (fixed width) and Value column (truncated at 500 chars), row actions column
- [x] T027 [US2] Add "Load More" button in `ListKeyDetailDialog.vue`: track `loadedCount`, request next page range (`loadedCount` to `loadedCount + pageSize - 1`), append to items array, update originals snapshot. Handle fetch errors with inline error alert and retry — preserve already-loaded items on failure
- [x] T028 [US2] Add JSON detection, CodeMirror editor, and large value warning (> 1MB) for list item values in `ListKeyDetailDialog.vue` (same pattern as set dialog T014)
- [x] T029 [US5] Add inline edit mode for list items in `ListKeyDetailDialog.vue`: click edit icon → CodeMirror editor for value, with JSON format button. Track original value for dirty comparison
- [x] T030 [US5] Add "Add Item" functionality in `ListKeyDetailDialog.vue` footer: text input with direction toggle (Head/Tail via `v-btn-toggle` or `v-select`), "Add" button, append to items list marked as new with direction metadata
- [x] T031 [US5] Add delete/restore functionality for list items in `ListKeyDetailDialog.vue`: delete icon marks item in `pendingDeletions` with strikethrough, restore reverses
- [x] T032 [US5] Implement dirty tracking in `ListKeyDetailDialog.vue`: computed `isDirty` comparing items against originals, checking pending deletions, new items, and TTL changes
- [x] T033 [US5] Implement `save()` function in `ListKeyDetailDialog.vue`: collect new items → `createListKey()` with direction, collect deletions → `removeFromList()` with count=1 for each, collect value edits → remove old + push new, execute via `Promise.all`, close on success
- [x] T034 [US8] Add TtlPicker to `ListKeyDetailDialog.vue`: place above footer, fetch TTL via `getKeyMetadata()`, include in save
- [x] T035 [US5] Add Cancel and Save buttons in `ListKeyDetailDialog.vue` footer: Cancel calls `close()`, Save disabled when `!isDirty`, Save shows loading state

**Checkpoint**: List Key Detail Dialog is fully functional — view with indices, edit, add to head/tail, remove, TTL, batch save with auto-close.

---

## Phase 4: US3 + US6 + US8 — Sorted Set Key Detail Dialog (Priority: P1/P2/P3)

**Goal**: Users can open a sorted set key, view entries with member + score sorted by score, paginate, add entries with scores, edit scores and member values, remove entries, manage TTL, and auto-close after save.

**Independent Test**: Click a zset key in Keys tab, verify entries show member and score in score order, add an entry with score, edit a score, save, confirm dialog closes.

### Implementation

- [x] T036 [US3] Create `src/components/SortedSetKeyDetailDialog.vue` with dialog shell: `v-dialog`, `v-card`, `open(key)` via `defineExpose`, `close()`, `onDialogChange()` with AbortController, emit `close`
- [x] T037 [US3] Add header section in `SortedSetKeyDetailDialog.vue`: `.card-header-separated` with key name, type chip (color teal, label "zset"), entry count display
- [x] T038 [US3] Add loading/error/ready/empty state switching in `SortedSetKeyDetailDialog.vue` body (skeleton loader, error alert with retry, empty state with centered icon + "No entries" message, ready content)
- [x] T039 [US3] Implement `fetchEntries()` function in `SortedSetKeyDetailDialog.vue`: call `getSortedSetRange()` with start/stop, track loaded count for pagination
- [x] T040 [US3] Add entries table in `SortedSetKeyDetailDialog.vue`: `v-table density="compact" hover` with Member column (truncated at 500 chars), Score column (fixed width, right-aligned), and Actions column
- [x] T041 [US3] Add "Load More" button in `SortedSetKeyDetailDialog.vue`: request next range, append entries, update originals. Handle fetch errors with inline error alert and retry — preserve already-loaded entries on failure
- [x] T042 [US3] Add JSON detection, CodeMirror editor, and large value warning (> 1MB) for sorted set member values (same pattern as set/list dialogs)
- [x] T043 [US6] Add inline edit for member values in `SortedSetKeyDetailDialog.vue`: CodeMirror editor with JSON format support
- [x] T044 [US6] Add inline score editing in `SortedSetKeyDetailDialog.vue`: click score → `v-text-field type="number"` for inline score modification, validate numeric input
- [x] T045 [US6] Add "Add Entry" functionality in `SortedSetKeyDetailDialog.vue` footer: member text input + score number input + "Add" button, validate both fields, append to entries marked as new
- [x] T046 [US6] Add delete/restore functionality for sorted set entries in `SortedSetKeyDetailDialog.vue`: delete marks in `pendingDeletions`, restore reverses, strikethrough styling
- [x] T047 [US6] Implement dirty tracking in `SortedSetKeyDetailDialog.vue`: computed `isDirty` comparing entries (member + score) against originals, checking deletions, new entries, TTL
- [x] T048 [US6] Implement `save()` function in `SortedSetKeyDetailDialog.vue`: collect additions and score changes → `createSortedSetKey()` (ZADD handles both add and update), collect removals → `removeFromSortedSet()`, execute via `Promise.all`, close on success
- [x] T049 [US8] Add TtlPicker to `SortedSetKeyDetailDialog.vue`: place above footer, fetch TTL via `getKeyMetadata()`, include in save
- [x] T050 [US6] Add Cancel and Save buttons in `SortedSetKeyDetailDialog.vue` footer with dirty/loading state

**Checkpoint**: Sorted Set Key Detail Dialog is fully functional — view with scores, edit members and scores, add entries, remove, TTL, batch save with auto-close.

---

## Phase 5: US7 — Integration, Keys Explorer & Deep Linking (Priority: P2)

**Goal**: All three key types are clickable in the Keys tab, dialogs are mounted in the server page, URL deep linking works for set/list/zset types, and closing a dialog preserves list context.

**Independent Test**: Click a set/list/zset key in Keys tab → dialog opens. Copy URL, paste in new tab → correct dialog opens via deep link. Close dialog → Keys tab list unchanged.

### Implementation

- [x] T051 [US7] Update `src/components/RedisKeysExplorer.vue`: change the `v-if` condition from `['string', 'hash'].includes(item.type.toLowerCase())` to `['string', 'hash', 'set', 'list', 'zset'].includes(item.type.toLowerCase())` to make all key types clickable
- [x] T052 [US7] Add template refs and dialog mounting in `src/pages/redis/[id].vue`: add `<SetKeyDetailDialog>`, `<ListKeyDetailDialog>`, `<SortedSetKeyDetailDialog>` components with `:group-id="id"` and `@close="onKeyDialogClose"`. Add typed refs: `ref<InstanceType<typeof SetKeyDetailDialog> | null>(null)` for each
- [x] T053 [US7] Extend `onOpenKey()` function in `src/pages/redis/[id].vue`: add `else if` branches for `'set'`, `'list'`, `'zset'` types that call the corresponding dialog ref's `.open(key)` method and `router.replace()` with the type in query params
- [x] T054 [US7] Extend `checkDeepLink()` function in `src/pages/redis/[id].vue`: add `else if` branches for `queryType === 'set'`, `'list'`, `'zset'` that set `tab.value = 'keys'` and call the dialog's `.open(queryKey)` via `nextTick()`
- [x] T055 [US7] Run `npm run build` and `npm run type-check` to verify full compilation with all new components integrated

**Checkpoint**: All five Redis key types are clickable, dialogs open/close correctly, deep linking works for all types, and Keys tab context is preserved.

---

## Phase 6: Unit & Component Tests

**Purpose**: Satisfy Constitution Principle II — unit tests for API functions and component tests for dialog lifecycle.

### Unit Tests

- [x] T056 Create `tests/unit/redisKeys.spec.ts`: test `getSetMembers()` builds correct URL with groupId, key, cursor, and pageSize query params, and parses `GetSetMembersResult` response
- [x] T057 [P] Add tests in `tests/unit/redisKeys.spec.ts` for `getListRange()` — verifies URL with start/stop params — and `getSortedSetRange()` — verifies URL with start/stop params and parses `SortedSetEntryDto[]` response
- [x] T058 [P] Add tests in `tests/unit/redisKeys.spec.ts` for `removeFromSet()`, `removeFromList()`, and `removeFromSortedSet()` — verify each posts correct JSON body to the right endpoint

### Component Tests

- [x] T059 Create `tests/component/SetKeyDetailDialog.spec.ts`: test `open()` sets visible and triggers fetch, `close()` hides dialog and emits `close`, dirty tracking returns false when no changes and true after member addition/deletion
- [x] T060 [P] Create `tests/component/ListKeyDetailDialog.spec.ts`: test `open()` lifecycle, dirty tracking, and that save collects correct add/remove payloads
- [x] T061 [P] Create `tests/component/SortedSetKeyDetailDialog.spec.ts`: test `open()` lifecycle, dirty tracking for score edits, and that save collects correct ZADD/ZREM payloads

**Checkpoint**: All unit and component tests pass via `npm run test`.

---

## Phase 7: Validation — Playwright MCP & Build Verification

**Purpose**: Visual validation of all three dialogs using Playwright MCP, plus build/lint/type verification.

- [x] T062 Run `npm run lint` to verify no lint errors across all new and modified files
- [x] T063 Run `npm run type-check` to verify TypeScript strict mode passes
- [x] T064 Run `npm run build` to verify production build succeeds
- [x] T065 Use Playwright MCP to navigate to a Redis server with set keys, open a set key detail dialog, and screenshot the result to `.playwright-mcp/set-key-dialog.png`
- [x] T066 Use Playwright MCP to navigate to a Redis server with list keys, open a list key detail dialog, and screenshot to `.playwright-mcp/list-key-dialog.png`
- [x] T067 Use Playwright MCP to navigate to a Redis server with sorted set keys, open a sorted set key detail dialog, and screenshot to `.playwright-mcp/sorted-set-key-dialog.png`
- [x] T068 Use Playwright MCP to test deep linking: navigate directly to a URL with `?key=testkey&type=set&tab=keys`, verify the set dialog opens automatically, screenshot to `.playwright-mcp/deep-link-set.png`
- [x] T069 Use Playwright MCP to test dialog close after save: open a set dialog, add a member, click Save, verify dialog closes and Keys tab is still showing, screenshot to `.playwright-mcp/save-close-flow.png`
- [x] T070 Use Playwright MCP to verify empty state: open a detail dialog for a key with no members, screenshot to `.playwright-mcp/empty-state.png`
- [x] T071 Use Playwright MCP `browser_evaluate` to measure dialog data load time: open a set dialog and assert time from open to data rendered is < 2 seconds (SC-002 validation)
- [x] T072 Use Playwright MCP to verify keyboard accessibility: open a dialog, Tab through all interactive elements (fields, buttons, close), verify focus order is logical and all actions are reachable without mouse

**Checkpoint**: All visual validations pass, build is clean, tests pass, feature is complete.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Foundational — API Client)**: No dependencies — start immediately. BLOCKS all dialog phases.
- **Phase 2 (Set Dialog)**: Depends on Phase 1 completion
- **Phase 3 (List Dialog)**: Depends on Phase 1 completion. Can run in parallel with Phase 2.
- **Phase 4 (Sorted Set Dialog)**: Depends on Phase 1 completion. Can run in parallel with Phases 2 and 3.
- **Phase 5 (Integration)**: Depends on Phases 2, 3, and 4 (all dialogs must exist to mount them)
- **Phase 6 (Unit & Component Tests)**: Depends on Phase 5 (all code must exist to test). Can run in parallel with Phase 7 build checks.
- **Phase 7 (Validation)**: Depends on Phase 5 (integration must be complete for E2E testing)

### User Story Dependencies

- **US1 (View Set)**: Depends on Phase 1 API functions → implemented in Phase 2
- **US2 (View List)**: Depends on Phase 1 API functions → implemented in Phase 3
- **US3 (View Sorted Set)**: Depends on Phase 1 API functions → implemented in Phase 4
- **US4 (Edit Set)**: Implemented alongside US1 in Phase 2
- **US5 (Edit List)**: Implemented alongside US2 in Phase 3
- **US6 (Edit Sorted Set)**: Implemented alongside US3 in Phase 4
- **US7 (Deep Linking)**: Depends on all three dialogs → Phase 5
- **US8 (TTL Management)**: Implemented within each dialog phase (Phases 2, 3, 4)

### Within Each Dialog Phase

1. Dialog shell and lifecycle (open/close/abort)
2. Header with key info and type chip
3. Loading/error states
4. Data fetching and table display
5. Pagination (Load More)
6. JSON detection and CodeMirror
7. Edit/add/remove functionality
8. Dirty tracking
9. Save with auto-close
10. TTL picker integration
11. Footer buttons

### Parallel Opportunities

- **Phase 1**: T001–T006 all modify the same file (`redisKeys.ts`) so must be sequential
- **Phases 2, 3, 4**: Each creates a different file — can run in parallel after Phase 1
- **Phase 5**: T051 modifies `RedisKeysExplorer.vue`, T052–T054 modify `[id].vue` — can run in parallel
- **Phase 6**: Unit test tasks T057–T058 marked [P] can run in parallel; component tests T059–T061 marked [P] can run in parallel (different test files)
- **Phase 7**: T062–T064 (build checks) can run in parallel; T065–T072 (Playwright) are sequential

---

## Parallel Example: Dialog Phases

```text
# After Phase 1 completes, launch all three dialog phases in parallel:
Phase 2: SetKeyDetailDialog.vue      (T008–T021)
Phase 3: ListKeyDetailDialog.vue     (T022–T035)
Phase 4: SortedSetKeyDetailDialog.vue (T036–T050)

# All three write to different files — no conflicts

# After Phase 5, launch test + validation in parallel:
Phase 6: Unit & Component Tests      (T056–T061)
Phase 7: Build checks T062–T064      (parallel with Phase 6)
Phase 7: Playwright T065–T072        (after build checks)
```

---

## Implementation Strategy

### MVP First (Set Dialog Only)

1. Complete Phase 1: API Client Layer
2. Complete Phase 2: Set Key Detail Dialog (US1 + US4 + US8)
3. Add minimal integration: make set clickable in Keys Explorer + mount dialog
4. **STOP and VALIDATE**: Test set dialog independently with Playwright
5. Proceed to remaining dialogs

### Incremental Delivery

1. Phase 1 → API layer ready
2. Phase 2 → Set dialog complete → validate
3. Phase 3 → List dialog complete → validate
4. Phase 4 → Sorted set dialog complete → validate
5. Phase 5 → Full integration + deep linking → validate
6. Phase 6 → Unit & component tests pass
7. Phase 7 → Final Playwright validation + build verification → done

### Parallel Strategy

1. Complete Phase 1 (API functions — single file, sequential)
2. Launch Phases 2, 3, 4 in parallel (each is an independent `.vue` file)
3. When all complete → Phase 5 (integration)
4. Launch Phase 6 (tests) and Phase 7 build checks in parallel
5. Phase 7 Playwright validation (sequential)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each dialog is a complete, independently testable unit
- All API functions go in one existing file — must be sequential within Phase 1
- Commit after each phase completes
- The spec requests Playwright MCP validation — Phase 6 handles this explicitly
- Auto-close after save is implemented in the `save()` function of each dialog (call `close()` on success)
