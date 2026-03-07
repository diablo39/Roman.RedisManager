# Contract: Redis Key Search with Unified Continuation Token

## Endpoint

- Method: `GET`
- Route: `/api/redis-keys`

## Query Parameters

- `groupId` (required, GUID)
- `pattern` (optional, string, default `*`)
- `pageSize` (optional, integer, default server value)
- `continuationToken` (optional, string)

Notes:

- Omit `continuationToken` to start a new search.
- Replay `continuationToken` exactly as returned to continue paging.
- `continuationToken` is opaque and must not be parsed by clients.

## 200 Response

```json
{
  "keys": [
    { "key": "session:1001" },
    { "key": "session:1002" }
  ],
  "hasMoreResults": true,
  "continuationToken": "v1.eyJ2Ijo..."
}
```

Rules:

- When `hasMoreResults` is `true`, `continuationToken` is present.
- When `hasMoreResults` is `false`, `continuationToken` is `null` or omitted.
- Response does not include cluster node cursor details.

## Error Responses

### 400 Bad Request: Invalid continuation token

```json
{
  "type": "https://roman.redismanager/errors/invalid_continuation_token",
  "title": "Invalid continuation token",
  "status": 400,
  "code": "invalid_continuation_token",
  "message": "Continuation token is invalid. Start a new search."
}
```

### 400 Bad Request: Context mismatch

```json
{
  "type": "https://roman.redismanager/errors/continuation_context_mismatch",
  "title": "Invalid continuation token",
  "status": 400,
  "code": "continuation_context_mismatch",
  "message": "Continuation token does not match the current search parameters. Start a new search."
}
```

### 400 Bad Request: Not resumable

```json
{
  "type": "https://roman.redismanager/errors/continuation_not_resumable",
  "title": "Invalid continuation token",
  "status": 400,
  "code": "continuation_not_resumable",
  "message": "Continuation token can no longer be resumed. Start a new search."
}
```

## Breaking Changes from Current Contract

- Request parameter `cursor` is replaced by `continuationToken`.
- Response property `cursor` (numeric) is replaced by `continuationToken` (string).
- Response property `nodeCursors` is removed.
- Completion is represented by `hasMoreResults=false` and no next token.

## Contract Invariants

- One continuation contract for standalone and cluster groups.
- Client does not branch behavior by group topology.
- Token replay is deterministic within a valid search context.
