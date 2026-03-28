# Quickstart: Hash Key Details Viewer

**Feature**: 010-hash-key-details | **Date**: 2026-03-28

## Implementation Order

### Step 1: API Client Functions

**File**: `src/api/redisKeys.ts`

Add three new items:
1. `HashFieldDto` and `GetHashFieldsResult` interfaces
2. `RemoveHashFieldsRequest` and `RemoveHashFieldsResult` interfaces
3. `getHashFields()` function — GET with query params, cursor pagination
4. `removeHashFields()` function — POST with JSON body

Pattern: Follow existing `getStringKeyValue()` for GET and `createHashKey()` for POST.

### Step 2: HashKeyDetailDialog Component

**File**: `src/components/HashKeyDetailDialog.vue` (NEW)

Structure (following `StringKeyDetailDialog` pattern):
1. `v-dialog` with `max-width="900"` (wider than string dialog for two-column field table)
2. Card header: `mdi-code-braces` icon, "Hash Key Details", close button
3. Loading state: `v-skeleton-loader`
4. Error state: `v-alert type="error"` with Retry
5. Ready state:
   - Key name (read-only `v-text-field`)
   - Type chip (`color="orange"`, "hash")
   - Field count display
   - Add Field button + inline form (field name + value inputs)
   - Fields table: `v-table density="compact" hover`
     - Columns: Field Name | Value | Actions (edit/delete icons)
     - Click value → CodeMirror editor appears inline
     - Per-field Save/Cancel buttons when editing
   - Load More button (when `hasMoreResults`)
   - TTL section (`TtlPicker`)
   - Empty state for 0 fields
6. Footer: Cancel button only (saves are per-field, not whole-dialog)

### Step 3: Make Hash Keys Clickable

**File**: `src/components/RedisKeysExplorer.vue`

Change the key name column to make hash keys clickable (same as string keys):
```vue
<a v-if="['string', 'hash'].includes(item.type.toLowerCase())" ...>
```

### Step 4: Page Integration & Deep Linking

**File**: `src/pages/redis/[id].vue`

1. Import and register `HashKeyDetailDialog`
2. Add template ref: `const hashKeyDialog = ref()`
3. Extend `onOpenKey()` to handle `type === 'hash'`
4. Extend `checkDeepLink()` to handle `type === 'hash'`
5. Add `@close` handler to clear query params

### Step 5: Tests

- **Unit**: API functions (mock fetch, verify URL construction, error handling)
- **Component**: HashKeyDetailDialog (mount with mock props, verify states)
- **Visual**: Playwright screenshots for dialog states (loading, ready, empty, error, editing)

## Key Decisions

| Decision | Choice | Reference |
|----------|--------|-----------|
| Dialog width | 900px (wider for field table) | Wider than StringKeyDetailDialog's 700px |
| Edit mode | Per-field inline CodeMirror | research.md R2 |
| Pagination | Cursor-based load-more | research.md R1 |
| Save granularity | Per-field (not whole dialog) | research.md R2 |
| Deep link format | `?key=X&type=hash` | research.md R4 |
| Delete confirmation | Per-field dialog | research.md R5 |

## Dev Commands

```bash
npm run dev          # Start dev server at http://localhost:3000
npm run build        # Verify compilation
npm run type-check   # TypeScript validation
npm run lint         # ESLint check
```
