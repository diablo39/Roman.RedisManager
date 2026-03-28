# Tasks: Hash Key Details Viewer

**Input**: Design documents from `/specs/010-hash-key-details/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle II (Testing Is Mandatory).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new project setup needed — adding to existing codebase. This phase adds the shared API client functions that all user stories depend on.

- [ ] T001 Add `HashFieldDto`, `GetHashFieldsResult`, `RemoveHashFieldsRequest`, and `RemoveHashFieldsResult` interfaces to `src/api/redisKeys.ts` per contracts/api-client.md
- [ ] T002 Add `getHashFields()` function to `src/api/redisKeys.ts` — GET `/api/redis/data/hashes` with query params `groupId`, `key`, `cursor`, `pageSize`; follow `getStringKeyValue()` pattern
- [ ] T003 Add `removeHashFields()` function to `src/api/redisKeys.ts` — POST `/api/redis/data/hashes/remove` with JSON body; follow `createHashKey()` / `postJson()` pattern

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Make hash keys clickable in the keys list — required before ANY user story dialog work is useful

**⚠️ CRITICAL**: No user story work delivers value until hash keys can be opened from the list

- [ ] T004 Update `src/components/RedisKeysExplorer.vue` to make hash keys clickable — change the `v-if` condition on the key link `<a>` to include `'hash'` alongside `'string'` so clicking a hash key emits `open-key` with type `'hash'`

**Checkpoint**: Hash keys in the explorer are now clickable and emit the correct event

---

## Phase 3: User Story 1 — View Hash Key Details (Priority: P1) 🎯 MVP

**Goal**: Open a hash key from the Keys tab and view all its field-value pairs in a structured dialog with loading, error, empty, and paginated states.

**Independent Test**: Click a hash key in the Keys tab → dialog opens → all field-value pairs displayed in a two-column table. Empty hash shows empty state. Large hash shows Load More button.

**Acceptance**: spec.md scenarios 1.1–1.4, FR-001–003, FR-016–020, SC-001, SC-008

### Tests for User Story 1

- [ ] T005 [P] [US1] Create unit test for `getHashFields()` in `tests/unit/api/redisKeys.test.ts` — verify URL construction with query params, cursor/pageSize defaults, error handling for 404/409 responses
- [ ] T006 [P] [US1] Create component test for HashKeyDetailDialog loading state in `tests/component/HashKeyDetailDialog.test.ts` — mount with mock `groupId`, call `open()`, verify skeleton loader appears while API is pending
- [ ] T007 [P] [US1] Create component test for HashKeyDetailDialog error state in `tests/component/HashKeyDetailDialog.test.ts` — mock API to reject, verify error alert with retry button renders
- [ ] T008 [P] [US1] Create component test for HashKeyDetailDialog empty state in `tests/component/HashKeyDetailDialog.test.ts` — mock API returning 0 fields, verify empty state message renders
- [ ] T009 [P] [US1] Create component test for HashKeyDetailDialog ready state in `tests/component/HashKeyDetailDialog.test.ts` — mock API returning field-value pairs, verify table renders with field names and values in separate columns

### Implementation for User Story 1

- [ ] T010 [US1] Create `src/components/HashKeyDetailDialog.vue` with dialog scaffold — `v-dialog max-width="900"`, card with `card-header-separated` header (`mdi-code-braces` icon, "Hash Key Details" title, close button), `card-footer-separated` footer with Cancel button. Props: `groupId: string`. Emits: `close`. Expose: `open(key: string)`. Follow `StringKeyDetailDialog` pattern exactly. Add `aria-label` attributes on dialog, close button, and table for accessibility.
- [ ] T011 [US1] Implement loading state in `src/components/HashKeyDetailDialog.vue` — `v-skeleton-loader` with types `text, text, table-tbody` shown when `loading` is true. FR-016.
- [ ] T012 [US1] Implement error state in `src/components/HashKeyDetailDialog.vue` — `v-alert type="error" variant="tonal"` with error message and Retry button that re-calls `fetchData()`. FR-017.
- [ ] T013 [US1] Implement ready state in `src/components/HashKeyDetailDialog.vue` — read-only key name (`v-text-field disabled`), type chip (`v-chip color="orange" variant="tonal"` showing "hash"), field count display. FR-003.
- [ ] T014 [US1] Implement fields table in `src/components/HashKeyDetailDialog.vue` — `v-table density="compact" hover` with columns: Field Name (monospace), Value (truncated with overflow), Actions. Load fields via `getHashFields()` + `getKeyMetadata()` in parallel on `open()`. FR-002, FR-003.
- [ ] T015 [US1] Implement empty state in `src/components/HashKeyDetailDialog.vue` — centered icon (`mdi-code-braces-box`, 48px) + "No fields" title + "This hash key has no fields" description, shown when `fields.length === 0` after successful load. FR-018.
- [ ] T016 [US1] Implement cursor-based pagination in `src/components/HashKeyDetailDialog.vue` — accumulate fields array on load more, show "Load More" `v-btn variant="outlined"` when `hasMoreResults` is true, show `v-progress-linear` during additional page loads, display field count summary. FR-019. Follow `RedisKeysExplorer` load-more pattern per research.md R1.
- [ ] T017 [US1] Implement TTL display in `src/components/HashKeyDetailDialog.vue` — reuse `TtlPicker` component (read-only for now, editing is not a hash-specific story requirement). Use `msToTimespan()` conversion from `StringKeyDetailDialog`.
- [ ] T018 [US1] Register HashKeyDetailDialog in `src/pages/redis/[id].vue` — import component, add template ref, add `<HashKeyDetailDialog>` to template with `:group-id="id"` and `@close="onKeyDialogClose"`, extend `onOpenKey()` to handle `type.toLowerCase() === 'hash'` by calling `hashKeyDialog.value?.open(key)`. Note: URL query param update (`router.replace`) is deferred to US5 (T043); `onKeyDialogClose` is safe to call even without query params set.
- [ ] T019 [US1] Run `npm run build` and `npm run type-check` to verify compilation with no errors

**Checkpoint**: Hash key details dialog opens from Keys tab, displays field-value table with pagination, handles loading/error/empty states. US1 is fully functional and independently testable.

---

## Phase 4: User Story 2 — Edit Hash Field Values (Priority: P2)

**Goal**: Click on a field value in the hash details dialog to edit it inline with CodeMirror, save changes per-field to Redis.

**Independent Test**: Open hash key → click a field value → CodeMirror editor appears → modify value → click Save → value persisted. Press Escape → original value restored.

**Acceptance**: spec.md scenarios 2.1–2.5, FR-004–006, SC-002, SC-009

### Tests for User Story 2

- [ ] T020 [P] [US2] Create component test for inline edit activation in `tests/component/HashKeyDetailDialog.test.ts` — click a field value row, verify CodeMirror editor appears with field's current value
- [ ] T021 [P] [US2] Create component test for edit save in `tests/component/HashKeyDetailDialog.test.ts` — activate edit, change value, click Save, verify `createHashKey` API called with correct field name and new value
- [ ] T022 [P] [US2] Create component test for edit cancel in `tests/component/HashKeyDetailDialog.test.ts` — activate edit, change value, click Cancel, verify original value restored without API call

### Implementation for User Story 2

- [ ] T023 [US2] Implement click-to-edit mode in `src/components/HashKeyDetailDialog.vue` — clicking a field value row sets `editingField` to that field name and `editValue` to the field's current value; row expands to show CodeMirror editor (vue-codemirror) with the value. Only one field editable at a time. FR-004. Per research.md R2.
- [ ] T024 [US2] Implement per-field Save action in `src/components/HashKeyDetailDialog.vue` — Save button calls `createHashKey()` with `{ groupId, key, fields: { [editingField]: editValue } }`, on success updates the field in the `fields` array and exits edit mode, on failure shows `saveError` alert and remains in edit mode. FR-005. SC-002.
- [ ] T025 [US2] Implement per-field Cancel action in `src/components/HashKeyDetailDialog.vue` — Cancel button and Escape key restore `editValue` to original, clear `editingField`, exit edit mode without API call. FR-004 scenario 2.3.
- [ ] T026 [US2] Implement field value validation in `src/components/HashKeyDetailDialog.vue` — prevent saving empty values (show inline validation message "Value cannot be empty"), disable Save button when value is empty. FR-006, SC-009.
- [ ] T027 [US2] Implement save error handling in `src/components/HashKeyDetailDialog.vue` — on API error, show `v-alert type="error" variant="tonal"` with error message, revert field value to original in the display. FR-017 scenario 2.5.

**Checkpoint**: Field values are editable inline with save/cancel/validation. US2 works independently on top of US1.

---

## Phase 5: User Story 3 — JSON Value Formatting (Priority: P3)

**Goal**: Automatically detect and format JSON values in hash fields with syntax highlighting in both read and edit modes.

**Independent Test**: Open hash key with JSON field values → JSON is automatically indented and syntax-highlighted. Non-JSON values display as plain text.

**Acceptance**: spec.md scenarios 3.1–3.5, FR-007–008, SC-003

### Tests for User Story 3

- [ ] T028 [P] [US3] Create component test for JSON detection in `tests/component/HashKeyDetailDialog.test.ts` — mock fields with JSON and non-JSON values, verify JSON values display formatted (indented) and non-JSON values display as-is
- [ ] T029 [P] [US3] Create component test for Format JSON button in `tests/component/HashKeyDetailDialog.test.ts` — activate edit on a JSON field, click Format JSON button, verify value is re-indented

### Implementation for User Story 3

- [ ] T030 [US3] Implement JSON detection and read-mode formatting in `src/components/HashKeyDetailDialog.vue` — add `formatJsonIfValid()` helper (reuse pattern from `StringKeyDetailDialog`), apply to field values on display in the table. JSON values shown with CodeMirror (read-only, with `json()` language extension) for syntax highlighting; plain text values shown as regular text. FR-007, FR-008.
- [ ] T031 [US3] Implement JSON-aware edit mode in `src/components/HashKeyDetailDialog.vue` — when editing a field, apply `json()` CodeMirror extension if value is valid JSON. Add per-field "Format JSON" button (visible only during edit) that calls `formatJsonIfValid()` on the current edit value. FR-008 scenario 3.5. Per research.md R7.
- [ ] T032 [US3] Handle invalid JSON gracefully in `src/components/HashKeyDetailDialog.vue` — values that look like JSON but fail `JSON.parse()` display as plain text without errors. No formatting attempted on non-JSON values. FR-007 scenario 3.3.

**Checkpoint**: JSON values auto-formatted with highlighting, plain text unaffected. US3 enhances US1 read mode and US2 edit mode.

---

## Phase 6: User Story 4 — Add and Delete Hash Fields (Priority: P4)

**Goal**: Add new fields to a hash and delete existing fields with confirmation, completing CRUD operations.

**Independent Test**: Open hash key → click Add Field → enter name/value → Save → field appears in list. Click delete icon → confirm → field removed.

**Acceptance**: spec.md scenarios 4.1–4.6, FR-009–011, SC-006, SC-007

### Tests for User Story 4

- [ ] T033 [P] [US4] Create unit test for `removeHashFields()` in `tests/unit/api/redisKeys.test.ts` — verify POST request body structure, URL, error handling
- [ ] T034 [P] [US4] Create component test for Add Field in `tests/component/HashKeyDetailDialog.test.ts` — click Add Field button, verify inline form appears with name and value inputs, enter values, click Save, verify `createHashKey` API called
- [ ] T035 [P] [US4] Create component test for Delete Field in `tests/component/HashKeyDetailDialog.test.ts` — click delete icon on a field, verify confirmation dialog appears, confirm, verify `removeHashFields` API called

### Implementation for User Story 4

- [ ] T036 [US4] Implement Add Field UI in `src/components/HashKeyDetailDialog.vue` — "Add Field" button in the card header area (next to field count). Clicking toggles `addingField` state, showing an inline form row at the top of the fields table with `v-text-field` for field name and value, plus Save/Cancel buttons. FR-009, FR-010. Per research.md R5.
- [ ] T037 [US4] Implement Add Field save logic in `src/components/HashKeyDetailDialog.vue` — Save calls `createHashKey()` with `{ groupId, key, fields: { [newFieldName]: newFieldValue } }`. On success, prepend new `HashFieldDto` to `fields` array, clear form, close add row. Validate: field name non-empty, value non-empty. SC-006.
- [ ] T038 [US4] Implement duplicate field name warning in `src/components/HashKeyDetailDialog.vue` — when adding a field, check `fields` array for existing field with same name. If found, show warning text "This will overwrite the existing value for '{fieldName}'" but allow save (Redis HSET overwrites). Per research.md R5. FR-010 scenario 4.3.
- [ ] T039 [US4] Implement Delete Field UI in `src/components/HashKeyDetailDialog.vue` — delete icon button (`mdi-delete-outline`, `color="error"`, `size="small"`) per field row in the Actions column. Clicking sets `deleteTarget` and shows confirmation dialog. Follow `RedisKeysExplorer` delete dialog pattern: `v-dialog max-width="440"` with `card-header-separated` and `card-footer-separated`. FR-011 scenario 4.4.
- [ ] T040 [US4] Implement Delete Field confirm logic in `src/components/HashKeyDetailDialog.vue` — Confirm button calls `removeHashFields({ groupId, key, fields: [deleteTarget] })`. On success, remove field from `fields` array and close dialog. On failure, show error alert and keep field in list. FR-011 scenarios 4.5–4.6. SC-007.

**Checkpoint**: Full CRUD for hash fields. US4 adds create/delete on top of US1 view and US2 edit.

---

## Phase 7: User Story 5 — Deep Linking to Hash Key Details (Priority: P5)

**Goal**: Browser URL updates when hash dialog opens, and navigating to a deep link URL auto-opens the dialog.

**Independent Test**: Open hash key → URL shows `?key=mykey&type=hash`. Copy URL → open in new tab → dialog auto-opens for that key.

**Acceptance**: spec.md scenarios 5.1–5.5, FR-012–013, SC-004

### Tests for User Story 5

- [ ] T041 [P] [US5] Create component test for URL update on dialog open in `tests/component/redis-id-page.test.ts` — simulate opening hash dialog, verify `router.replace` called with `{ key, type: 'hash' }` query params
- [ ] T042 [P] [US5] Create component test for deep link resolution in `tests/component/redis-id-page.test.ts` — mount page with `?key=testkey&type=hash` query params, verify `HashKeyDetailDialog.open('testkey')` is called after server loads

### Implementation for User Story 5

- [ ] T043 [US5] Update URL on hash dialog open in `src/pages/redis/[id].vue` — in the `onOpenKey()` handler for `type === 'hash'`, call `router.replace({ query: { ...route.query, key, type: 'hash' } })` to update the browser URL. FR-012. Per research.md R4.
- [ ] T044 [US5] Extend `checkDeepLink()` in `src/pages/redis/[id].vue` — add condition: if `queryType === 'hash'`, set `tab.value = 'keys'` and call `hashKeyDialog.value?.open(queryKey)` via `nextTick()`. FR-013. Handle non-existent keys via the dialog's existing error state (404 from API). Scenario 5.4.
- [ ] T045 [US5] Verify deep link with authentication redirect in `src/pages/redis/[id].vue` — no code change needed; existing auth guard in router preserves query params through login redirect. Verify scenario 5.5 manually or via Playwright.

**Checkpoint**: Deep links work for hash keys. URL is shareable and bookmarkable. US5 is independently testable.

---

## Phase 8: User Story 6 — Preserve List Context on Close (Priority: P6)

**Goal**: Keys tab scroll position, selection, and filter state preserved when opening and closing the hash dialog.

**Independent Test**: Scroll down in keys list → open hash dialog → close → list at same scroll position with same filter.

**Acceptance**: spec.md scenarios 6.1–6.4, FR-014–015, SC-005

### Implementation for User Story 6

- [ ] T046 [US6] Verify scroll preservation behavior in `src/components/HashKeyDetailDialog.vue` — per research.md R3, `v-dialog` renders as overlay and `RedisKeysExplorer` DOM is preserved. No code changes needed. Manually verify scenarios 6.1–6.4 and document in test results. FR-014.
- [ ] T047 [US6] Verify selected key highlighting after dialog close — FR-015 is satisfied by the existing DOM preservation (the clicked key's `<a>` retains focus/visited state). If visual highlighting is needed beyond default browser link styling, add a `selectedKey` ref to `RedisKeysExplorer` and apply a `bg-blue-lighten-5` class to the matching row. Verify manually. FR-015.
- [ ] T048 [US6] Create Playwright visual verification for scroll preservation — navigate to keys tab, search, scroll down, open hash dialog, close, take screenshot confirming scroll position preserved. Save to `.playwright-mcp/hash-scroll-preservation.png`. SC-005.

**Checkpoint**: List context preserved on dialog open/close. US6 confirmed working via v-dialog overlay behavior.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Build verification, edge case handling, and final quality checks

- [ ] T049 [P] Run `npm run build` to verify production build succeeds with all changes
- [ ] T050 [P] Run `npm run type-check` to verify TypeScript strict mode passes
- [ ] T051 [P] Run `npm run lint` to verify ESLint passes with no warnings
- [ ] T052 Handle edge case: key deleted while dialog is open in `src/components/HashKeyDetailDialog.vue` — if an edit/delete/load-more API call returns 404, show error "This key no longer exists" and disable further operations. Per spec Edge Cases.
- [ ] T053 Handle edge case: large field values in `src/components/HashKeyDetailDialog.vue` — in read-only table rows, truncate displayed values to 500 characters with "..." suffix. Clicking a truncated value or entering edit mode shows the full value in CodeMirror. For values exceeding 1 MB, show a warning "Value is very large (X MB). Editor performance may be affected." (matching `StringKeyDetailDialog` large value pattern). Per spec Edge Cases.
- [ ] T054 Handle edge case: rapid key navigation in `src/components/HashKeyDetailDialog.vue` — use `AbortController` to cancel pending fetch requests when `open()` is called with a new key before the previous load completes. Per spec Edge Cases.
- [ ] T055 Handle edge case: close during in-flight save in `src/components/HashKeyDetailDialog.vue` — if `saving` or `deleting` is true when the user clicks Cancel/X/Escape, let the in-flight API call complete in the background (do not abort it). The dialog closes immediately; success/failure of the background operation is silently ignored. Per spec Edge Cases.
- [ ] T056 Playwright visual smoke test — navigate to hash key details at `http://localhost:3000`, capture screenshots for: dialog loading, dialog ready with fields, dialog empty state, dialog error state, inline editing, add field form, delete confirmation. Save to `.playwright-mcp/` directory.
- [ ] T057 Run full test suite with `npx vitest run` and verify all tests pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T001–T003 (API interfaces and functions)
- **US1 (Phase 3)**: Depends on Phase 1 + Phase 2 — BLOCKS all other stories
- **US2 (Phase 4)**: Depends on US1 (dialog must exist to add editing)
- **US3 (Phase 5)**: Depends on US1 (read-mode formatting) and US2 (edit-mode formatting)
- **US4 (Phase 6)**: Depends on US1 (dialog must exist to add/delete fields)
- **US5 (Phase 7)**: Depends on US1 (dialog must be registered in page to deep link)
- **US6 (Phase 8)**: Depends on US1 (dialog must open/close to verify preservation)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Foundational prerequisite — all other stories depend on this
- **US2 (P2)**: Depends on US1 only
- **US3 (P3)**: Depends on US1 + US2 (enhances both read and edit modes)
- **US4 (P4)**: Depends on US1 only — can run in parallel with US2
- **US5 (P5)**: Depends on US1 only — can run in parallel with US2, US4
- **US6 (P6)**: Depends on US1 only — can run in parallel with US2, US4, US5

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- API functions before component logic
- Component scaffold before feature-specific behavior
- Core implementation before error handling
- Story complete before moving to next priority

### Parallel Opportunities

- T005–T009 (US1 tests) can all run in parallel
- T020–T022 (US2 tests) can all run in parallel
- T028–T029 (US3 tests) can all run in parallel
- T033–T035 (US4 tests) can all run in parallel
- T041–T042 (US5 tests) can all run in parallel
- After US1 completes: US2, US4, US5, US6 can start in parallel
- T049 + T050 + T051 (build checks) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests in parallel:
Task: T005 "Unit test for getHashFields()"
Task: T006 "Component test for loading state"
Task: T007 "Component test for error state"
Task: T008 "Component test for empty state"
Task: T009 "Component test for ready state"

# Then implement sequentially:
Task: T010 "Create dialog scaffold"
Task: T011 "Implement loading state"
Task: T012 "Implement error state"
Task: T013 "Implement ready state"
Task: T014 "Implement fields table"
# ... etc
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (API client additions)
2. Complete Phase 2: Foundational (make hash keys clickable)
3. Complete Phase 3: User Story 1 (view hash key details)
4. **STOP and VALIDATE**: Open a hash key from Keys tab, verify field table displays
5. Deploy/demo if ready — read-only hash viewer is immediately useful

### Incremental Delivery

1. Setup + Foundational → API ready, hash keys clickable
2. Add US1 → View hash details → Deploy/Demo (MVP!)
3. Add US2 → Edit field values → Deploy/Demo
4. Add US3 → JSON formatting → Deploy/Demo
5. Add US4 → Add/delete fields → Deploy/Demo (full CRUD)
6. Add US5 → Deep linking → Deploy/Demo
7. Add US6 → Verify scroll preservation → Deploy/Demo
8. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Existing `createHashKey()` from `src/api/redisKeys.ts` is reused for both add and edit — no new "set" function needed
