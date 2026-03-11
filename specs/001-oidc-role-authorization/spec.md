# Feature Specification: OIDC AuthN/AuthZ Configuration

**Feature Branch**: `001-oidc-role-authorization`  
**Created**: 2026-03-08  
**Status**: Draft  
**Input**: User description: "AuthZ and AuthN for Redis-Manager API with OIDC providers, role normalization, and per-group permission overrides"

## User Scenarios & Testing *(mandatory)*

### Mutation Quality Requirement *(mandatory when tests change)*

- If this feature adds or modifies tests, the implementation MUST follow `run -> analyze -> improve -> rerun` for mutation quality.
- Mutation evidence MUST be recorded in the feature implementation notes and include:
- HTML and JSON mutation reports for each run.
- Surviving/no-coverage mutant analysis with actionable vs non-actionable classification.
- Assertion updates tied to specific surviving mutants.
- Follow-up mutation run output showing improvement or approved exceptions.

### User Story 1 - Unified Authentication Entry (Priority: P1)

As a Redis-Manager administrator, I can configure OpenID Connect authentication so API clients can authenticate to protected endpoints using bearer tokens from supported providers.

**Why this priority**: Without a unified authentication entry point, no secured access model can be enforced and all downstream authorization behavior is blocked.

**Independent Test**: Can be fully tested by configuring one identity provider, authenticating via API bearer token flow, and verifying the principal is accepted for protected API endpoints.

**Acceptance Scenarios**:

1. **Given** a valid bearer token from a configured identity provider, **When** a client calls a protected endpoint, **Then** the request is authenticated through token-based principal validation.
2. **Given** a missing bearer token, **When** a client calls a protected endpoint, **Then** access is denied with `401 Unauthorized`.
3. **Given** an unknown or misconfigured identity provider identifier, **When** an authentication attempt is made, **Then** access is denied and the error indicates invalid provider configuration.

---

### User Story 2 - Role Normalization Across Providers (Priority: P1)

As a Redis-Manager administrator, I can define internal application roles and map each role to provider-specific claim keys and claim values so authorization logic is consistent regardless of token issuer.

**Why this priority**: Role normalization is required so business-facing authorization policies are stable and do not depend on external claim shape differences.

**Independent Test**: Can be fully tested by defining one internal role mapped to two providers with different claim structures and verifying both principals resolve to the same internal role.

**Acceptance Scenarios**:

1. **Given** internal role mappings for multiple providers, **When** principals with matching provider claims authenticate, **Then** each principal is assigned the same normalized internal role.
2. **Given** an authenticated principal whose claims do not match any configured role mapping, **When** authorization is evaluated, **Then** the principal receives no mapped role and role-protected actions are denied.
3. **Given** updated role mappings in configuration, **When** the system reloads configuration, **Then** subsequent authorization evaluations use the updated mappings without domain model changes.

---

### User Story 3 - Group-Specific Authorization Overrides (Priority: P1)

As a Redis-Manager administrator, I can define default permissions and Redis server group-specific overrides so role access can differ by target group.

**Why this priority**: Operational risk is highest in sensitive groups (for example production caches), so per-group overrides are required to prevent unintended write access.

**Independent Test**: Can be fully tested by granting a role write access globally, denying that role for a specific group, and verifying writes are denied for the restricted group while still allowed elsewhere.

**Acceptance Scenarios**:

1. **Given** a default permission policy that allows a role to modify keys, **When** that role targets a group without overrides, **Then** the action is permitted.
2. **Given** a group-specific override that restricts modify-key actions to administrator roles, **When** a non-administrator role targets that group, **Then** the action is denied.
3. **Given** a group-specific override and a valid administrator role, **When** the administrator targets that group, **Then** the action is permitted.

---

### Edge Cases

- What happens when a request targets a Redis server group ID that has no explicit override and no matching default permission rule?
- How does the system handle a token that is valid for authentication but missing configured claim keys needed for role mapping?
- What happens when two configured role mappings match the same principal with conflicting permissions?
- How does authorization behave when the configured provider metadata is temporarily unavailable during sign-in?
- What happens when the group ID in the request is malformed, missing, or does not exist?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support authentication for protected API endpoints using bearer tokens issued by configured OpenID Connect providers.
- **FR-002**: System MUST provide out-of-the-box provider configuration support for EntraID, Google, and a generic OpenID Connect issuer profile.
- **FR-003**: System MUST allow administrators to add additional OpenID Connect provider definitions through configuration only.
- **FR-004**: System MUST provide a configuration section where administrators define internal application roles.
- **FR-005**: System MUST allow each internal role to map to one or more external claim key/value combinations per identity provider.
- **FR-006**: System MUST normalize authenticated principals into internal application roles based on configured claim mappings before role-based authorization checks run.
- **FR-007**: System MUST support global permission rules that define which internal roles may perform protected actions.
- **FR-008**: System MUST support per-Redis-server-group permission overrides, configured within each Redis server group definition, that can narrow or replace global permissions for the same protected actions.
- **FR-009**: System MUST evaluate authorization decisions using both normalized internal roles and the target Redis server group identifier from the request context.
- **FR-010**: System MUST deny protected actions when no matching allow rule exists after applying global rules and applicable group override rules.
- **FR-011**: System MUST expose authorization behavior to API endpoints through declarative role-based policies so endpoint code remains independent from provider-specific claim structures.
- **FR-012**: System MUST allow administrators to change provider mappings and permission overrides through configuration updates without requiring domain model redesign.
- **FR-013**: System MUST log authentication and authorization outcomes with enough detail to audit which normalized role and group context were used in each decision.

### Key Entities *(include if feature involves data)*

- **Identity Provider Configuration**: Defines issuer-specific authentication settings and claim interpretation metadata for EntraID, Google, generic OIDC, and future providers.
- **Application Role Definition**: Represents internal role names used by authorization rules (for example, `redis-reader`, `editor`, `admin`).
- **Role Claim Mapping Rule**: Connects an internal role to one or more provider-specific claim key/value matches.
- **Permission Rule**: Defines which normalized roles may execute protected action categories.
- **Group Permission Override**: Associates a Redis server group identifier with permission rules that supersede or constrain defaults for that group.
- **Authorization Decision Context**: Contains normalized roles, requested action, target group identifier, and the resulting allow/deny decision for audit purposes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of protected API requests require successful authentication through configured bearer-token flow.
- **SC-002**: Administrators can configure EntraID, Google, and generic OIDC providers and complete a documented provider test matrix with at least one successful authentication scenario per provider within one configuration cycle.
- **SC-003**: At least 95% of authorization decisions for supported action/group combinations resolve in under 1 second under normal operational load.
- **SC-004**: For a test matrix covering at least 20 role-to-claim mapping permutations across providers, normalized role resolution accuracy is 100%.
- **SC-005**: For a test matrix covering at least 20 global vs group-override authorization cases, allow/deny outcomes match configured policy expectations at 100%.
- **SC-006**: At least two configuration-only updates (one role mapping change and one group override change) can be applied and validated without code changes, rebuild, or redeploy, with verification completed in a single operational cycle.

## Assumptions

- The API already has endpoint-level authorization hooks where role-based policies can be applied consistently.
- A request can reliably identify the target Redis server group for operations that require group-scoped authorization decisions.
- Configuration changes follow existing operational practices for secure configuration management and deployment.
- Existing domain logic remains provider-agnostic; identity source interpretation is handled in authentication/authorization pipeline layers.
