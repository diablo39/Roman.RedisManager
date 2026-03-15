# API Endpoint Reference

This document lists the currently public HTTP endpoints exposed by the backend API.

## Base

- Base URL (development): `https://localhost:7244`
- OpenAPI JSON (development): `/openapi/v1.json`
- Swagger UI (development): enabled when running in Development environment

## Authentication

Some endpoints require a Bearer token and an authorization policy.

- `ReadKeys` policy: read access to key and server-group information
- `DeleteKeysByGroup` policy: delete access scoped by group permissions

For runnable request examples, see `src/Roman.RedisManager.Web/Roman.RedisManager.Web.http`.

## Redis Server Groups

### GET /api/redis-server-groups

- Description: List configured Redis server groups with pagination.
- Query:
  - `pageNumber` (optional, default `1`)
  - `pageSize` (optional, default `10`)
- Auth: `ReadKeys`

### GET /api/redis-server-groups/{id}

- Description: Get details for one Redis server group.
- Route:
  - `id` (`Guid`)
- Auth: `ReadKeys`

## Redis INFO

### GET /api/commands/redis-info

- Description: Get Redis INFO output for a host in a group.
- Query:
  - `groupId` (`Guid`)
  - `host` (`string`)
  - `port` (`int`)
- Auth: none

## Redis Keys

### GET /api/redis-keys

- Description: Search Redis keys by pattern with continuation-token paging.
- Query:
  - `groupId` (`Guid`, required)
  - `pattern` (optional, default `*`)
  - `continuationToken` (optional)
  - `pageSize` (optional, default `100`)
- Auth: `ReadKeys`

### DELETE /api/redis-keys/{key}

- Description: Delete a key in a group.
- Route:
  - `key` (`string`)
- Query:
  - `groupId` (`Guid`)
- Auth: `DeleteKeysByGroup`

### GET /api/redis-keys/{key}/metadata

- Description: Read key metadata (type, ttl, expiration flags).
- Route:
  - `key` (`string`)
- Query:
  - `groupId` (`Guid`)
- Auth: `ReadKeys`

### GET /api/redis-keys/{key}/value

- Description: Read key value with type-aware payload.
- Route:
  - `key` (`string`)
- Query:
  - `groupId` (`Guid`)
- Auth: `ReadKeys`

## Redis Data - Strings

### POST /api/redis/data/strings

- Description: Set or update a string value.
- Body: `SetStringRequest`
- Auth: none

### GET /api/redis/data/strings

- Description: Get a string value by key.
- Query:
  - `groupId` (`Guid`)
  - `key` (`string`)
- Auth: none

## Redis Data - Lists

### POST /api/redis/data/lists

- Description: Push one or more values into a list.
- Body: `PushToListRequest`
- Auth: none

### GET /api/redis/data/lists

- Description: Get a list value range.
- Query:
  - `groupId` (`Guid`)
  - `key` (`string`)
  - `start` (`long`, default `0`)
  - `stop` (`long`, default `-1`)
- Auth: none

### POST /api/redis/data/lists/remove

- Description: Remove values from a list.
- Body: `RemoveFromListRequest`
- Auth: none

## Redis Data - Hashes

### POST /api/redis/data/hashes

- Description: Set one or more hash fields.
- Body: `SetHashFieldsRequest`
- Auth: none

### GET /api/redis/data/hashes

- Description: Read hash fields using cursor paging.
- Query:
  - `groupId` (`Guid`)
  - `key` (`string`)
  - `cursor` (`long`, default `0`)
  - `pageSize` (`int`, default `100`)
- Auth: none

### POST /api/redis/data/hashes/remove

- Description: Remove one or more hash fields.
- Body: `RemoveHashFieldsRequest`
- Auth: none

## Redis Data - Sets

### POST /api/redis/data/sets

- Description: Add members to a set.
- Body: `AddToSetRequest`
- Auth: none

### GET /api/redis/data/sets

- Description: Read set members using cursor paging.
- Query:
  - `groupId` (`Guid`)
  - `key` (`string`)
  - `cursor` (`long`, default `0`)
  - `pageSize` (`int`, default `100`)
- Auth: none

### POST /api/redis/data/sets/remove

- Description: Remove members from a set.
- Body: `RemoveFromSetRequest`
- Auth: none

## Redis Data - Sorted Sets

### POST /api/redis/data/sorted-sets

- Description: Add members with scores to a sorted set.
- Body: `AddToSortedSetRequest`
- Auth: none

### GET /api/redis/data/sorted-sets

- Description: Read sorted-set members by rank range.
- Query:
  - `groupId` (`Guid`)
  - `key` (`string`)
  - `start` (`long`, default `0`)
  - `stop` (`long`, default `-1`)
- Auth: none

### POST /api/redis/data/sorted-sets/remove

- Description: Remove members from a sorted set.
- Body: `RemoveFromSortedSetRequest`
- Auth: none

## Hidden/Internal Endpoints

The following endpoints are intentionally excluded from OpenAPI:

- `TestController` test-only routes under `/api/test/*`
- `ErrorController` exception endpoint at `/error`
