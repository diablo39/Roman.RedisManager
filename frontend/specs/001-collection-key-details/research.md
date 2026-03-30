# Research: Collection Key Details

**Feature**: 001-collection-key-details
**Date**: 2026-03-28

## Research Summary

### Decision 1: Backend API Availability

**Decision**: All required backend read/write/remove APIs already exist for lists, sets, and sorted sets.

**Rationale**: The backend is a .NET/C# ASP.NET Core application using CQRS with Wolverine. Controllers exist at `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/` for all five Redis data types.

**Findings**:

| Type | Read Endpoint | Pagination | Write Endpoint | Remove Endpoint |
|------|--------------|------------|----------------|-----------------|
| Lists | `GET api/redis/data/lists?groupId&key&start&stop` | Index-based (LRANGE) | `POST api/redis/data/lists` | `POST api/redis/data/lists/remove` |
| Sets | `GET api/redis/data/sets?groupId&key&cursor&pageSize` | Cursor-based (SSCAN) | `POST api/redis/data/sets` | `POST api/redis/data/sets/remove` |
| Sorted Sets | `GET api/redis/data/sorted-sets?groupId&key&start&stop` | Index-based (ZRANGE) | `POST api/redis/data/sorted-sets` | `POST api/redis/data/sorted-sets/remove` |

**Alternatives considered**: Building mock APIs — rejected because real endpoints are already implemented.

---

### Decision 2: Pagination Strategy Per Type

**Decision**: Use the pagination model that matches each backend endpoint.

**Rationale**:
- **Sets** use cursor-based pagination (SSCAN), identical to the existing hash fields pattern. The cursor is an opaque long value.
- **Lists** use index-based range pagination (LRANGE with start/stop). The frontend will paginate by requesting successive index ranges (e.g., 0–99, 100–199).
- **Sorted sets** use index-based range pagination (ZRANGE with start/stop), same as lists. Entries are returned in score order.

**Alternatives considered**: Using SSCAN for all types — rejected because lists and sorted sets don't support cursor-based scanning; their natural access pattern is index-based.

---

### Decision 3: Frontend API Client Gaps

**Decision**: Add six new API functions to `src/api/redisKeys.ts`.

**Rationale**: The frontend currently only has create/write functions (`createListKey`, `createSetKey`, `createSortedSetKey`) but no read or remove functions for these types. Six functions are needed:

| Function | Purpose | Backend Endpoint |
|----------|---------|-----------------|
| `getListRange` | Read list items by index range | `GET api/redis/data/lists` |
| `getSetMembers` | Read set members with cursor pagination | `GET api/redis/data/sets` |
| `getSortedSetRange` | Read sorted set entries by index range | `GET api/redis/data/sorted-sets` |
| `removeFromList` | Remove items from list | `POST api/redis/data/lists/remove` |
| `removeFromSet` | Remove members from set | `POST api/redis/data/sets/remove` |
| `removeFromSortedSet` | Remove members from sorted set | `POST api/redis/data/sorted-sets/remove` |

**Alternatives considered**: Creating a separate API module — rejected to maintain consistency with existing pattern where all key operations are in `redisKeys.ts`.

---

### Decision 4: Dialog Component Architecture

**Decision**: Create three separate dialog components following the HashKeyDetailDialog pattern.

**Rationale**: The existing hash dialog (HashKeyDetailDialog.vue) is the most feature-rich dialog and provides the closest template for collection type dialogs. Each type has unique display needs:
- **SetKeyDetailDialog**: Single-column member list (no field names like hash), cursor-based pagination
- **ListKeyDetailDialog**: Index column + value column, index-based pagination, head/tail add direction
- **SortedSetKeyDetailDialog**: Member + Score columns, index-based pagination, score editing

**Alternatives considered**:
- Single generic dialog with conditional rendering — rejected because it would create excessive complexity in a single component and reduce maintainability.
- Composable-based shared logic — considered but the existing pattern uses self-contained dialog components. Consistency with existing codebase is more important.

---

### Decision 5: Deep Linking Extension

**Decision**: Extend the existing `onOpenKey` function and `checkDeepLink` function in `redis/[id].vue` to handle `set`, `list`, and `zset` types.

**Rationale**: The URL pattern `?key=keyname&type=string|hash` already works. Adding three more type values (`set`, `list`, `zset`) follows the established pattern exactly. No routing changes needed.

**Alternatives considered**: None — the existing pattern is clean and extensible.

---

### Decision 6: Edit Pattern for Sets (Member Replacement)

**Decision**: Editing a set member means removing the old value and adding the new value, since Redis sets are value-based (no field names).

**Rationale**: Unlike hashes where you can update a field's value in place, set members ARE the identity. Changing a member's value is semantically "remove old + add new." The UI should make this transparent to the user by collecting edits and translating them to remove + add operations on save.

**Alternatives considered**: Disabling inline edit for sets — rejected because it would be inconsistent with the hash and other dialog editing experiences.

---

### Decision 7: List Item Removal Strategy

**Decision**: Use the Redis LREM command (via `POST api/redis/data/lists/remove`) which removes items by value and count.

**Rationale**: The backend's `RemoveFromListRequest` takes `Value` and `Count` parameters, matching Redis LREM semantics. For the UI, marking an item for deletion will record the value, and on save, a remove request is sent. Note: if a list has duplicate values, LREM removes matching values by count — the UI should remove the specific occurrence the user selected (count=1).

**Alternatives considered**: Index-based removal — not supported by the current backend API.
