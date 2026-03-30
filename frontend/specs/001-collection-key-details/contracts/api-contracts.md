# API Contracts: Collection Key Details

**Feature**: 001-collection-key-details
**Date**: 2026-03-28

These contracts document the backend API endpoints that the frontend will consume. All endpoints already exist in the backend.

---

## Sets

### Read Set Members

```
GET /api/redis/data/sets?groupId={groupId}&key={key}&cursor={cursor}&pageSize={pageSize}
```

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| groupId | string | yes | — | Server group ID |
| key | string | yes | — | Redis key name |
| cursor | number | no | 0 | Pagination cursor |
| pageSize | number | no | 100 | Members per page |

**Response** (200):
```json
{
  "members": ["value1", "value2"],
  "cursor": 42,
  "hasMoreResults": true
}
```

### Add Set Members

```
POST /api/redis/data/sets
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "members": ["value1", "value2"],
  "ttl": "1.00:00:00"
}
```

**Response** (200): `{ "success": true }`

### Remove Set Members

```
POST /api/redis/data/sets/remove
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "members": ["value1", "value2"]
}
```

**Response** (200): `{ "removedCount": 2 }`

---

## Lists

### Read List Range

```
GET /api/redis/data/lists?groupId={groupId}&key={key}&start={start}&stop={stop}
```

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| groupId | string | yes | — | Server group ID |
| key | string | yes | — | Redis key name |
| start | number | no | 0 | Start index (inclusive) |
| stop | number | no | -1 | Stop index (inclusive, -1 = end) |

**Response** (200):
```json
{
  "values": ["item1", "item2", "item3"]
}
```

### Push to List

```
POST /api/redis/data/lists
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "values": ["item1", "item2"],
  "direction": 1,
  "ttl": "1.00:00:00"
}
```

Direction: 0 = Left (LPUSH), 1 = Right (RPUSH, default)

**Response** (200): `{ "success": true }`

### Remove from List

```
POST /api/redis/data/lists/remove
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "value": "item1",
  "count": 1
}
```

Count: 0 = all occurrences, positive = first N from head, negative = first N from tail

**Response** (200): `{ "removedCount": 1 }`

---

## Sorted Sets

### Read Sorted Set Range

```
GET /api/redis/data/sorted-sets?groupId={groupId}&key={key}&start={start}&stop={stop}
```

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| groupId | string | yes | — | Server group ID |
| key | string | yes | — | Redis key name |
| start | number | no | 0 | Start index (inclusive) |
| stop | number | no | -1 | Stop index (inclusive, -1 = end) |

**Response** (200):
```json
{
  "entries": [
    { "member": "value1", "score": 1.0 },
    { "member": "value2", "score": 2.5 }
  ]
}
```

### Add to Sorted Set

```
POST /api/redis/data/sorted-sets
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "entries": [
    { "member": "value1", "score": 1.0 }
  ],
  "ttl": "1.00:00:00"
}
```

**Response** (200): `{ "success": true }`

### Remove from Sorted Set

```
POST /api/redis/data/sorted-sets/remove
Content-Type: application/json
```

**Body**:
```json
{
  "groupId": "uuid",
  "key": "keyname",
  "members": ["value1", "value2"]
}
```

**Response** (200): `{ "removedCount": 2 }`

---

## Shared Endpoints (already consumed)

### Key Metadata

```
GET /api/redis-keys/{key}/metadata?groupId={groupId}
```

**Response** (200):
```json
{
  "metadata": {
    "type": "set",
    "ttlMilliseconds": 86400000
  }
}
```

Used by all detail dialogs to fetch TTL information.

---

## URL Deep Linking Contract

The frontend uses URL query parameters for dialog state:

```
/redis/{serverId}?tab=keys&key={keyName}&type={keyType}
```

| Parameter | Values | Description |
|-----------|--------|-------------|
| tab | `keys` | Activates the Keys tab |
| key | string | Key name to open in detail dialog |
| type | `string`, `hash`, `set`, `list`, `zset` | Determines which dialog to open |
