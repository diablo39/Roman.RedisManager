# Feature Specification: Frontend Authentication

**Feature Branch**: `008-frontend-authentication`  
**Created**: 2026-03-22  
**Status**: Draft  
**Input**: User description: "name of the feature: frontend authentication

Us a user I'd like to be able to login to system using one of the supported OIDC systems.

Contract #file:Roman.RedisManager.Web.json contains endpoint that return list of supported oidc providers use it to fetch list of supported iodc providers
Prepare ui that allows user to redirect to provider for authentication
add Guards - all other pages / screens requires authentication"

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Start Sign-In With a Supported Provider (Priority: P1)

As a user who is not signed in, I can open the login screen, see the currently supported sign-in providers, and choose one provider to begin authentication.

**Why this priority**: Users cannot access the system at all without a working sign-in entry point, so provider discovery and sign-in initiation are the core MVP.

**Independent Test**: Can be fully tested by opening the login screen in a signed-out state, confirming that available providers are listed from the current provider discovery result, and selecting a provider to start the authentication redirect.

**Acceptance Scenarios**:

1. **Given** the user is signed out and at least one sign-in-capable provider is available, **When** the user opens the login screen, **Then** the system shows the available providers using the names supplied by the current provider discovery result.
2. **Given** the user is signed out and the login screen shows one or more providers, **When** the user selects a provider, **Then** the system starts the authentication flow with that provider.
3. **Given** the user is signed out and no provider is currently sign-in-capable, **When** the user opens the login screen, **Then** the system explains that sign-in is unavailable and does not present unusable sign-in actions.

---

### User Story 2 - Reach Protected Screens Only After Authentication (Priority: P2)

As a user trying to open any application page other than the public login flow, I am required to authenticate before protected content becomes available.

**Why this priority**: Protecting the rest of the application is the main control that gives authentication business value beyond the login screen itself.

**Independent Test**: Can be fully tested by requesting a protected page while signed out and confirming the user is redirected to sign in before protected content is shown.

**Acceptance Scenarios**:

1. **Given** the user is signed out, **When** the user requests a protected page, **Then** the system redirects the user to the login flow before rendering protected content.
2. **Given** the user is signed out and was redirected away from a protected page, **When** the user finishes authentication successfully, **Then** the system returns the user to the originally requested in-app destination when that destination is valid.
3. **Given** the user is signed out and requests the default landing experience, **When** the user is redirected to sign in, **Then** the system may return the user to the standard authenticated landing page after successful authentication.
4. **Given** the user is authenticated, **When** the application loads protected Redis server group data, **Then** the request is sent as an authenticated request bound to that user session.

---

### User Story 3 - Recover From Authentication State Problems (Priority: P3)

As a user, I receive a clear outcome when authentication cannot start or complete, including when no providers are available, the session expires mid-flow, or I revisit the login page after already signing in.

**Why this priority**: Authentication failures are inevitable, and without explicit recovery behavior users will get trapped in broken or confusing states.

**Independent Test**: Can be fully tested by simulating unavailable providers, expired sign-in state, and an already-authenticated visit to the login route and confirming each path results in a clear, usable outcome.

**Acceptance Scenarios**:

1. **Given** the user is already authenticated, **When** the user opens the login route, **Then** the system redirects the user to the requested destination or the standard authenticated landing page instead of showing the login screen.
2. **Given** the user starts authentication but the sign-in state cannot be completed successfully, **When** the user returns to the application, **Then** the system shows a clear error outcome and gives the user a way to try signing in again.
3. **Given** the system can no longer validate the user session, **When** the user requests a protected page, **Then** the system treats the session as signed out and requires authentication again.

### Edge Cases

- The provider discovery result is temporarily unavailable or fails to load.
- The provider discovery result succeeds but returns zero sign-in-capable providers.
- A provider shown on the login screen is no longer usable by the time the user selects it.
- The requested return destination is missing, invalid, or points outside the application.
- The user returns from the external provider without a usable authenticated session.
- The user opens the login route directly after already signing in.
- A protected Redis server group request is attempted after the authenticated session has expired or can no longer be validated.

### User Experience Consistency _(mandatory)_

- The login experience MUST follow the existing application layout, terminology, and page-level feedback patterns already used for loading, empty, error, and success states.
- Provider choices MUST be presented using the display names supplied by the provider discovery source so the UI stays aligned with supported identity systems.
- Protected-route handling MUST feel consistent across all screens: signed-out users should see a predictable redirect to the login flow rather than inconsistent per-page failures.
- When authentication is unavailable or fails, the UI MUST provide a concise explanation and an obvious recovery action such as retrying or selecting another available provider.
- Intentional deviation: the login route is the only user-facing route that remains publicly accessible; all other application pages are treated as protected by default because the feature goal is full authenticated access control.
- Accessibility expectations: users must be able to discover providers, start sign-in, understand unavailable/error states, and recover from failures using keyboard navigation and screen-reader-readable labels and status messaging.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The system MUST provide a public sign-in entry point for users who are not authenticated.
- **FR-002**: The system MUST retrieve the list of sign-in-capable OIDC providers from the current provider discovery source before presenting provider choices.
- **FR-003**: The sign-in entry point MUST present only the providers returned by the current provider discovery result and MUST identify each provider by its supplied display name.
- **FR-004**: The system MUST allow the user to start authentication by selecting one of the available providers.
- **FR-005**: The system MUST preserve the user’s intended in-application destination during the sign-in flow and restore that destination after successful authentication when the destination is valid.
- **FR-006**: The system MUST require authentication for every application page and screen except the public authentication flow.
- **FR-007**: When an unauthenticated user requests a protected page or screen, the system MUST redirect the user to the sign-in entry point before protected content is shown.
- **FR-008**: When an authenticated user requests the sign-in entry point, the system MUST redirect the user to the preserved destination or the standard authenticated landing page.
- **FR-009**: When the current provider discovery result indicates that sign-in is unavailable, the system MUST present a non-blocking explanation of the unavailable state and MUST withhold unusable sign-in actions.
- **FR-010**: When authentication cannot be completed successfully, the system MUST show a clear recovery path that allows the user to retry sign-in without navigating through unrelated screens.
- **FR-011**: When the system can no longer validate the current authenticated session, the system MUST clear the invalid session state and require the user to authenticate again before accessing protected content.
- **FR-012**: The system MUST send authenticated requests for protected Redis server group data using the current authenticated user session.

### Key Entities _(include if feature involves data)_

- **Authentication Provider**: A supported external identity option that can be offered to the user for sign-in, including a stable key, a display name, and availability status.
- **Authentication Availability State**: The current sign-in availability result, including whether sign-in is available, which providers may be used, and why sign-in is unavailable when no provider can be offered.
- **Authenticated Session**: The user’s current signed-in state within the application, including whether access is currently valid and whether the session must be re-established.
- **Return Destination**: The in-application page or screen the user originally requested before being redirected to sign in.
- **Protected Redis Server Group Request**: A request for Redis server group data that is only valid when issued under an authenticated user session.

### Assumptions & Dependencies

- A server-managed provider discovery source remains the source of truth for which OIDC providers are enabled and sign-in-capable at any given time.
- The initial scope covers interactive sign-in and route protection only; sign-out, role-based authorization, and profile management are outside this feature unless added later.
- The application has one standard authenticated landing experience that can be used when no valid return destination is available.
- Provider-specific branding requirements are not part of this feature beyond using the provider names supplied by the provider discovery source.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: 100% of signed-out requests to protected pages are redirected to the sign-in flow before protected page content is displayed.
- **SC-002**: In normal operating conditions, users can see the currently available sign-in providers within 10 seconds of opening the sign-in entry point.
- **SC-003**: At least 95% of successful sign-ins return the user to the originally requested in-app destination when that destination is valid.
- **SC-004**: 100% of unavailable-provider states present users with a readable explanation instead of a dead-end or broken sign-in action.
- **SC-005**: At least 90% of first-attempt users can identify a valid provider and start sign-in without external assistance.
- **SC-006**: Already-authenticated users who open the sign-in route are redirected away from it in all standard usage scenarios.
