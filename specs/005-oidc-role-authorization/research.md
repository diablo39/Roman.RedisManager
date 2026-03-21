# Phase 0 Research: OIDC AuthN/AuthZ Configuration

## Decision 1: Use a bearer-only authentication entrypoint for protected APIs

- Decision: Configure ASP.NET Core authentication with bearer token handling as the only authentication mechanism.
- Rationale: This keeps the API security model simple and aligned with non-browser client access.
- Alternatives considered:
- Session-based authentication rejected because CLI and third-party clients require bearer support.
- Supporting multiple authentication entry modes rejected because it increases complexity without current product need.
- Separate APIs per auth mode rejected due to duplicated authorization logic and higher operational complexity.

## Decision 2: Model provider definitions as configuration-first entries with built-in profiles for EntraID, Google, and generic OIDC

- Decision: Create typed options for identity providers where each provider entry includes issuer/authority and claim mapping metadata; include default sections for EntraID, Google, and generic OIDC.
- Rationale: Meets out-of-the-box provider support while preserving extension-by-configuration for new providers.
- Alternatives considered:
- Hardcoding provider implementations rejected because extensibility without redeploy is a key requirement.
- Single provider configuration rejected because multiple issuers must be supported concurrently.

## Decision 3: Normalize external claims into internal roles via a transformation component

- Decision: Implement principal transformation that evaluates configured claim key/value rules per provider and adds normalized role claims used by `[Authorize(Roles = ...)]`.
- Rationale: Keeps endpoint authorization semantics stable and independent of external provider claim shapes.
- Alternatives considered:
- Endpoint-level manual claim parsing rejected because it leaks provider-specific logic into application/web code.
- Domain-layer claim mapping rejected because identity source concerns must stay outside domain logic.

## Decision 4: Enforce group-aware authorization through policy handlers using request group context

- Decision: Define authorization requirements and handlers that evaluate normalized roles against default permission rules and group-specific override rules using target `groupId` from request context.
- Rationale: Satisfies requirement for non-uniform permissions by Redis group while keeping authorization declarative.
- Alternatives considered:
- Role-only global policies rejected because they cannot express per-group restrictions.
- Inline controller checks rejected because they violate consistency and are harder to audit/test.

## Decision 5: Resolve precedence with explicit override-first semantics

- Decision: For a protected action and target group: evaluate matching group override first; if present, use it as authoritative; otherwise use global default permissions; deny when no allow rule matches.
- Rationale: Deterministic and safe-by-default behavior is required for production-sensitive groups.
- Alternatives considered:
- Merge-allows semantics rejected because it can unintentionally re-grant restricted operations.
- Implicit fallback to allow rejected for security reasons.

## Decision 6: Keep authorization concerns in Web layer with options validated at startup

- Decision: Store authentication/authorization configuration in Web app settings, bind to strongly typed options classes with data annotations and startup validation.
- Rationale: Aligns with project conventions and catches configuration errors before runtime traffic.
- Alternatives considered:
- Lazy runtime parsing of raw configuration rejected due to delayed failures.
- Infrastructure repository-based config retrieval rejected as unnecessary complexity for static policy configuration.

## Decision 7: Testing strategy includes behavior matrices and mutation loop evidence

- Decision: Add tests for claim normalization and group override policy evaluation matrices, then run mutation loop `run -> analyze -> improve -> rerun` and record artifacts.
- Rationale: Authorization logic is branch-heavy and benefits from mutation-guided assertion hardening.
- Alternatives considered:
- Happy-path-only tests rejected because they miss negative authorization and precedence regressions.
- Skipping mutation analysis rejected by constitution and copilot instructions.
