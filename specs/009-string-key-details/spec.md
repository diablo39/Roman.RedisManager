# Feature Specification: String Key Details View

**Feature Branch**: `009-string-key-details`
**Created**: 2026-03-25
**Status**: Draft
**Input**: User description: "As a user I'd like to see details of a key of type string, with editable fields, JSON formatting, shareable URL, and cancel navigation."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View String Key Details from Keys Tab (Priority: P1)

As a user browsing keys in the Keys tab, I want to click on a key name to open a dialog that displays all attributes of that string key, so I can inspect its value and metadata without leaving the keys list.

**Why this priority**: This is the core interaction flow - without it, no other stories function.

**Independent Test**: Can be fully tested by searching for a string key in the Keys tab, clicking its name, and verifying a dialog opens with correct key name, value, type, and TTL.

**Acceptance Scenarios**:

1. **Given** I am on a Redis server's Keys tab with search results displayed, **When** I click on a key name of type "string", **Then** a dialog opens showing the key's name, type, value, and TTL.
2. **Given** the string key detail dialog opens, **When** data is loading, **Then** the system fetches the key's value and metadata from the API and displays all fields once loaded.
3. **Given** a string key has no TTL (never expires), **When** I view its details in the dialog, **Then** the TTL field indicates no expiration.
4. **Given** a string key has a TTL set, **When** I view its details in the dialog, **Then** the TTL is displayed in a human-readable editable format.

---

### User Story 2 - JSON Value Formatting and Syntax Highlighting (Priority: P1)

As a user viewing a string key whose value is valid JSON, I want the value to be automatically formatted (pretty-printed) with syntax highlighting, so I can easily read structured data.

**Why this priority**: JSON is the most common structured value stored in Redis string keys; readable display is essential for usability.

**Independent Test**: Can be tested by creating a string key with a JSON value, opening its detail dialog, and verifying the value is formatted and syntax-highlighted.

**Acceptance Scenarios**:

1. **Given** a string key has a value that is valid JSON, **When** I view its details, **Then** the value is displayed formatted (pretty-printed) with syntax highlighting.
2. **Given** a string key has a value that is NOT valid JSON (plain text), **When** I view its details, **Then** the value is displayed as-is in a text area without formatting errors.

---

### User Story 3 - Edit String Key Details (Priority: P1)

As a user viewing a string key's details in the dialog, I want to edit the value and TTL fields and save my changes, so I can update the key directly from the UI.

**Why this priority**: Edit capability is core to the "detail in edit mode" requirement and provides direct user value.

**Independent Test**: Can be tested by opening a string key detail dialog, modifying the value, clicking Save, and verifying the updated value persists in Redis.

**Acceptance Scenarios**:

1. **Given** I am viewing the key detail dialog, **When** it opens, **Then** the key name and type fields are displayed as read-only, and the value and TTL fields are editable.
2. **Given** I have not modified any field, **When** I view the Save button, **Then** it is disabled.
3. **Given** I modify the value or TTL field, **When** I check the Save button, **Then** it becomes enabled.
4. **Given** I have modified fields and click Save, **When** the save operation succeeds, **Then** the system persists changes via the API and the Save button becomes disabled again.
5. **Given** I have modified fields and click Save, **When** the save operation fails, **Then** an error message is displayed and the Save button remains enabled.

---

### User Story 4 - Shareable URL / Deep Linking (Priority: P2)

As a user, I want to copy the URL while the key detail dialog is open and share it (or paste it into a browser), so that after authentication I can directly see the details of that specific key.

**Why this priority**: Enables collaboration and bookmarking, but depends on the dialog existing first.

**Independent Test**: Can be tested by opening a key detail dialog, copying the browser URL, opening it in a new browser session, authenticating, and verifying the dialog opens automatically with correct key details.

**Acceptance Scenarios**:

1. **Given** I have the key detail dialog open, **When** I copy the browser URL, **Then** the URL contains query parameters (server group ID and key name) sufficient to identify the key.
2. **Given** I paste a key detail URL into a new browser window, **When** I authenticate, **Then** I am taken to the Redis server page and the key detail dialog opens automatically showing the correct key's information.

---

### User Story 5 - Close Dialog (Priority: P2)

As a user viewing key details in the dialog, I want to close the dialog and return to the keys list with my previous search results preserved.

**Why this priority**: Provides essential navigation flow, but is secondary to viewing and editing.

**Independent Test**: Can be tested by opening the detail dialog from the keys list, closing it, and verifying the keys list still shows the previous search results.

**Acceptance Scenarios**:

1. **Given** I opened the key detail dialog from the Keys tab search results, **When** I click Cancel or the dialog close button, **Then** the dialog closes and the keys list remains visible with my previous search results intact.
2. **Given** the dialog was opened automatically via a deep link URL, **When** I close the dialog, **Then** the server page is displayed with the Keys tab active.
3. **Given** I have unsaved changes and close the dialog, **When** the dialog closes, **Then** changes are discarded silently (no confirmation prompt).

---

### Edge Cases

- What happens when the key no longer exists by the time the dialog loads? The system should display an error state inside the dialog indicating the key was not found.
- What happens when the value is extremely large (> 1 MB)? The system should display a performance warning banner and still render without freezing the browser. Values above 1 MB are the threshold for this warning.
- What happens when the key's TTL expires while the user has the dialog open? The system should handle a failed save gracefully with an appropriate error message.
- What happens when the user modifies the value to invalid JSON? The system should save it as a plain string - JSON formatting is display-only, not a constraint on input.
- What happens when the URL contains a key name with special characters (slashes, colons, etc.)? The URL encoding must handle all valid Redis key characters.

### User Experience Consistency *(mandatory)*

- The key detail dialog MUST use `v-dialog` with appropriate max-width and the `.card-header-separated` header pattern, consistent with other dialogs in the application (e.g., delete confirmation dialog, create key dialog).
- Read-only fields (key name, type) MUST be visually distinct from editable fields (e.g., disabled input styling).
- The type field MUST use the same color-coded chip pattern used in the keys list (String = blue).
- Loading state MUST use skeleton loaders consistent with the existing keys explorer loading pattern.
- Error states MUST use `v-alert type="error" variant="tonal"` with a Retry button, matching existing error handling.
- The Cancel button placement and styling MUST follow the same pattern as other dialogs in the application.
- The TtlPicker component SHOULD be reused for TTL editing to maintain consistency.
- Opening the dialog MUST update the browser URL with query parameters for deep linking; closing the dialog MUST remove those query parameters.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a dialog for a string key showing: key name (read-only), type (read-only), value (editable), and TTL (editable).
- **FR-002**: System MUST fetch key details (value and metadata) from the API when the dialog opens, using the key name and server group ID.
- **FR-003**: System MUST detect whether the string value is valid JSON and, if so, display it formatted (pretty-printed) with syntax highlighting.
- **FR-004**: System MUST allow editing of the value field (as a text/code editor) and the TTL field (using the existing TtlPicker component).
- **FR-005**: System MUST keep the Save button disabled until the user modifies at least one editable field.
- **FR-006**: System MUST send updated value and TTL to the API when the user clicks Save, using the POST /api/redis/data/strings endpoint.
- **FR-007**: System MUST provide a Cancel button that closes the dialog, preserving the keys list search state underneath.
- **FR-008**: System MUST support deep linking - opening the dialog MUST update the URL with query parameters (`?key=<name>&type=string`); navigating to a URL with these parameters MUST auto-open the dialog after authentication.
- **FR-009**: System MUST handle error states: key not found, network failures, and save failures, with appropriate user feedback inside the dialog.
- **FR-010**: System MUST encode the key name in the URL query parameters to support special characters in Redis key names.
- **FR-011**: System MUST preserve code quality gates (linting, type safety, and static analysis) for all modified artifacts.
- **FR-012**: System MUST include automated tests for critical logic and user journeys affected by this feature.

### Key Entities

- **String Key Detail**: Represents the full detail of a Redis string key, comprising: key name (identifier), type ("string"), value (the stored string), and TTL (time-to-live in milliseconds, nullable).
- **Server Group**: The Redis server/cluster context in which the key exists, identified by a UUID group ID.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can open a string key's detail dialog in a single click from the keys list.
- **SC-002**: JSON values are displayed formatted and syntax-highlighted within 1 second of dialog open.
- **SC-003**: Users can edit the value and TTL and successfully save changes on the first attempt when the key exists.
- **SC-004**: A shared URL opens directly to the Redis server page with the key detail dialog auto-opened after authentication, with 100% accuracy for all valid Redis key names.
- **SC-005**: Closing the dialog preserves the keys list search results underneath.
- **SC-006**: All new UI states (loading, error, empty, edit) match approved design patterns with zero critical accessibility violations.

## Assumptions

- This feature covers **string-type keys only**. Detail views for other Redis types (hash, list, set, sorted set) are out of scope and will be addressed in separate features.
- The backend API endpoints (`GET /api/redis/data/strings`, `GET /api/redis-keys/{key}/metadata`, `POST /api/redis/data/strings`) already exist and are stable. No backend changes are required.
- Authentication and authorization are handled by the existing OIDC guard and Bearer token mechanism.
- The `POST /api/redis/data/strings` endpoint serves as both create and update — sending a request for an existing key overwrites it.
- Values larger than 1 MB may degrade editor performance. A warning is displayed but no hard truncation is applied.
- Unsaved changes are discarded silently on dialog close — no confirmation prompt is shown.
- The dialog approach eliminates the need for a separate page route; deep linking is achieved through URL query parameters on the existing server page.
