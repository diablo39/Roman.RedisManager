# Tasks: String Key Details View

**Input**: Design documents from `/specs/009-string-key-details/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per FR-012 and Constitution Principle II (Testing Is Mandatory). Test tasks are defined per user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Web app**: `frontend/src/`, `frontend/tests/`
- All paths relative to repository root (`Roman.RedisManager/`)

---

## Phase 1: Setup

**Purpose**: Install new dependency and prepare project structure

- [x] T001 Install vue-codemirror and @codemirror/lang-json dependencies in frontend/package.json
- [x] T002 Create page directory structure at frontend/src/pages/redis/[id]/ for nested routes

**Checkpoint**: Dependencies installed, directory structure ready

---

## Phase 2: Foundational (API Client)

**Purpose**: API functions that ALL user stories depend on — MUST complete before any story work

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 [P] Add `getStringKeyValue(groupId, key, signal?)` function to frontend/src/api/redisKeys.ts — calls `GET /api/redis/data/strings?groupId=...&key=...`, returns `GetStringQueryResult { value: string | null }`
- [x] T004 [P] Add `getKeyMetadata(key, groupId, signal?)` function to frontend/src/api/redisKeys.ts — calls `GET /api/redis-keys/{key}/metadata?groupId=...`, returns `GetKeyMetadataQueryResult { metadata: { type, ttlMilliseconds } }`
- [x] T005 [P] Add TypeScript interfaces `GetStringQueryResult`, `GetKeyMetadataQueryResult`, and `RedisKeyMetadataDto` to frontend/src/api/redisKeys.ts

**Checkpoint**: Foundation ready — API functions available for component development

---

## Phase 3: User Story 1 — View String Key Details from Keys Tab (Priority: P1) MVP

**Goal**: User clicks a string key name in the Keys tab and sees a detail page with key name, type, value, and TTL fetched from the API.

**Independent Test**: Search for a string key in Keys tab → click its name → detail page loads with all four fields populated correctly.

### Tests for User Story 1

- [x] T006 [P] [US1] Create component test for page load and field display in frontend/tests/component/StringKeyDetail.spec.ts — test: component renders loading skeleton on mount, displays key name (read-only), type chip, value field, and TTL picker after successful API fetch; displays error alert on API failure with Retry button

### Implementation for User Story 1

- [x] T007 [US1] Create StringKeyDetail.vue component skeleton in frontend/src/components/StringKeyDetail.vue — props: `groupId: string`, `keyName: string`; emit: `cancel`; scaffold loading/error/ready states per UI contracts
- [x] T008 [US1] Implement data fetching in StringKeyDetail.vue — call `getStringKeyValue()` and `getKeyMetadata()` in parallel on mount using AbortController; populate key, type, value, ttlMilliseconds refs; handle loading and error states with skeleton loaders and v-alert error pattern
- [x] T009 [US1] Implement read-only fields in StringKeyDetail.vue — key name as disabled `v-text-field` with monospace font, type as color-coded `v-chip` (String = blue, using same `typeColor()` pattern from RedisKeysExplorer)
- [x] T010 [US1] Implement value display in StringKeyDetail.vue — integrate vue-codemirror `<Codemirror>` component with `v-model` binding for the value field; configure as plain text editor (no JSON language extension yet — that's US2)
- [x] T011 [US1] Implement TTL display in StringKeyDetail.vue — convert `ttlMilliseconds` to .NET TimeSpan format string and bind to `<TtlPicker v-model="ttlTimespan" />`; handle null TTL (no expiration)
- [x] T012 [US1] Create string key detail page at frontend/src/pages/redis/[id]/string.vue — extract `id` from route params and `key` from route query; render `<StringKeyDetail :group-id="id" :key-name="key" />`; add route meta (layout: default, title: 'String Key Details')
- [x] T013 [US1] Modify RedisKeysExplorer.vue in frontend/src/components/RedisKeysExplorer.vue — make key name column clickable (for string-type keys); on click, `router.push({ path: '/redis/${groupId}/string', query: { key: item.key, from: 'keys' } })` (include `from: 'keys'` query param for US5 cancel detection)

**Checkpoint**: User Story 1 fully functional — clicking a string key navigates to detail page showing all fields. Save/edit not yet wired.

---

## Phase 4: User Story 2 — JSON Value Formatting and Syntax Highlighting (Priority: P1)

**Goal**: When the string value is valid JSON, it is auto-formatted (pretty-printed) and displayed with syntax highlighting in the CodeMirror editor.

**Independent Test**: Create a string key with `{"name":"test","count":42}` → navigate to detail → value appears formatted and syntax-highlighted. Create a plain text key → value appears as-is with no errors.

### Tests for User Story 2

- [x] T014 [P] [US2] Create unit test for JSON detection helper (isJson, formatJsonIfValid) in frontend/tests/unit/jsonDetection.spec.ts — test valid JSON, invalid JSON, empty string, null, nested objects, arrays, and large JSON payloads

### Implementation for User Story 2

- [x] T015 [US2] Implement JSON detection logic in StringKeyDetail.vue — add `isJson` computed property using `JSON.parse()` try/catch; add `formattedValue` computed that pretty-prints JSON on initial load; conditionally apply `@codemirror/lang-json` `json()` extension to the CodeMirror editor when `isJson` is true

**Checkpoint**: User Story 2 complete — JSON values are pretty-printed and syntax-highlighted; plain text values display normally

---

## Phase 5: User Story 3 — Edit String Key Details (Priority: P1)

**Goal**: User can edit the value and TTL fields; Save button enables on change and persists edits via the API.

**Independent Test**: Navigate to a string key detail → modify value → Save button enables → click Save → verify value persisted → Save button disables again.

### Tests for User Story 3

- [x] T016 [P] [US3] Create component test for dirty state tracking and save flow in frontend/tests/component/StringKeyDetail.spec.ts — test: Save disabled on load, enabled after value change, enabled after TTL change, disabled after successful save, error alert on failed save

### Implementation for User Story 3

- [x] T017 [US3] Implement dirty state tracking in StringKeyDetail.vue — store original value and TTL on load; add `isDirty` computed comparing current vs original; bind `:disabled="!isDirty"` on Save button
- [x] T018 [US3] Implement save action in StringKeyDetail.vue — on Save click, call `createStringKey({ groupId, key, value, ttl, condition: 0 })` from existing API (note: `createStringKey` is a create-or-update operation — the POST endpoint handles both new and existing keys); show loading state on button; on success, update original values and reset dirty state; on failure, show `v-alert type="error" variant="tonal"` with error message

**Checkpoint**: User Story 3 complete — editing and saving works end-to-end. All P1 stories done.

---

## Phase 6: User Story 4 — Shareable URL / Deep Linking (Priority: P2)

**Goal**: The page URL contains groupId and key name so it can be shared and opened directly after authentication.

**Independent Test**: Copy the detail page URL → open in new browser window → authenticate → correct key detail page loads.

### Tests for User Story 4

- [x] T019 [P] [US4] Create unit test for route parameter extraction in frontend/tests/unit/stringKeyRoute.spec.ts — test: correct `id` and `key` extracted from route; missing `key` query param produces error state; special characters in key name are correctly decoded from URL

### Implementation for User Story 4

- [x] T020 [US4] Verify deep linking works in frontend/src/pages/redis/[id]/string.vue — ensure page reads `id` from path params and `key` from query params on mount (not just from navigation); handle missing `key` query param with error state; ensure auth guard redirects to login then back to the detail URL after authentication

**Checkpoint**: User Story 4 complete — URLs are shareable and work after authentication

---

## Phase 7: User Story 5 — Cancel Navigation (Priority: P2)

**Goal**: Cancel button returns user to the keys list with previous search preserved, or to the server page if arrived via direct URL.

**Independent Test**: Navigate from keys list to detail → click Cancel → returned to keys list with search results visible. Open detail via direct URL → click Cancel → redirected to server page Keys tab.

### Tests for User Story 5

- [x] T021 [P] [US5] Create unit test for cancel navigation logic in frontend/tests/unit/cancelNavigation.spec.ts — test: returns `router.back()` when `from=keys` query param is present; returns `router.push('/redis/${id}')` when `from` param is absent (direct URL entry)

### Implementation for User Story 5

- [x] T022 [US5] Implement cancel navigation in frontend/src/pages/redis/[id]/string.vue — handle `@cancel` emit from StringKeyDetail; detect if user came from within the app via `from: 'keys'` query param (already added in T013); if yes, call `router.back()`; if no, call `router.push(`/redis/${id}`)` to navigate to server page with Keys tab

**Checkpoint**: User Story 5 complete — Cancel navigation works from both entry points

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Quality gates, edge cases, performance validation, and accessibility across all stories

- [x] T023 Handle edge case: key not found on load in StringKeyDetail.vue — display error state with "Key not found" message and a "Go Back" button
- [x] T024 Handle edge case: key name with special characters — verify URL encoding/decoding works correctly for keys containing `/`, `:`, spaces, `{`, `}` in the route query parameter
- [x] T025 Handle edge case: large values (> 1 MB) in StringKeyDetail.vue — add a warning banner when the value exceeds 1 MB indicating potential performance impact; ensure CodeMirror does not freeze the browser for large payloads
- [x] T026 Validate performance budgets — verify page load with value display completes in < 1 second, JSON formatting completes in < 100 ms for typical values (< 100 KB), and bundle size increase from vue-codemirror is < 100 KB gzipped (per plan.md performance goals)
- [x] T027 Validate accessibility — verify keyboard navigation through all form fields (Tab order: key → type → value editor → TTL picker → Cancel → Save); verify ARIA labels on read-only fields and editor; verify screen reader announces field states (disabled, loading)
- [x] T028 Run `npm run type-check` and fix any TypeScript errors across all modified/new files
- [x] T029 Run `npm run lint` and fix any ESLint issues across all modified/new files
- [x] T030 Run `npm run build` and verify production build succeeds with no errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (npm install) — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — core page and component
- **US2 (Phase 4)**: Depends on Phase 3 (needs CodeMirror integration from US1)
- **US3 (Phase 5)**: Depends on Phase 3 (needs form fields from US1); can run in parallel with US2
- **US4 (Phase 6)**: Depends on Phase 3 (needs page to exist); can run in parallel with US2/US3
- **US5 (Phase 7)**: Depends on Phase 3 (needs page with cancel emit and `from` param from T013); can run in parallel with US2/US3/US4
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Foundation only — no story dependencies. MVP.
- **US2 (P1)**: Depends on US1 (CodeMirror must be integrated first)
- **US3 (P1)**: Depends on US1 (form fields must exist); independent of US2
- **US4 (P2)**: Depends on US1 (page must exist); independent of US2/US3
- **US5 (P2)**: Depends on US1 (page must exist with `from` query param from T013)

### Within Each User Story

- Tests written first (where applicable)
- Component skeleton → data fetching → field rendering → integration
- Commit after each task or logical group

### Parallel Opportunities

- T003, T004, T005 can all run in parallel (separate functions/interfaces added to same file)
- T006 (US1 test) can run in parallel with T007 (US1 skeleton)
- T014 (US2 test) can run in parallel with T016 (US3 test)
- US3, US4, US5 can all run in parallel after US1 completes
- T028, T029, T030 (polish) are sequential (fix before build)

---

## Parallel Example: After Phase 3 Completes

```text
# These can run in parallel after US1 is done:
Developer A: T014-T015 [US2] JSON detection tests and CodeMirror language extension
Developer B: T016-T018 [US3] Dirty state tracking and save action
Developer C: T019-T020 [US4] Deep link tests + T021-T022 [US5] Cancel navigation
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (install deps)
2. Complete Phase 2: Foundational (API functions)
3. Complete Phase 3: User Story 1 (view detail page)
4. **STOP and VALIDATE**: Navigate to a string key → verify detail page loads with all fields
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → API ready
2. US1 → View detail page (MVP!)
3. US2 → JSON formatting (enhances US1 display)
4. US3 → Edit and save (full edit capability)
5. US4 + US5 → Deep linking + Cancel navigation (polish)
6. Polish → Quality gates, edge cases, performance, accessibility

### Sequential Solo Strategy (Recommended)

Since this is a frontend-only feature with a single developer:

1. Phase 1-2: Setup + API functions (T001-T005)
2. Phase 3: US1 — build the full component and page (T006-T013)
3. Phase 4: US2 — add JSON detection/formatting (T014-T015)
4. Phase 5: US3 — add edit/save capability (T016-T018)
5. Phase 6-7: US4+US5 — deep linking and cancel (T019-T022)
6. Phase 8: Polish and quality gates (T023-T030)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- The existing `createStringKey` API function is reused for save — it is a create-or-update operation (POST endpoint handles both)
- TtlPicker component is reused as-is (no modifications needed)
- Large values threshold: 1 MB — values above this display a performance warning
