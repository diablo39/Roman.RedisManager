# API Contract: Authorization Responses

**Date**: 2026-03-15 | **Branch**: `003-endpoint-auth-rules`

## Security Scheme

All protected endpoints require Bearer token authentication.

```
Authorization: Bearer <token>
```

## Authorization Policies per Endpoint

### Reader Policy (GET endpoints)

| Route | Method | Policy |
|-------|--------|--------|
| `api/redis-keys` | GET | Reader |
| `api/redis-keys/{key}/metadata` | GET | Reader |
| `api/redis-keys/{key}/value` | GET | Reader |
| `api/redis-server-groups` | GET | Reader |
| `api/redis-server-groups/{id}` | GET | Reader |
| `api/commands/redis-info` | GET | Reader |
| `api/redis-strings/{groupId}/{key}` | GET | Reader |
| `api/redis-hashes/{groupId}/{key}` | GET | Reader |
| `api/redis-lists/{groupId}/{key}` | GET | Reader |
| `api/redis-sets/{groupId}/{key}` | GET | Reader |
| `api/redis-sorted-sets/{groupId}/{key}` | GET | Reader |

### Editor Policy (mutation endpoints)

| Route | Method | Policy |
|-------|--------|--------|
| `api/redis-keys/{key}` | DELETE | Editor |
| `api/redis-strings/{groupId}/{key}` | POST | Editor |
| `api/redis-hashes/{groupId}/{key}/fields` | POST | Editor |
| `api/redis-hashes/{groupId}/{key}/fields/remove` | POST | Editor |
| `api/redis-lists/{groupId}/{key}/push` | POST | Editor |
| `api/redis-lists/{groupId}/{key}/remove` | POST | Editor |
| `api/redis-sets/{groupId}/{key}/add` | POST | Editor |
| `api/redis-sets/{groupId}/{key}/remove` | POST | Editor |
| `api/redis-sorted-sets/{groupId}/{key}/add` | POST | Editor |
| `api/redis-sorted-sets/{groupId}/{key}/remove` | POST | Editor |

### Unprotected Endpoints

| Route | Method | Reason |
|-------|--------|--------|
| `api/test/*` | GET | Development-only test controller |
| `/error` | ANY | Exception handler endpoint |

## Response Contracts

### 401 Unauthorized

Returned when no valid Bearer token is provided.

```
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer
```

### 403 Forbidden

Returned when authenticated user lacks the required policy.

```
HTTP/1.1 403 Forbidden
```

## OpenAPI Security Definition

The OpenAPI document must include:
1. A `securitySchemes` component defining the Bearer authentication scheme
2. Per-operation `security` requirements declaring which policy (reader/editor) applies
3. No security requirement on TestController and ErrorController operations
