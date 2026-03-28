# Feature Specification: Hash Key Details Viewer

**Feature Branch**: `010-hash-key-details`  
**Created**: March 26, 2026  
**Status**: Draft  
**Input**: User description: "As a user, I want to open details for Redis hash keys from the Keys tab, inspect and edit hash fields, format JSON values, deep link to the dialog, and close without losing list context."

## User Scenarios & Testing _(mandatory)_

### User Story 1 - View Hash Key Details (Priority: P1)

As a user browsing Redis keys, I want to open a hash key from the Keys tab and view all its field-value pairs in a detailed view so that I can inspect the hash contents without using the Redis CLI.

**Why this priority**: This is the foundational capability that enables all other features. Without the ability to view hash details, users cannot perform any subsequent operations. This delivers immediate value by providing visibility into hash key data.

**Independent Test**: Can be fully tested by selecting a hash key from the Keys tab, clicking to open details, and verifying that all field-value pairs are displayed. Delivers value by replacing CLI commands with a visual interface.

**Acceptance Scenarios**:

1. **Given** I am on the Keys tab viewing a list of Redis keys, **When** I click on a hash key or its action menu, **Then** a dialog opens displaying the hash key name and all field-value pairs
2. **Given** a hash key has 0 fields (empty hash), **When** I open its details, **Then** I see an empty state message indicating no fields exist
3. **Given** a hash key has 100+ fields, **When** I open its details, **Then** all fields load and display with proper pagination or scrolling
4. **Given** I open a hash key details dialog, **When** the dialog is visible, **Then** I can see field names in one column and corresponding values in another column

---

### User Story 2 - Edit Hash Field Values (Priority: P2)

As a user viewing hash key details, I want to edit existing field values inline so that I can update Redis hash data without switching to a separate tool or CLI.

**Why this priority**: Editing is the primary action users need after viewing data. This transforms the viewer from read-only to a full management tool, significantly increasing utility.

**Independent Test**: Can be tested independently by opening a hash key, clicking on a field value, modifying it, and verifying the change persists in Redis. Delivers value by enabling data correction and updates.

**Acceptance Scenarios**:

1. **Given** I am viewing hash key details, **When** I click on a field value, **Then** the value becomes editable (inline edit mode activated)
2. **Given** I have edited a field value, **When** I save the change (blur or press Enter), **Then** the new value is sent to Redis and persisted
3. **Given** I have edited a field value, **When** I press Escape or cancel, **Then** the original value is restored without saving
4. **Given** I attempt to save an empty field value, **When** I try to save, **Then** I see a validation error indicating values cannot be empty
5. **Given** a save operation fails (network error, Redis error), **When** the error occurs, **Then** I see an error message and the field reverts to its original value

---

### User Story 3 - JSON Value Formatting (Priority: P3)

As a user viewing hash field values that contain JSON, I want the JSON to be automatically detected and formatted with syntax highlighting so that I can read complex JSON structures more easily.

**Why this priority**: Many Redis hashes store serialized JSON objects. Pretty-printing and syntax highlighting dramatically improve readability for complex data structures without requiring external tools.

**Independent Test**: Can be tested by opening a hash key with JSON-formatted field values and verifying that JSON values are displayed with indentation and syntax highlighting. Delivers value by improving data comprehension.

**Acceptance Scenarios**:

1. **Given** a hash field value contains valid JSON (starts with `{` or `[`), **When** I view the field, **Then** the JSON is automatically formatted with proper indentation
2. **Given** a hash field value contains valid JSON, **When** I view the field, **Then** syntax highlighting is applied (different colors for keys, strings, numbers, booleans, null)
3. **Given** a hash field value looks like JSON but is invalid, **When** I view the field, **Then** it displays as plain text without formatting errors
4. **Given** a hash field value is plain text (not JSON), **When** I view the field, **Then** it displays as-is without formatting attempts
5. **Given** I am editing a JSON-formatted field value, **When** I make changes, **Then** the JSON remains formatted in the editor for readability

---

### User Story 4 - Add and Delete Hash Fields (Priority: P4)

As a user managing hash keys, I want to add new fields or delete existing fields so that I can fully manage the hash structure without external tools.

**Why this priority**: Completes the CRUD operations for hash fields. While less frequently used than viewing/editing, this enables full hash management and removes the need for CLI access.

**Independent Test**: Can be tested by opening a hash key, adding a new field, deleting an existing field, and verifying changes in Redis. Delivers value by providing complete hash management.

**Acceptance Scenarios**:

1. **Given** I am viewing hash key details, **When** I click "Add Field" button, **Then** a new empty row appears where I can enter a field name and value
2. **Given** I have entered a new field name and value, **When** I save, **Then** the field is added to the Redis hash and appears in the list
3. **Given** I attempt to add a field with a name that already exists, **When** I try to save, **Then** I see a validation error or confirmation dialog about overwriting
4. **Given** I am viewing a hash field, **When** I click a delete icon/button for that field, **Then** I see a confirmation dialog
5. **Given** I confirm deletion of a field, **When** I confirm, **Then** the field is removed from Redis and disappears from the list
6. **Given** I attempt to delete a field but the operation fails, **When** the error occurs, **Then** I see an error message and the field remains in the list

---

### User Story 5 - Deep Linking to Hash Key Details (Priority: P5)

As a user, I want to share a URL that opens a specific hash key's details dialog directly so that I can collaborate with teammates or bookmark important hash keys.

**Why this priority**: Enhances collaboration and workflow efficiency through shareable links. While valuable, it's not essential for basic hash key management.

**Independent Test**: Can be tested by copying a hash key details URL, opening it in a new browser tab, and verifying the keys tab loads with the specific hash key dialog open. Delivers value through improved collaboration and navigation.

**Acceptance Scenarios**:

1. **Given** I have opened a hash key details dialog, **When** I look at the browser URL, **Then** the URL includes the connection ID and hash key name as query parameters (e.g., `/redis/123?key=mykey&type=hash`)
2. **Given** I copy a hash key details URL, **When** I paste it in a new browser tab and navigate to it, **Then** the application loads the Keys tab and automatically opens the hash key details dialog
3. **Given** I share a hash key details URL with a teammate, **When** they open it (and have access), **Then** they see the same hash key details I was viewing
4. **Given** I open a deep link to a hash key that no longer exists, **When** the page loads, **Then** I see an error message indicating the key was not found
5. **Given** I open a deep link to a hash key without authentication, **When** I navigate to the URL, **Then** I am redirected to login and then back to the hash key details after authenticating

---

### User Story 6 - Preserve List Context on Close (Priority: P6)

As a user navigating through multiple hash keys, I want the Keys tab to remember my scroll position and selected key when I close the hash key details dialog so that I can efficiently browse multiple keys without losing my place.

**Why this priority**: Quality-of-life improvement for power users managing many keys. Prevents frustration but not critical for basic functionality.

**Independent Test**: Can be tested by scrolling to a specific position in the keys list, opening a hash key details dialog, closing it, and verifying the list returns to the same scroll position with the key still selected. Delivers value through improved navigation efficiency.

**Acceptance Scenarios**:

1. **Given** I have scrolled down in the Keys tab list, **When** I open a hash key details dialog and then close it, **Then** the list returns to the same scroll position
2. **Given** I have selected a hash key in the Keys tab, **When** I close the hash key details dialog, **Then** the same key remains selected/highlighted in the list
3. **Given** I open hash key details via deep link, **When** I close the dialog, **Then** the list scrolls to show the current key in view
4. **Given** I apply a filter to the keys list and then open a hash key, **When** I close the dialog, **Then** the same filter remains applied and list position is preserved

---

### Edge Cases

- What happens when a hash key is deleted by another client while the details dialog is open?
  - The dialog should detect the deletion on next operation (edit, refresh) and show an error message indicating the key no longer exists
- What happens when a hash key has extremely large field values (e.g., 10MB string)?
  - The system should handle large values gracefully, potentially truncating display with an option to view full value or download
- What happens when a hash key has fields with special characters or Unicode in field names?
  - Field names should be displayed correctly with proper UTF-8 encoding, and editable without encoding issues
- What happens when multiple users edit the same hash field simultaneously?
  - Last write wins (standard Redis behavior), with no conflict detection (Redis doesn't support optimistic locking for hash fields)
- What happens when the user closes the dialog while an edit/save operation is in progress?
  - The operation should complete in the background, or be cancelled with a warning prompt
- What happens when navigating between multiple hash keys rapidly?
  - The dialog should cancel pending requests for the previous key and load the new key cleanly

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: System MUST provide a clickable action in the Keys tab to open hash key details (e.g., double-click, context menu, or dedicated action button)
- **FR-002**: System MUST fetch all field-value pairs for a selected hash key and display them in a dialog or dedicated view
- **FR-003**: System MUST display hash key details in a structured format with field names and values clearly separated (e.g., two-column table)
- **FR-004**: System MUST allow inline editing of hash field values, with save and cancel actions
- **FR-005**: System MUST persist edited field values to the Redis instance via appropriate API calls
- **FR-006**: System MUST validate field values before saving (non-empty values required; encoding is handled by the backend)
- **FR-007**: System MUST detect JSON-formatted field values (strings starting with `{` or `[` and parseable as JSON)
- **FR-008**: System MUST format detected JSON values with indentation and syntax highlighting
- **FR-009**: System MUST provide a button or action to add new fields to the hash
- **FR-010**: System MUST allow users to specify both field name and value when adding a new field
- **FR-011**: System MUST provide a delete action for each hash field with confirmation before deletion
- **FR-012**: System MUST include the connection ID and hash key name in the browser URL when hash key details are open
- **FR-013**: System MUST support deep linking by parsing URL parameters and opening the specified hash key details dialog on page load
- **FR-014**: System MUST preserve the Keys tab list scroll position when opening and closing the hash key details dialog
- **FR-015**: System MUST maintain the selected key highlight in the Keys tab after closing the dialog
- **FR-016**: System MUST display loading indicators while fetching hash key data
- **FR-017**: System MUST display appropriate error messages for failed operations (fetch, save, delete, add)
- **FR-018**: System MUST handle empty hashes (0 fields) by displaying an empty state message
- **FR-019**: System MUST support hashes with up to 1000 fields with acceptable performance
- **FR-020**: System MUST provide a close action (X button, close button, or ESC key) to dismiss the hash key details dialog

### Key Entities _(include if feature involves data)_

- **Hash Key**: A Redis data structure containing multiple field-value pairs, identified by a unique key name within a Redis database connection
- **Hash Field**: An individual field within a hash key, consisting of a field name (string) and field value (string)
- **Field Value**: A string value associated with a hash field, which may contain plain text, serialized JSON, or other data formats
- **Keys Tab Context**: The state of the Keys tab including scroll position, selected key, applied filters, and sort order

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: Users can open hash key details from the Keys tab in under 2 seconds (measured with API response time < 500ms)
- **SC-002**: Users can edit a hash field value and save changes with no more than 3 clicks (click to edit, modify, click to save)
- **SC-003**: JSON values in hash fields are automatically detected and formatted without any user action required
- **SC-004**: Deep links to hash key details work reliably with a 95%+ success rate (valid keys load correctly when URL is shared)
- **SC-005**: List context (scroll position and selection) is preserved in 100% of dialog open/close cycles
- **SC-006**: Users can add a new hash field in under 10 seconds (click add, enter name/value, save)
- **SC-007**: Users can delete a hash field with confirmation in under 5 seconds
- **SC-008**: The hash key details dialog displays up to 1000 fields with initial load time under 3 seconds
- **SC-009**: Field validation prevents 100% of attempts to save empty field values

## Assumptions

- Users are authenticated and have permission to view and modify Redis data for the selected connection
- The backend API provides endpoints for fetching hash fields (`HGETALL`), setting field values (`HSET`), deleting fields (`HDEL`), and adding new fields (`HSET`)
- Hash keys typically contain between 1 to 1000 fields; extremely large hashes (10,000+ fields) are rare and may require pagination in future iterations
- JSON detection uses simple heuristics (starts with `{` or `[`) and attempts to parse; invalid JSON is treated as plain text
- List context preservation relies on browser state management (component state or Pinia store), not server-side session
- Dialog can be opened from the Keys tab's existing UI structure (likely the `RedisKeysExplorer` component)
- Changes to hash fields are persisted immediately upon save (no batch/transaction mode required)
- The application already has components for dialog display, form validation, and error handling that can be reused
- Deep linking follows existing route structure for Redis connections (`/redis/[id]`) with additional parameters for key type and key name
- Users have stable internet connectivity for API operations; offline editing is out of scope
- The hash key details dialog does not need to support concurrent editing conflict detection (last write wins is acceptable)
- Mobile responsive design for hash key details is out of scope for this iteration (desktop/tablet only)

