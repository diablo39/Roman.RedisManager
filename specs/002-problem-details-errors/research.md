# Research Notes: RFC 9457 Error Responses

## Decision: Use built-in ASP.NET Core ProblemDetails middleware

**Rationale:**
- ASP.NET Core already implements an RFC 9457-compliant `ProblemDetails` type
  and corresponding middleware/service (`AddProblemDetails`, `UseExceptionHandler`,
  `UseStatusCodePages`). Reusing built-in functionality minimizes effort and
  avoids reinventing the contract.
- The project currently uses controllers (not minimal APIs) and already
  configures MVC in `Program.cs`; therefore adding `builder.Services.AddProblemDetails()`
  is straightforward.
- The existing error behavior (exceptions get swallowed by statuscode pages and
  return empty bodies) will be replaced with problem details without major
  changes to individual controllers.

**Alternatives considered:**
1. Writing custom exception filters and manual `Problem()` results in each
   controller action. Rejected due to boilerplate and increased maintenance.
2. Leaving default behavior and documenting clients to expect empty bodies.
   Rejected because it fails the user requirement for a standardized payload.

## Decision: Use trace-id as primary correlation ID and include request path

**Rationale:**
- Support success metric SC-003 requires correlation information in responses.
- `trace-id` is stable and concise across distributed traces, making it better
  as a support-facing correlation key than full `traceparent`.
- ASP.NET Core `ProblemDetails` exposes an `Extensions` dictionary where these
  values can be added via a custom `IProblemDetailsFactory` or middleware.
- Minimal overhead and no persistence changes.

**Implementation notes:**
- Add `traceId` (derived from distributed tracing context trace identifier) as
  the primary support correlation field in `ProblemDetails.Extensions`.
- Optionally include raw `traceparent` as a secondary diagnostic field.
- Continue including `requestPath` to improve support triage and incident lookup.

## Decision: Validation errors via `ValidationProblemDetails`

**Rationale:**
- Applying `[ApiController]` attribute already causes automatic model-state
  based 400 responses using `ValidationProblemDetails`. We keep that behavior
  and ensure serialization settings produce camelCase keys.

## Research summary
- `Microsoft.AspNetCore.Mvc.ProblemDetails` type is in the `Microsoft.AspNetCore.Mvc.Core`
  package, already referenced by the project.
- `IProblemDetailsService` and `AddProblemDetails` are defined in `Microsoft.AspNetCore.Mvc.ProblemDetails`.
- The default problem type URIs (e.g. `https://tools.ietf.org/html/rfc7231#section-6.5.1`) can be overridden via
  `ProblemDetailsDefaults` or by intercepting the factory.
- To avoid leaking stack traces in production, rely on `UseExceptionHandler` which
  by default will capture and produce generic details.
- Extensions: `context.TraceIdentifier` available in `HttpContext`.

## Implementation considerations
- `Program.cs` must register `AddProblemDetails()` and configure `UseExceptionHandler()`
  before controllers.
- Optionally customize `ProblemDetailsFactory` or use a middleware snippet that
  intercepts `ProblemDetails` and adds additional extensions (`traceId`,
  optional `traceparent`, `requestPath`).
- Validation error keys: ensure `SystemTextJsonValidationMetadataProvider` already
  applied in `Program.cs` for camelCase, but not strictly necessary since keys are
  property names.

## Conclusion
Proceed with leveraging built-in problem details support and augment as needed
for correlation context and consistent payloads across status codes.