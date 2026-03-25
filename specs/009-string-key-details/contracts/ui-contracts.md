# UI Contracts: String Key Details View

**Feature**: 009-string-key-details
**Date**: 2026-03-25

## 1. Deep Link Contract

**URL pattern**: `/redis/:id?key=<keyName>&type=string`

| Parameter | Location | Type | Required | Description |
|-----------|----------|------|----------|-------------|
| `id` | Path (`:id`) | UUID string | Yes | Redis server group ID |
| `key` | Query (`?key=`) | URL-encoded string | Yes | Redis key name |
| `type` | Query (`&type=`) | string literal | Yes | Key type (currently only `string`) |

**Examples**:
- `/redis/a1b2c3d4-e5f6-7890-abcd-ef1234567890?key=user:profile:123&type=string`

**Behavior**:
- When URL contains `key` and `type=string` query params, the dialog auto-opens on page load
- When dialog opens, URL is updated with query params (without full navigation)
- When dialog closes, query params are removed from URL

---

## 2. Navigation Contract

### Inbound: Keys List → Dialog

**Trigger**: Click on key name in `RedisKeysExplorer` table (string-type keys only).

**Action**: Emit event to parent page, which opens the dialog and updates URL query params.

### Outbound: Close Dialog

**Logic**: Close the dialog, remove `key` and `type` query params from URL using `router.replace`.

---

## 3. API Contracts (consumed by this feature)

### GET String Value

**Endpoint**: `GET /api/redis/data/strings?groupId={groupId}&key={key}`
**Auth**: Bearer token, scope: Reader

**Response** (`GetStringQueryResult`):
```json
{ "value": "string or null" }
```

### GET Key Metadata

**Endpoint**: `GET /api/redis-keys/{key}/metadata?groupId={groupId}`
**Auth**: Bearer token, scope: Reader

**Response** (`GetKeyMetadataQueryResult`):
```json
{ "metadata": { "type": "string", "ttlMilliseconds": 300000 } }
```

### POST Set String Value (save)

**Endpoint**: `POST /api/redis/data/strings`
**Auth**: Bearer token, scope: Editor

**Request** (`SetStringRequest`):
```json
{ "groupId": "uuid", "key": "string", "value": "string", "ttl": "1.02:30:00 or null" }
```

---

## 4. Component Contract: StringKeyDetailDialog

**File**: `src/components/StringKeyDetailDialog.vue`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `groupId` | `string` | Yes | Redis server group UUID |

### Exposed Methods

| Method | Params | Description |
|--------|--------|-------------|
| `open(keyName: string)` | key name string | Opens dialog and fetches data for the given key |

### Emits

| Event | Payload | Description |
|-------|---------|-------------|
| `close` | none | Dialog was closed (Cancel, X button, or outside click) |

### States

| State | UI |
|-------|----|
| Closed | Dialog not visible |
| Loading | Dialog open with skeleton loaders |
| Error (load) | `v-alert type="error" variant="tonal"` + Retry button inside dialog |
| Ready | All fields displayed, Save disabled |
| Dirty | Save button enabled |
| Saving | Save button with `:loading="true"` |
| Save Error | `v-alert type="error" variant="tonal"` above form in dialog |
