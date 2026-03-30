# Data Model: Collection Key Details

**Feature**: 001-collection-key-details
**Date**: 2026-03-28

## Entities

### SetMemberDto

Represents a single member of a Redis set, as returned by the backend SSCAN endpoint.

| Field | Type | Description |
|-------|------|-------------|
| value | string | The member value |

**Relationships**: Belongs to a Redis key of type "set". Unique within the set.

**Validation**: Value must be a non-empty string.

---

### GetSetMembersResult

Response from the set members read endpoint (cursor-based pagination).

| Field | Type | Description |
|-------|------|-------------|
| members | string[] | Array of set member values |
| cursor | number | Cursor position for next page (0 = no more) |
| hasMoreResults | boolean | Whether more members are available |

---

### ListItemDto

Represents a single item in a Redis list, with its index position.

| Field | Type | Description |
|-------|------|-------------|
| index | number | Zero-based position in the list |
| value | string | The item value |

**Note**: Index is computed client-side from the range request parameters, not returned by the backend.

**Relationships**: Belongs to a Redis key of type "list". Order is significant. Duplicates allowed.

**Validation**: Value must be a non-empty string.

---

### GetListRangeResult

Response from the list range read endpoint (index-based pagination).

| Field | Type | Description |
|-------|------|-------------|
| values | string[] | Array of list item values in order |

**Note**: The backend returns only values. The frontend computes indices from the `start` parameter of the request.

---

### SortedSetEntryDto

Represents a member-score pair in a Redis sorted set.

| Field | Type | Description |
|-------|------|-------------|
| member | string | The entry's member value |
| score | number | The entry's numeric score (determines sort order) |

**Relationships**: Belongs to a Redis key of type "zset". Member is unique; score determines ordering.

**Validation**: Member must be a non-empty string. Score must be a valid number (including negative, zero, and decimal values).

---

### GetSortedSetRangeResult

Response from the sorted set range read endpoint (index-based pagination).

| Field | Type | Description |
|-------|------|-------------|
| entries | SortedSetEntryDto[] | Array of member-score pairs, sorted by score ascending |

---

### RemoveFromSetRequest

Request to remove members from a set.

| Field | Type | Description |
|-------|------|-------------|
| groupId | string | Server group identifier |
| key | string | Redis key name |
| members | string[] | Members to remove |

---

### RemoveFromListRequest

Request to remove items from a list.

| Field | Type | Description |
|-------|------|-------------|
| groupId | string | Server group identifier |
| key | string | Redis key name |
| value | string | Value to remove |
| count | number | Number of occurrences to remove (0 = all, 1 = first match) |

---

### RemoveFromSortedSetRequest

Request to remove entries from a sorted set.

| Field | Type | Description |
|-------|------|-------------|
| groupId | string | Server group identifier |
| key | string | Redis key name |
| members | string[] | Member values to remove |

---

## State Transitions

### Dialog Lifecycle States

```
Closed → Loading → Ready → Editing → Saving → Closed
                 ↘ Error (retry → Loading)
                          Editing ↘ Error (retry → Saving)
```

### Item States (within dialog)

| State | Visual | Description |
|-------|--------|-------------|
| Original | Normal row | Loaded from server, unmodified |
| Modified | Highlighted row | Value changed locally |
| Pending Add | Highlighted with "new" indicator | Added locally, not yet saved |
| Pending Delete | Strikethrough with delete indicator | Marked for removal, can be restored |
