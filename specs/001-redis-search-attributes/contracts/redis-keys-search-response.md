# Contract: GET /api/redis-keys

## Purpose

Return paged Redis keys for a server group with enriched metadata and continuation support.

## Request

- Method: `GET`
- Route: `/api/redis-keys`
- Query parameters:
- `groupId` (guid, required)
- `pattern` (string, optional, default `*`)
- `continuationToken` (string, optional)
- `pageSize` (int, optional, capped by server-side max)

## Response 200

```json
{
  "keys": [
    {
      "key": "session:123",
      "type": "String",
      "ttlMilliseconds": 1800000,
      "hasExpiration": true
    },
    {
      "key": "user:42:profile",
      "type": "Hash",
      "ttlMilliseconds": null,
      "hasExpiration": false
    }
  ],
  "hasMoreResults": true,
  "continuationToken": "<opaque-token-or-null>"
}
```

Field rules:
- `keys`: always present; can be empty.
- `type`: always present; string enum-like value.
- `ttlMilliseconds`: nullable number with explicit mapping rules:
  - persistent key: `ttlMilliseconds = null` and `hasExpiration = false`
  - expiring key: `ttlMilliseconds >= 0` and `hasExpiration = true`
  - metadata miss (key disappeared/volatile): `type = "Unknown"`, `ttlMilliseconds = null`, `hasExpiration = false`
- `hasExpiration`: always present.
- `continuationToken`: present when `hasMoreResults` is true; null otherwise.

## Error Contracts

- `400 Bad Request`: invalid input or invalid continuation token.
- `404 Not Found`: unknown Redis group.
- `500 Internal Server Error`: Redis connectivity or server-side failures.

## Backward Compatibility

Backward compatibility with the previous minimal key item shape is not required for this feature.
