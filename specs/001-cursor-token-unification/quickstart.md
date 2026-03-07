# Quickstart: Validate Unified Continuation Token Flow

## Prerequisites

- Backend is running.
- A valid Redis server group exists.
- Test data contains enough keys to require multiple pages.

## 1. Start a New Search (No Token)

Request:

```http
GET /api/redis-keys?groupId={GROUP_ID}&pattern=*&pageSize=50
```

Expected:

- `200 OK`
- Response contains `keys`
- Response contains `hasMoreResults`
- If `hasMoreResults=true`, response includes `continuationToken`

## 2. Continue Search with Returned Token

Request:

```http
GET /api/redis-keys?groupId={GROUP_ID}&pattern=*&pageSize=50&continuationToken={TOKEN_FROM_PREVIOUS_RESPONSE}
```

Expected:

- `200 OK`
- Next page of keys is returned
- New `continuationToken` is returned when more results remain

## 3. Verify Completion Semantics

Repeat step 2 until search completes.

Expected final page:

- `hasMoreResults=false`
- `continuationToken` absent or null

Recommended validation:

- Capture three consecutive page responses and verify each request replays the previous `continuationToken` exactly.
- Confirm the terminal page explicitly reports `hasMoreResults=false` and does not return a next token.

## 4. Verify Invalid Token Handling

Request with corrupted token:

```http
GET /api/redis-keys?groupId={GROUP_ID}&pattern=*&pageSize=50&continuationToken=invalid-token
```

Expected:

- `400 Bad Request`
- Error code indicates invalid continuation token
- Error message instructs user to restart search

## 5. Verify Context Mismatch Handling

Reuse a token with different `pattern` or `pageSize`.

Expected:

- `400 Bad Request`
- Error code indicates continuation context mismatch

## 6. Verify Topology-Agnostic Contract

Run steps 1-5 against both:

- Standalone group
- Cluster group

Expected:

- Same request and response contract in both cases
- No response field exposes node-level cursor state
