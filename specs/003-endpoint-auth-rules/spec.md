# Feature Specification: Endpoint Authorisation Rules

**Feature Branch**: `003-endpoint-auth-rules`
**Created**: 2026-03-15
**Status**: Draft
**Input**: User description: "Authorisation rules - protect all endpoints (except tests controller) via authorisation rules. Endpoints that return data require 'reader' policy, endpoints that set/remove data require 'editor' policy."

## User Scenarios & Testing

### Mutation Quality Requirement

- If this feature adds or modifies tests, the implementation MUST follow `run -> analyze -> improve -> rerun` for mutation quality.
- Mutation reports and iteration evidence will be recorded in the feature directory under `test-reports/`.

### User Story 1 - Read-Only User Access (Priority: P1)

A user with the "reader" policy can browse and retrieve data from any endpoint that returns information. This includes searching keys, viewing key metadata, retrieving key values, listing server groups, fetching server group details, getting Redis info, and reading values for all data types (strings, hashes, lists, sets, sorted sets). The user can perform all read operations without restriction.

**Why this priority**: Read access is the most fundamental operation. Most users will need to view data, and protecting read endpoints ensures that unauthenticated or unauthorised users cannot access sensitive Redis data.

**Independent Test**: Can be fully tested by authenticating as a user with only the "reader" policy and verifying access to all GET endpoints returns successful responses.

**Acceptance Scenarios**:

1. **Given** a user authenticated with the "reader" policy, **When** they request any GET endpoint (except tests controller), **Then** they receive a successful response with the requested data.
2. **Given** a user authenticated with the "reader" policy, **When** they attempt to call a write/delete endpoint, **Then** they receive a 403 Forbidden response.
3. **Given** an unauthenticated user, **When** they request any protected GET endpoint, **Then** they receive a 401 Unauthorised response.

---

### User Story 2 - Editor User Access (Priority: P1)

A user with the "editor" policy can create, update, and delete data through any endpoint that modifies state. This includes setting string values, adding/removing hash fields, pushing/removing list items, adding/removing set members, adding/removing sorted set members, and deleting keys. The editor policy grants write access only; read access requires the "reader" policy separately.

**Why this priority**: Write protection is equally critical to read protection. Without it, any authenticated user could modify or destroy Redis data.

**Independent Test**: Can be fully tested by authenticating as a user with the "editor" policy and verifying access to all POST/PUT/DELETE/PATCH endpoints returns successful responses.

**Acceptance Scenarios**:

1. **Given** a user authenticated with the "editor" policy, **When** they call any write/delete endpoint, **Then** the operation succeeds and the data is modified as expected.
2. **Given** a user without the "editor" policy, **When** they attempt to call a write/delete endpoint, **Then** they receive a 403 Forbidden response.
3. **Given** a user with both "reader" and "editor" policies, **When** they call any endpoint (except tests controller), **Then** all operations succeed.

---

### User Story 3 - Tests Controller Remains Unprotected (Priority: P2)

The tests controller remains accessible without authorisation so that development and testing workflows are not disrupted. This controller is only available in the development environment and is used for verifying error handling behaviour.

**Why this priority**: Ensuring the tests controller exclusion is important but lower priority since it only affects the development environment. It prevents accidental lockout during development.

**Independent Test**: Can be tested by calling tests controller endpoints without any authentication credentials and verifying they respond normally in the development environment.

**Acceptance Scenarios**:

1. **Given** the application is running in development mode, **When** an unauthenticated user calls any tests controller endpoint, **Then** the request is processed without authorisation checks.
2. **Given** the application is running in development mode, **When** an authenticated user without any policies calls a tests controller endpoint, **Then** the request succeeds.

---

### User Story 4 - Unauthorised Access Feedback (Priority: P2)

When a user attempts to access a protected endpoint without the required policy, the system provides a clear and consistent error response indicating what went wrong, allowing the user or administrator to take corrective action.

**Why this priority**: Good error feedback reduces support burden and helps administrators troubleshoot access issues quickly.

**Independent Test**: Can be tested by making requests with insufficient permissions and verifying response codes and messages are consistent and informative.

**Acceptance Scenarios**:

1. **Given** an unauthenticated user, **When** they call any protected endpoint, **Then** they receive a 401 Unauthorised response.
2. **Given** an authenticated user without the required policy, **When** they call a protected endpoint, **Then** they receive a 403 Forbidden response with a message indicating which policy is required.

---

### Edge Cases

- What happens when a user has the "editor" policy but not the "reader" policy? They should only be able to write/delete, not read.
- What happens when authorisation configuration is missing or malformed? The system should fail closed (deny access by default).
- What happens when a new endpoint is added in the future without explicit authorisation? The system should require authorisation by default for all new endpoints (fail closed).
- How does the system behave when the authentication provider is unavailable? Users should receive a 401 response rather than a 500 error.

## Clarifications

### Session 2026-03-15

- Q: Should existing granular policies (ReadKeys, DeleteKeysByGroup, and permission actions ReadKeys/ReadMetadata/ReadValues/DeleteKey/WriteKey) be replaced or extended? → A: Replace existing granular policies entirely with reader/editor.
- Q: What level of authorization detail should the OpenAPI specification expose? → A: Security scheme definition AND per-endpoint policy requirements (reader/editor).
- Q: How should reader/editor policies be determined from the authentication token? → A: Custom claim names, configurable per deployment.

## Requirements

### Functional Requirements

- **FR-001**: System MUST enforce the "reader" policy on all endpoints that return data (GET operations), except the tests controller.
- **FR-002**: System MUST enforce the "editor" policy on all endpoints that create, update, or delete data (POST, PUT, DELETE, PATCH operations), except the tests controller.
- **FR-003**: System MUST allow the tests controller to remain accessible without any authorisation requirements.
- **FR-004**: System MUST return a 401 Unauthorised response when an unauthenticated user accesses a protected endpoint.
- **FR-005**: System MUST return a 403 Forbidden response when an authenticated user lacks the required policy for an endpoint.
- **FR-006**: The "reader" and "editor" policies MUST be independent — possessing one does not imply the other.
- **FR-007**: System MUST apply authorisation to all current and future endpoints by default (fail-closed approach), with the tests controller being an explicit exception.
- **FR-008**: System MUST continue to support existing authentication mechanisms (bearer token, OIDC providers) without modification.
- **FR-009**: System MUST replace all existing granular authorization policies (ReadKeys, DeleteKeysByGroup) and permission actions (ReadKeys, ReadMetadata, ReadValues, DeleteKey, WriteKey) with the new "reader" and "editor" policies.
- **FR-010**: System MUST remove the existing group-based permission authorization handler and related infrastructure as part of the policy replacement.
- **FR-011**: The OpenAPI specification MUST define the authentication security scheme and declare the required policy (reader or editor) on each protected endpoint.
- **FR-012**: The OpenAPI specification MUST indicate that the tests controller endpoints do not require authentication.
- **FR-013**: The "reader" and "editor" policies MUST be evaluated from configurable custom claim names in the authentication token, allowing each deployment to define which claim and value(s) grant each policy.
- **FR-014**: The custom claim configuration MUST be modifiable through application settings without code changes.

## Success Criteria

### Measurable Outcomes

- **SC-001**: 100% of data-returning endpoints (except tests controller) reject unauthenticated requests with a 401 response.
- **SC-002**: 100% of data-modifying endpoints (except tests controller) reject requests from users without the "editor" policy with a 403 response.
- **SC-003**: 100% of data-returning endpoints (except tests controller) reject requests from users without the "reader" policy with a 403 response.
- **SC-004**: Tests controller endpoints remain accessible without authentication in all scenarios.
- **SC-005**: Authorisation check adds no perceptible delay to request processing (less than 50ms overhead per request).
- **SC-006**: All 23 existing endpoints are covered by the appropriate policy (12 read endpoints with "reader" policy, 11 write endpoints with "editor" policy).
- **SC-007**: The OpenAPI specification accurately reflects the security scheme and per-endpoint policy requirements for 100% of endpoints.

## Assumptions

- The existing authentication infrastructure (bearer tokens, OIDC providers) is functioning correctly and does not need modification.
- The "reader" and "editor" policies are evaluated from custom claim names that are configurable per deployment via application settings.
- The tests controller is exclusively used in non-production environments and does not need protection.
- The existing group-based permission model, granular policies, and permission actions will be fully replaced by the new reader/editor policy model.
- No new user interface changes are required — authorisation is enforced at the endpoint level only.

## Dependencies

- Existing authentication system must be operational and issuing tokens with appropriate claims.
- Policy definitions must be configurable by administrators without code changes.

## Out of Scope

- Changes to the authentication mechanism itself (login flow, token issuance, OIDC configuration).
- User interface changes for managing policies or displaying authorisation errors.
- Audit logging of authorisation decisions (may be a future enhancement).
- Fine-grained per-resource or per-key authorisation (e.g., restricting access to specific Redis keys or server groups).
