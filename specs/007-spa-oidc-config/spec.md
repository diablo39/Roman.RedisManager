# Feature Specification: SPA OIDC Authentication Bootstrap

**Feature Branch**: `[007-spa-oidc-config]`  
**Created**: 2026-03-22  
**Status**: Draft  
**Input**: User description: "I have Vue3 based frontend application that will use API provided by this application to allow user to interact with redis via http api. I need authentication. I was thinking about adding some oidc client to my frontend application, but I don't know which. This library will need some configuration from the backend. Find for me an OIDC client library that is able to interact with any OIDC server, check configuration option requirements, and design an appropriate endpoint in my application."

## User Scenarios & Testing *(mandatory)*

### Mutation Quality Requirement *(mandatory when tests change)*

- If this feature adds or modifies tests, the implementation MUST follow `run -> analyze -> improve -> rerun` for mutation quality.
- Mutation evidence for this feature MUST be recorded in the repository's standard test output locations, including HTML and JSON reports for the affected project and a short analysis of any surviving or no-coverage mutants.

### User Story 1 - Frontend obtains login bootstrap settings (Priority: P1)

As a frontend application, I need a backend-provided authentication bootstrap response so I can start a standards-compliant OIDC sign-in flow without hardcoding provider-specific settings in the deployed client.

**Why this priority**: Without a bootstrap response, the frontend cannot reliably initiate authentication across environments or providers, which blocks all authenticated Redis access.

**Independent Test**: Can be fully tested by requesting the authentication bootstrap response from an unauthenticated client and verifying that it contains the enabled provider choices plus the browser-safe settings required to start sign-in.

**Acceptance Scenarios**:

1. **Given** at least one authentication provider is enabled, **When** the frontend requests authentication bootstrap data, **Then** the system returns the enabled provider choices and the browser-safe sign-in settings required for a generic OIDC client.
2. **Given** multiple authentication providers are enabled, **When** the frontend requests authentication bootstrap data, **Then** the system returns each provider with a stable key, a display name, and the sign-in settings needed to start authentication with that provider.
3. **Given** provider configuration contains backend-only sensitive values, **When** the frontend requests authentication bootstrap data, **Then** those sensitive values are excluded from the response.

---

### User Story 2 - Frontend authenticates against different OIDC servers (Priority: P2)

As a product owner, I want the frontend authentication model to work with any standards-compliant OIDC provider so that the application is not locked to one identity vendor.

**Why this priority**: Vendor-neutral authentication reduces coupling and aligns with the backend's existing support for multiple provider kinds, including generic OIDC.

**Independent Test**: Can be tested by configuring different provider records and verifying that the bootstrap response remains valid for each enabled provider without requiring frontend code changes.

**Acceptance Scenarios**:

1. **Given** an enabled generic OIDC provider with discovery support, **When** the frontend requests bootstrap data, **Then** the response contains the required fields for the frontend to start a standard authorization code with PKCE flow.
2. **Given** an enabled provider whose discovery metadata cannot be consumed directly by the browser, **When** the frontend requests bootstrap data, **Then** the response contains the additional browser-safe metadata needed to complete sign-in without direct discovery calls.

---

### User Story 3 - Authentication configuration changes without frontend redeploy (Priority: P3)

As an administrator, I want to manage authentication settings in backend configuration so that provider changes can be applied per environment without rebuilding the frontend.

**Why this priority**: Environment-specific identity settings are operational data. Treating them as backend-managed configuration reduces release friction and misconfiguration risk.

**Independent Test**: Can be tested by changing backend authentication settings, restarting the application, and verifying that subsequent bootstrap responses reflect the new provider configuration.

**Acceptance Scenarios**:

1. **Given** a provider is disabled in backend configuration, **When** the frontend requests bootstrap data after the configuration is reloaded, **Then** the disabled provider is not offered for sign-in.
2. **Given** a provider display name or browser-safe authentication setting changes in backend configuration, **When** the frontend requests bootstrap data after the configuration is reloaded, **Then** the response reflects the updated values.

---

### Edge Cases

- What happens when no authentication providers are enabled? The system returns a deterministic bootstrap response with `bootstrapState = unavailable` and machine-readable reason code(s), without returning partial provider configuration.
- How does the system handle a provider that is enabled for token validation but lacks the browser-safe settings required for interactive sign-in? The system excludes that provider from the frontend bootstrap response and explains why it is unavailable.
- How does the system handle a provider whose discovery document cannot be called from the browser because of cross-origin restrictions? The system returns explicit discovery override data needed by the frontend instead of forcing the browser to infer missing endpoints.
- What happens when a frontend requests a specific provider that becomes disabled before sign-in completes? The system rejects that provider selection on the next bootstrap request and does not advertise stale settings.
- How does the system handle duplicate provider keys in configuration? The system fails configuration validation and does not expose ambiguous provider choices.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST expose a read-only authentication bootstrap API that is callable before user authentication.
- **FR-002**: The authentication bootstrap API MUST return only authentication providers that are currently enabled and valid for frontend sign-in.
- **FR-003**: For each returned provider, the system MUST supply a stable provider key and human-readable display name so the frontend can present and select sign-in options.
- **FR-004**: For each returned provider, the system MUST supply the browser-safe OIDC client settings required for a standards-compliant SPA client to initiate an authorization code with PKCE sign-in flow.
- **FR-005**: The bootstrap response MUST include the minimum provider settings required for sign-in initiation, including issuer authority, client identifier, redirect target, and requested scopes.
- **FR-006**: The bootstrap response MUST include sign-out and silent renewal settings when those flows are supported for the selected provider and current deployment.
- **FR-007**: The bootstrap response MUST support provider-specific discovery overrides when direct browser discovery is unavailable or insufficient.
- **FR-008**: The bootstrap response MUST NOT expose backend-only secrets, private signing material, or any value intended only for server-side token validation.
- **FR-009**: The system MUST provide a deterministic endpoint-level availability state and machine-readable reason code(s) so the frontend can distinguish a usable bootstrap response from an unavailable configuration state.
- **FR-010**: Authentication bootstrap data MUST remain consistent with the backend's configured token validation providers so that the frontend can only initiate sign-in against providers the API will accept.
- **FR-011**: The system MUST allow backend configuration changes to alter the frontend bootstrap response without requiring frontend source code changes.
- **FR-012**: The system MUST preserve support for multiple OIDC providers, including generic OIDC issuers, within the same deployment.

### Key Entities *(include if feature involves data)*

- **Authentication Provider Summary**: A browser-safe representation of one enabled and sign-in-capable provider option, including its stable key, display label, and OIDC startup profile.
- **Frontend OIDC Bootstrap Profile**: The set of browser-safe OIDC client settings needed by the SPA to start sign-in, maintain session continuity where supported, and complete sign-out.
- **Authentication Bootstrap Response**: The full read-only response returned to the frontend, containing endpoint-level availability state, reason codes when unavailable, and zero or more provider summaries.

### Assumptions

- The frontend will use a generic standards-compliant SPA OIDC client rather than a provider-specific SDK.
- The preferred browser flow is authorization code with PKCE for all interactive sign-ins.
- Redirect targets used by the SPA are known per deployment and can be represented as backend-managed configuration.
- The backend remains the source of truth for which identity providers are accepted for API access.
- This feature is limited to frontend authentication bootstrap and does not change the existing authorization model for Redis operations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A frontend team can configure sign-in for a new environment using only backend-managed authentication settings, with no frontend rebuild required for provider-specific value changes.
- **SC-002**: In usability testing, 95% of bootstrap requests made by an unauthenticated client return either an `available` bootstrap state with complete browser-safe provider configuration or an `unavailable` bootstrap state with deterministic reason code(s).
- **SC-003**: In validation scenarios covering at least three different OIDC provider types, the frontend can start sign-in successfully using the bootstrap response without adding provider-specific logic for each vendor.
- **SC-004**: Zero backend-only secrets or private validation values appear in the authentication bootstrap response across all supported providers.
- **SC-005**: After an administrator changes enabled providers or browser-safe authentication settings and reloads configuration, the updated bootstrap response is observable on the next request.
