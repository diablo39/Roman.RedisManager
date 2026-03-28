# Research: Hash Key Details Viewer

**Feature**: 010-hash-key-details | **Date**: 2026-03-28

## Research Tasks & Findings

### R1: Hash Field Pagination Strategy

**Task**: Determine how to load and display up to 1000+ hash fields efficiently using the cursor-based `GET /api/redis/data/hashes` endpoint.

**Decision**: Load-more pagination with cursor accumulation (matching `RedisKeysExplorer` pattern)

**Rationale**: The backend uses HSCAN-style cursor pagination (`cursor` + `pageSize` params, returns `fields[]`, `cursor`, `hasMoreResults`). The frontend already implements this exact pattern in `RedisKeysExplorer` for key listing — accumulate results into an array, show a "Load More" button when `hasMoreResults` is true, and pass the returned cursor on the next request. This pattern is proven, simple, and handles the SC-008 requirement (1000 fields < 3s) because only one page loads at a time (default 100 fields per page).

**Alternatives considered**:
- Virtual scrolling with `v-virtual-scroll`: Overkill for typical hash sizes (1-100 fields). Adds complexity without UX benefit. Could be a future enhancement for 10,000+ field hashes.
- Server-side full load (no pagination): Would violate performance budget for large hashes and the backend doesn't expose an unpaginated HGETALL endpoint.

---

### R2: Inline Editing UX Pattern

**Task**: Determine the best approach for editing hash field values within the dialog.

**Decision**: Click-to-edit with CodeMirror per field value, batch save for all changes

**Rationale**: The `StringKeyDetailDialog` uses a single Save button that persists all changes (value + TTL) at once. The hash dialog follows this same pattern — users make multiple edits locally (modify values, add fields, mark fields for deletion), then click Save to persist everything. This is safer (changes can be discarded by cancelling) and more efficient (fewer API calls). The backend supports this well: `POST /api/redis/data/hashes` accepts `fields: Record<string, string>` for all upserts, and `POST /api/redis/data/hashes/remove` accepts `fields: string[]` for all deletions. Dirty state is tracked by comparing current state against the original loaded state.

**Alternatives considered**:
- Per-field save (save each edit immediately): Simpler state management but worse UX — no way to undo, more network calls, inconsistent with `StringKeyDetailDialog` pattern.
- Full-dialog CodeMirror for selected field (modal-in-modal): Disruptive context switch, poor UX for quick edits.
- Inline `v-text-field` for simple values, CodeMirror only for JSON: Two different editing experiences is inconsistent. CodeMirror handles both plain text and JSON well.

---

### R3: Scroll Position Preservation (US-6)

**Task**: Determine how to preserve Keys tab scroll position and selection when opening/closing the hash key details dialog.

**Decision**: No special implementation needed — dialog overlay preserves DOM state

**Rationale**: The hash key details dialog uses `v-dialog`, which renders as a modal overlay on top of the existing page. The `RedisKeysExplorer` component and its scroll position remain mounted in the DOM while the dialog is open. When the dialog closes, the table is exactly where the user left it. This is the same behavior already working for `StringKeyDetailDialog`. The `v-window-item` for the keys tab also preserves its DOM subtree when switching tabs.

The only case requiring attention is deep link navigation: when a user arrives via URL with `?key=...&type=hash`, the keys tab may not have been searched yet. In this case, the dialog opens over the initial "Search for keys" instructional state, which is acceptable — the user came specifically for that key.

**Alternatives considered**:
- Saving scroll position to Pinia store: Unnecessary since the DOM is never unmounted.
- Using `keep-alive` on router views: Already unnecessary for the same reason.

---

### R4: Deep Linking Strategy (US-5)

**Task**: Determine URL structure and encoding for hash key deep links.

**Decision**: Extend existing query parameter pattern: `?key=<encoded-key>&type=hash&tab=keys`

**Rationale**: The existing deep linking in `[id].vue` already uses query params `?key=...&type=string`. Extending this to `type=hash` follows the established convention. Vue Router handles URL encoding automatically. The `checkDeepLink()` function needs a simple extension to check for `type === 'hash'` and open the `HashKeyDetailDialog` instead.

Key names with special characters (colons, unicode, spaces) are handled by standard URL encoding via `encodeURIComponent`/`decodeURIComponent`, which Vue Router applies transparently to query parameters.

**Alternatives considered**:
- Path-based routes (`/redis/:id/hash/:keyName`): Would require new route definitions and break the existing tab-based page structure. The hash detail is a dialog, not a separate page.
- Hash fragment (`#hash:keyName`): Non-standard, harder to parse, doesn't compose with existing query params.

---

### R5: Add/Delete Field UX Pattern (US-4)

**Task**: Determine the UX for adding new fields and deleting existing fields.

**Decision**:
- **Add**: "Add Field" button opens an inline form row at the top of the field list. New fields are added to local state immediately and included in the batch save.
- **Delete**: Delete icon button per row marks the field for deletion (visual strikethrough + muted styling). An undo button restores it. Actual deletion happens on batch Save via `removeHashFields()`.

**Rationale**: Batch save means add/delete are local operations until Save. This lets users undo mistakes (undelete a field, remove a just-added field) before committing. The visual strikethrough pattern is common in batch-edit UIs and clearly communicates pending state.

For duplicate field name detection (FR-003/acceptance 4.3): check against loaded fields client-side. If the field name exists, show a warning "This will overwrite the existing value" but allow it, since Redis HSET naturally overwrites.

**Alternatives considered**:
- Separate "Add Field" dialog: Adds unnecessary navigation for a simple two-input operation.
- Swipe-to-delete: Not a desktop pattern; Vuetify doesn't support this natively.

---

### R6: Backend API Integration Details

**Task**: Confirm all required backend endpoints exist and document their contracts.

**Decision**: All three required endpoints exist and are sufficient.

**Findings**:

| Operation | Method | Endpoint | Request | Response |
|-----------|--------|----------|---------|----------|
| Get fields | GET | `/api/redis/data/hashes?groupId=X&key=Y&cursor=0&pageSize=100` | Query params | `{ fields: [{field, value}], cursor: number, hasMoreResults: boolean }` |
| Set fields | POST | `/api/redis/data/hashes` | `{ groupId, key, fields: Record<string,string>, ttl? }` | `{ success: boolean }` |
| Delete fields | POST | `/api/redis/data/hashes/remove` | `{ groupId, key, fields: string[] }` | `{ removedCount: number }` |
| Get metadata | GET | `/api/redis-keys/{key}/metadata?groupId=X` | Query params | `{ metadata: { type, ttlMilliseconds } }` |

All endpoints require authentication. Set/Remove require `Editor` policy; Get requires `Reader` policy.

**No new backend work is needed.** The frontend needs three new API client functions: `getHashFields()`, `setHashField()` (wrapper around `createHashKey` for single field), and `removeHashFields()`.

---

### R7: JSON Formatting in Hash Context

**Task**: Determine how to apply JSON detection and formatting for hash field values.

**Decision**: Reuse the same `formatJsonIfValid()` pattern from `StringKeyDetailDialog`. Apply JSON detection per field value. In read mode, display formatted JSON with syntax highlighting via CodeMirror (read-only). In edit mode, use CodeMirror with JSON language extension when content is valid JSON.

**Rationale**: The existing pattern works well and users expect consistent behavior between string values and hash field values. CodeMirror is already a dependency, so no bundle size increase.

**Note**: The "Format JSON" button should appear per-field when editing, not globally, since each field value is independent.
