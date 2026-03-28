# Data Model: Hash Key Details Viewer

**Feature**: 010-hash-key-details | **Date**: 2026-03-28

## Entities

### HashFieldDto

Represents a single field-value pair within a Redis hash key. Returned by the backend `GET /api/redis/data/hashes` endpoint.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `field` | `string` | The hash field name | Non-empty, unique within a hash key |
| `value` | `string` | The hash field value | May be empty string; may contain JSON, plain text, or binary-safe string |

**Source**: Backend DTO `HashFieldDto(string Field, string Value)`

---

### GetHashFieldsResult

Response from the paginated hash field fetch endpoint.

| Field | Type | Description |
|-------|------|-------------|
| `fields` | `HashFieldDto[]` | Array of field-value pairs for the current page |
| `cursor` | `number` | Cursor for fetching the next page (0 = no more pages) |
| `hasMoreResults` | `boolean` | Whether more fields exist beyond the current page |

**Source**: Backend DTO `GetHashFieldsQueryResult`

---

### RemoveHashFieldsRequest

Request body for deleting one or more hash fields.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `groupId` | `string` | Redis server group identifier | Required, non-empty UUID |
| `key` | `string` | The Redis hash key name | Required, non-empty |
| `fields` | `string[]` | Array of field names to remove | Required, at least one field |

**Source**: Backend DTO `RemoveHashFieldsRequest`

---

### RemoveHashFieldsResult

Response from the hash field removal endpoint.

| Field | Type | Description |
|-------|------|-------------|
| `removedCount` | `number` | Number of fields actually removed |

**Source**: Backend DTO `RemoveHashFieldsCommandResult`

---

### SetHashFieldsRequest (existing)

Already defined in `src/api/redisKeys.ts`. Used for both creating hash keys and updating individual field values.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `groupId` | `string` | Redis server group identifier | Required |
| `key` | `string` | The Redis hash key name | Required |
| `fields` | `Record<string, string>` | Map of field names to values | At least one field |
| `ttl` | `string \| null` | Optional TTL in .NET TimeSpan format | Optional |

---

### RedisKeyMetadataDto (existing)

Already defined in `src/api/redisKeys.ts`. Used to fetch key type and TTL.

| Field | Type | Description |
|-------|------|-------------|
| `type` | `string` | Redis data type (e.g., "hash") |
| `ttlMilliseconds` | `number \| null` | TTL in milliseconds, null if no expiry |

---

## Component State Model

### HashKeyDetailDialog Internal State

| State | Type | Initial | Description |
|-------|------|---------|-------------|
| `visible` | `boolean` | `false` | Dialog open/closed |
| `keyName` | `string` | `''` | Current hash key name |
| `loading` | `boolean` | `true` | Initial data fetch in progress |
| `loadError` | `string \| null` | `null` | Error from initial fetch |
| `fields` | `HashFieldDto[]` | `[]` | Accumulated hash fields from all pages |
| `cursor` | `number` | `0` | Current pagination cursor |
| `hasMoreFields` | `boolean` | `false` | More pages available |
| `loadingMore` | `boolean` | `false` | Loading next page |
| `editingField` | `string \| null` | `null` | Field name currently being edited |
| `editValue` | `string` | `''` | Current edit buffer for the field being edited |
| `saving` | `boolean` | `false` | Save operation in progress |
| `saveError` | `string \| null` | `null` | Error from save/delete operations |
| `addingField` | `boolean` | `false` | Add-field form is visible |
| `newFieldName` | `string` | `''` | New field name input |
| `newFieldValue` | `string` | `''` | New field value input |
| `ttl` | `string \| null` | `null` | Current TTL (from metadata) |
| `originalTtl` | `string \| null` | `null` | Original TTL for dirty tracking |
| `deleting` | `boolean` | `false` | Delete operation in progress |
| `deleteTarget` | `string \| null` | `null` | Field name pending deletion confirmation |

---

## State Transitions

### Dialog Lifecycle

```
Closed → open(key) → Loading → [Success] → Ready
                               → [Error]   → Error → retry → Loading
Ready → edit field → Editing → save → Ready
                              → cancel → Ready
Ready → add field → Adding → save → Ready
                            → cancel → Ready
Ready → delete field → Confirm → confirm → Ready
                               → cancel → Ready
Ready → load more → LoadingMore → Ready
Ready → close → Closed
```

### Field Edit Flow

```
ReadOnly → click value → EditMode (editingField = fieldName, editValue = field.value)
EditMode → Save button → saving=true → API HSET → update fields[] → ReadOnly
EditMode → Cancel/Escape → restore editValue → ReadOnly
EditMode → Save fails → show saveError → remain EditMode
```

### Deep Link Flow

```
Page load with ?key=X&type=hash → tab='keys' → wait for server load →
  HashKeyDetailDialog.open(X) → fetch fields → Ready
```

## Relationships

```
RedisKeysExplorer ──emits 'open-key'──→ [id].vue ──calls open()──→ HashKeyDetailDialog
                                                                        │
                                                                        ├── GET /api/redis/data/hashes (fields)
                                                                        ├── GET /api/redis-keys/{key}/metadata (TTL)
                                                                        ├── POST /api/redis/data/hashes (save field)
                                                                        └── POST /api/redis/data/hashes/remove (delete field)
```
