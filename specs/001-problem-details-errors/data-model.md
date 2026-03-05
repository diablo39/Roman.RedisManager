# Data Model: RFC 9457 Error Responses

This feature does not introduce any persistent domain entities. Instead, it
defines the structure of the error payload returned by the API. The relevant
"entities" are transient objects serialized in HTTP responses.

## Key Structures

- **ProblemDetails** (existing ASP.NET Core type)
  - Properties: `Type` (URI identifying problem class), `Title` (short
    human-readable summary), `Status` (HTTP status code), `Detail` (human-readable
    explanation), `Instance` (URI identifying this specific occurrence),
    `Extensions` (dictionary for additional metadata such as mandatory
    `traceId` as primary support correlation ID, optional `traceparent`, and
    `requestPath`).

- **ValidationProblemDetails** (subclass of `ProblemDetails`)
  - Includes `Errors` dictionary mapping field names to arrays of error messages.

## Relationships

- No relationships to domain aggregates or repositories; these are purely
  serialization contracts returned from middleware or controllers.

## Notes

- All additional fields (trace id, path) will be added via the
- All additional fields (`traceId`, optional `traceparent`, `requestPath`) will
  be added via the
  `ProblemDetails.Extensions` dictionary at runtime and do not require model
  classes.
- The API does not store or persist these objects; they exist only for the
  duration of a single HTTP request.
