# Data Model: String Key Details View

**Feature**: 009-string-key-details
**Date**: 2026-03-25

## Entities

### StringKeyDetail (Frontend view model)

Composite view model assembled from two API responses on the detail page.

| Field | Type | Source | Editable | Notes |
|-------|------|--------|----------|-------|
| `key` | `string` | Route query param | No | Redis key name; identifier |
| `type` | `string` | `GET /api/redis-keys/{key}/metadata` → `metadata.type` | No | Always "string" for this page |
| `value` | `string \| null` | `GET /api/redis/data/strings` → `value` | Yes | Raw string value; may be valid JSON |
| `ttlMilliseconds` | `number \| null` | `GET /api/redis-keys/{key}/metadata` → `metadata.ttlMilliseconds` | Yes (via TtlPicker) | `null` = no expiration |
| `groupId` | `string` (UUID) | Route path param `:id` | No | Server group context |

### Derived/Computed Properties

| Property | Type | Derivation |
|----------|------|------------|
| `isJson` | `boolean` | `JSON.parse(value)` succeeds without error |
| `formattedValue` | `string` | If `isJson`: `JSON.stringify(JSON.parse(value), null, 2)`, else: raw `value` |
| `ttlTimespan` | `string \| null` | Convert `ttlMilliseconds` to .NET TimeSpan format for TtlPicker and API save |
| `isDirty` | `boolean` | `currentValue !== originalValue \|\| currentTtl !== originalTtl` |

### SetStringRequest (API save payload)

Used when saving edits via `POST /api/redis/data/strings`.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `groupId` | `string` (UUID) | Yes | Server group identifier |
| `key` | `string` | Yes | Redis key name (not editable, sent as-is) |
| `value` | `string` | Yes | The edited string value |
| `ttl` | `string \| null` | Yes | .NET TimeSpan format (`[d.]hh:mm:ss`) or `null` for no expiration |
| `condition` | `number` | No | `0` = Always (default for updates) |

### GetStringQueryResult (API read response)

| Field | Type | Required |
|-------|------|----------|
| `value` | `string \| null` | Yes |

### RedisKeyMetadataDto (API metadata response, nested in GetKeyMetadataQueryResult)

| Field | Type | Required |
|-------|------|----------|
| `type` | `string` | Yes |
| `ttlMilliseconds` | `number \| null` | Yes |

## State Transitions

```
[Page Load]
  → Loading (fetching value + metadata in parallel)
  → Success (display fields, Save disabled)
  → Error (key not found / network failure → error alert + Retry)

[User Edits Value or TTL]
  → Dirty (Save enabled)

[User Clicks Save]
  → Saving (Save button loading)
  → Save Success (reset dirty state, Save disabled, update original values)
  → Save Error (show error alert, Save remains enabled)

[User Clicks Cancel]
  → Navigate back (router.back() or push to server page)
```

## Validation Rules

- `key`: Read-only, cannot be changed on this page.
- `type`: Read-only, always "string".
- `value`: Any string accepted (empty string is valid). JSON formatting is display-only, not a validation constraint.
- `ttl`: Must be a valid TimeSpan or null. Validated by TtlPicker component (hours 0-23, minutes 0-59, seconds 0-59).

## Relationships

- **StringKeyDetail** belongs to a **ServerGroup** (identified by `groupId`).
- The detail page is reached from **RedisKeysExplorer** (keys list), which provides the `groupId` and `key` as navigation parameters.
- The save operation reuses the existing `createStringKey` API function (which calls `POST /api/redis/data/strings` — same endpoint for create and update).
