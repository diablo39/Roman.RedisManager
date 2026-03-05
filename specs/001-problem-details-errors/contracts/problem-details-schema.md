# Contracts: Problem Details Error Format

The API will conform to the following JSON schema for error responses. This
contract is public and should be included in client generation or documentation.

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "ProblemDetails",
  "type": "object",
  "properties": {
    "type": {"type": "string", "format": "uri"},
    "title": {"type": "string"},
    "status": {"type": "integer"},
    "detail": {"type": "string"},
    "instance": {"type": "string", "format": "uri"},
    "traceId": {"type": "string"},
    "traceparent": {"type": "string"},
    "requestPath": {"type": "string"},
    "errors": {
      "type": "object",
      "additionalProperties": {"type": "array", "items": {"type": "string"}}
    }
  },
  "required": ["traceId"],
  "additionalProperties": true
}
```

- `errors` property is only present for validation failures (`ValidationProblemDetails`).
- `traceId` is the primary support correlation ID and is expected on all problem responses.
- `traceparent` is optional diagnostic context for distributed tracing interoperability.
- Additional properties may be included via the `extensions` mechanism; clients
  must ignore unknown fields.
