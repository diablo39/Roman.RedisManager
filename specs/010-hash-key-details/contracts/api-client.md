# API Client Contract: Hash Key Operations

**Feature**: 010-hash-key-details | **Date**: 2026-03-28
**File**: `src/api/redisKeys.ts` (additions to existing module)

## New TypeScript Interfaces

```typescript
export interface HashFieldDto {
  field: string
  value: string
}

export interface GetHashFieldsResult {
  fields: HashFieldDto[]
  cursor: number
  hasMoreResults: boolean
}

export interface RemoveHashFieldsRequest {
  groupId: string
  key: string
  fields: string[]
}

export interface RemoveHashFieldsResult {
  removedCount: number
}
```

## New API Functions

### getHashFields

Fetches hash fields with cursor-based pagination.

```typescript
export async function getHashFields(
  groupId: string,
  key: string,
  cursor?: number,
  pageSize?: number,
  signal?: AbortSignal,
): Promise<GetHashFieldsResult>
```

**Endpoint**: `GET /api/redis/data/hashes`
**Query params**: `groupId`, `key`, `cursor` (default 0), `pageSize` (default 100)
**Auth**: Reader policy
**Errors**: 400 (bad params), 404 (key not found), 409 (type mismatch)

---

### removeHashFields

Removes one or more fields from a hash key.

```typescript
export async function removeHashFields(
  request: RemoveHashFieldsRequest,
  signal?: AbortSignal,
): Promise<RemoveHashFieldsResult>
```

**Endpoint**: `POST /api/redis/data/hashes/remove`
**Body**: `RemoveHashFieldsRequest` as JSON
**Auth**: Editor policy
**Errors**: 400 (bad params), 404 (key not found)

---

### Existing: createHashKey (reused for single-field updates)

The existing `createHashKey(request: SetHashFieldsRequest)` function sends `POST /api/redis/data/hashes`. For editing a single field, call it with `fields: { [fieldName]: newValue }`. For setting TTL changes, include the `ttl` field.

No wrapper function needed — the existing function signature handles both creation and update.

## Component Contract: HashKeyDetailDialog

### Props

```typescript
defineProps<{
  groupId: string
}>()
```

### Emits

```typescript
defineEmits<{
  close: []
}>()
```

### Exposed Methods

```typescript
defineExpose({
  open: (key: string) => void
})
```

### Behavior Contract

| Action | API Call | Success | Failure |
|--------|----------|---------|---------|
| Open dialog | `getHashFields()` + `getKeyMetadata()` | Show field table | Show error alert with Retry |
| Load more | `getHashFields(cursor)` | Append to fields | Show error alert |
| Edit field | (local only) | Update field in working copy, mark dirty | — |
| Add field | (local only) | Prepend to working copy, mark dirty | — |
| Mark delete | (local only) | Add to `pendingDeletions` set, mark dirty | — |
| Save (batch) | `createHashKey({ fields: allUpserts })` + `removeHashFields({ fields: allDeletions })` | Reload fields from API, reset dirty | Show error, preserve changes for retry |
| Close/Cancel | — | Emit `close`, discard changes, clear query params | — |

## URL Query Parameter Contract

**Pattern**: `?key=<keyName>&type=hash&tab=keys`

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `key` | `string` | Yes | URL-encoded Redis key name |
| `type` | `'hash'` | Yes | Literal string identifying key type |
| `tab` | `'keys'` | Implicit | Set automatically when key param present |

**Set on**: Dialog open (via `router.replace`)
**Cleared on**: Dialog close (remove `key` and `type` from query)
**Deep link**: On page load, if `type=hash` and `key` present, auto-open `HashKeyDetailDialog`
