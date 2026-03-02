# Feature: Redis Value CRUD Endpoints

## Overview

Extends the Redis Manager API with full read, write, and delete support for all five core Redis data types: **String**, **List**, **Set**, **Hash**, and **Sorted Set**. Prior to this feature the API was entirely read-only. All write operations accept an optional TTL. Bulk operations are supported where applicable (e.g. pushing multiple list values in one request, adding multiple set members, setting multiple hash fields).

---

## Capabilities by Data Type

### Generic Key Operations (all types)

- **Delete a key** — permanently removes any key regardless of type.
- **Get key metadata** — returns the data type of the key and its remaining TTL, if set.
- **Get key value (auto-detect)** — reads the value of any key without needing to know the type in advance; the response is discriminated by a `type` field so callers can handle each type appropriately.

### String

- **Set a string value** — creates or overwrites a string key. Supports three conditional modes:
  - *Always* — overwrite unconditionally (default).
  - *Only if not exists* — set the value only when the key does not yet exist (NX).
  - *Only if exists* — update the value only when the key already exists (XX).
- **Get a string value** — returns the current value of a string key, or `null` if the key does not exist.

### List

- **Push values** — appends or prepends one or more values to a list. The caller chooses whether to push to the left (head) or right (tail) of the list.
- **Get a range** — returns a slice of the list by index range. Requesting index `0` to `-1` returns the full list.
- **Remove elements** — removes occurrences of a specific value from the list. A count of `0` removes all occurrences; a positive count removes up to that many from the head; a negative count from the tail.

### Set

- **Add members** — adds one or more members to a set. Duplicate members are silently ignored.
- **Scan members** — retrieves members using cursor-based pagination, suitable for large sets.
- **Remove members** — removes one or more specific members from the set.

### Hash

- **Set fields** — creates or updates one or more field–value pairs in a hash. Existing fields are overwritten.
- **Scan fields** — retrieves field–value pairs using cursor-based pagination, suitable for large hashes.
- **Remove fields** — deletes one or more specific fields from the hash.

### Sorted Set

- **Add entries** — adds one or more member–score pairs to a sorted set. If a member already exists its score is updated.
- **Get range by rank** — returns entries ordered by score (ascending) within an index range. Requesting `0` to `-1` returns all entries.
- **Remove members** — removes one or more members from the sorted set.

---

## Cross-Cutting Behaviour

### TTL (Time to Live)
Every write operation accepts an optional TTL. When provided, the key automatically expires after the specified duration. When omitted the key persists until explicitly deleted.

### Conditional Write Response
When a conditional `SET` (NX or XX) is not applied because the condition is not met, the endpoint returns a success response with `success: false` — it is not treated as an error.

### Mutation Responses
Write and remove endpoints return a minimal result only (success flag or removed count). They do not return the new state of the key after the mutation.

### Cursor Pagination
Set and Hash scan endpoints use Redis cursor-based pagination (`SSCAN` / `HSCAN`). A cursor of `0` starts a new scan. Each response includes the next cursor and a `hasMoreResults` flag. The page size is a hint and Redis may return more or fewer items per page.

### Type Mismatch
If a write or read operation targets a key that already holds a different Redis data type, the API returns **409 Conflict**.
