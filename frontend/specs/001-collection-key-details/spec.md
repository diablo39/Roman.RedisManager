# Feature Specification: Collection Key Details (Sets, Sorted Sets, Lists)

**Feature Branch**: `001-collection-key-details`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "Frontend for sets, sorted sets and lists — open details from Keys tab, inspect/edit fields, format JSON values, deep link to dialog, close without losing list context, close dialog after save. Use Playwright MCP to validate implementation."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Set Members (Priority: P1)

A user browsing keys in the Keys tab clicks on a key of type "set". A detail dialog opens showing all members of the set. The user can see each member value, scroll through large sets with pagination, and identify JSON-formatted members at a glance.

**Why this priority**: Sets are one of the most commonly used Redis data structures. Viewing members is the foundational capability that all editing features build upon.

**Independent Test**: Can be fully tested by navigating to a Redis server with set keys, clicking a set key in the Keys tab, and confirming members are displayed with correct values and pagination.

**Acceptance Scenarios**:

1. **Given** a Redis server with set keys, **When** the user clicks a set key in the Keys tab, **Then** a detail dialog opens showing the key name, type chip ("set"), member count, and a list of all members.
2. **Given** a set with more members than the default page size, **When** the dialog opens, **Then** the first page of members is shown with a "Load More" button to fetch additional members.
3. **Given** a set member containing a JSON string, **When** the member is displayed, **Then** the value is identified as JSON and can be formatted/pretty-printed.

---

### User Story 2 - View List Items (Priority: P1)

A user clicks on a key of type "list" in the Keys tab. A detail dialog opens showing the list items with their index positions. The user can browse items with pagination and view JSON-formatted values.

**Why this priority**: Lists are a fundamental Redis data structure. Index-based viewing is critical for understanding list ordering and content.

**Independent Test**: Can be fully tested by clicking a list key in the Keys tab and confirming items are displayed with correct index positions and pagination.

**Acceptance Scenarios**:

1. **Given** a Redis server with list keys, **When** the user clicks a list key, **Then** a detail dialog opens showing the key name, type chip ("list"), item count, and items displayed with their index positions.
2. **Given** a list with more items than the default page size, **When** the dialog opens, **Then** the first page of items is shown with a "Load More" button.
3. **Given** a list item containing a JSON string, **When** the item is displayed, **Then** the value is identified as JSON and can be formatted/pretty-printed.

---

### User Story 3 - View Sorted Set Entries (Priority: P1)

A user clicks on a key of type "zset" in the Keys tab. A detail dialog opens showing entries with both their member value and score, sorted by score. The user can browse with pagination and view JSON-formatted member values.

**Why this priority**: Sorted sets have dual data (member + score), making them unique. Displaying both is essential for understanding sorted set content.

**Independent Test**: Can be fully tested by clicking a sorted set key and confirming entries display both member and score values in score order.

**Acceptance Scenarios**:

1. **Given** a Redis server with sorted set keys, **When** the user clicks a sorted set key, **Then** a detail dialog opens showing the key name, type chip ("zset"), entry count, and entries with both member and score displayed.
2. **Given** a sorted set with many entries, **When** the dialog opens, **Then** entries are displayed sorted by score with pagination support.
3. **Given** a sorted set member containing a JSON string, **When** the entry is displayed, **Then** the member value is identified as JSON and can be formatted/pretty-printed.

---

### User Story 4 - Edit Set Members (Priority: P2)

A user viewing a set detail dialog wants to add new members, remove existing members, and edit member values. Changes are collected locally and sent in a single batch save. The dialog closes automatically after a successful save.

**Why this priority**: Editing enables users to manage set data directly from the UI, which is the primary value proposition after viewing.

**Independent Test**: Can be tested by opening a set key, adding a member, removing a member, editing a member value, saving, and confirming changes persist in Redis.

**Acceptance Scenarios**:

1. **Given** a set detail dialog is open, **When** the user adds a new member value, **Then** the member appears in the list marked as new, and a dirty indicator shows unsaved changes.
2. **Given** a set detail dialog with members, **When** the user marks a member for removal, **Then** the member is visually marked for deletion and can be restored before saving.
3. **Given** a set detail dialog with unsaved changes, **When** the user clicks Save, **Then** all changes (additions and removals) are sent to the server in a batch, and the dialog closes automatically on success.
4. **Given** a set member value is displayed, **When** the user clicks edit on that member, **Then** a code editor opens with JSON formatting support for JSON values.

---

### User Story 5 - Edit List Items (Priority: P2)

A user viewing a list detail dialog wants to add new items (to the head or tail), remove items, and edit item values. Changes are saved in batch and the dialog closes after save.

**Why this priority**: Editing list items with position-awareness (head/tail push) is essential for list management.

**Independent Test**: Can be tested by opening a list key, adding items to head/tail, removing items, editing values, saving, and confirming changes persist.

**Acceptance Scenarios**:

1. **Given** a list detail dialog is open, **When** the user adds a new item, **Then** the user can choose to add to the beginning (head) or end (tail) of the list.
2. **Given** a list detail dialog with items, **When** the user marks an item for removal, **Then** the item is visually marked for deletion and can be restored before saving.
3. **Given** a list detail dialog with unsaved changes, **When** the user clicks Save, **Then** all changes are sent to the server and the dialog closes automatically on success.
4. **Given** a list item value is displayed, **When** the user clicks edit on that item, **Then** a code editor opens with JSON formatting support.

---

### User Story 6 - Edit Sorted Set Entries (Priority: P2)

A user viewing a sorted set detail dialog wants to add new entries (member + score), remove entries, and edit member values or scores. Changes are saved in batch and the dialog closes after save.

**Why this priority**: Sorted sets require editing both member values and scores, making the editing experience slightly more complex but equally important.

**Independent Test**: Can be tested by opening a sorted set key, adding entries with scores, removing entries, editing scores, saving, and confirming changes persist.

**Acceptance Scenarios**:

1. **Given** a sorted set detail dialog is open, **When** the user adds a new entry, **Then** the user provides both a member value and a numeric score.
2. **Given** a sorted set dialog with entries, **When** the user edits an entry's score, **Then** the updated score is reflected locally and marked as changed.
3. **Given** a sorted set dialog with unsaved changes, **When** the user clicks Save, **Then** all changes (additions, score updates, and removals) are sent to the server and the dialog closes automatically on success.

---

### User Story 7 - Deep Link to Collection Key Dialog (Priority: P2)

A user shares or bookmarks a URL that includes the key name and type. When another user (or the same user) opens that URL, the application navigates to the correct server, switches to the Keys tab, and automatically opens the appropriate detail dialog.

**Why this priority**: Deep linking enables collaboration and bookmarking — important for team productivity but not blocking core functionality.

**Independent Test**: Can be tested by constructing a URL with key and type query parameters, navigating to it, and confirming the correct dialog opens automatically.

**Acceptance Scenarios**:

1. **Given** a URL with query parameters specifying a set/list/zset key and its type, **When** the user navigates to that URL, **Then** the Keys tab is activated and the corresponding detail dialog opens with the key's data loaded.
2. **Given** a user opens a collection key detail dialog from the Keys tab, **When** the dialog opens, **Then** the URL is updated with query parameters reflecting the key name and type.
3. **Given** a user closes a detail dialog, **When** the dialog closes, **Then** the key and type query parameters are removed from the URL without a full page navigation, and the Keys tab list context is preserved.

---

### User Story 8 - TTL Management for Collection Keys (Priority: P3)

A user viewing any collection key detail dialog can view and modify the key's time-to-live (TTL). The TTL picker shows the current expiration and allows setting a new one.

**Why this priority**: TTL management is a secondary concern after viewing and editing data, but important for key lifecycle management.

**Independent Test**: Can be tested by opening a collection key with a TTL, verifying the TTL is displayed, modifying it, saving, and confirming the new TTL is applied.

**Acceptance Scenarios**:

1. **Given** a collection key with an existing TTL, **When** the detail dialog opens, **Then** the TTL picker displays the current expiration broken down into days, hours, minutes, and seconds.
2. **Given** a collection key without a TTL, **When** the user sets a TTL via the picker, **Then** the TTL is included in the save operation and applied to the key.

---

### Edge Cases

- What happens when a collection key has zero members/items/entries? The dialog should show an empty state with a message and option to add new items.
- How does the system handle a key that is deleted by another client while the dialog is open? The save operation should show an appropriate error message.
- What happens when the user tries to add a duplicate member to a set? Since sets enforce uniqueness, the system should warn the user that the member already exists and will be overwritten/ignored.
- How does the system handle very large values (e.g., a list item > 1MB)? The system should display a warning about large values, consistent with the existing string key detail behavior.
- What happens when network errors occur during pagination (Load More)? The system should show an error with a retry option, without losing already-loaded data.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to click on set, sorted set, and list keys in the Keys tab to open a detail dialog (currently only string and hash keys are clickable).
- **FR-002**: System MUST display set members in a scrollable list with pagination support for large sets.
- **FR-003**: System MUST display list items with their index positions in a scrollable list with pagination support.
- **FR-004**: System MUST display sorted set entries showing both member value and score, sorted by score, with pagination support.
- **FR-005**: System MUST detect JSON-formatted values and provide a formatting/pretty-print option in the code editor for all collection types.
- **FR-006**: Users MUST be able to add new members/items/entries to any collection type from the detail dialog.
- **FR-007**: Users MUST be able to mark members/items/entries for removal, with the ability to undo before saving.
- **FR-008**: Users MUST be able to edit individual member/item values using a code editor with JSON syntax highlighting.
- **FR-009**: For sorted sets, users MUST be able to edit the score of an entry.
- **FR-010**: For lists, users MUST be able to choose whether to add items to the head (beginning) or tail (end).
- **FR-011**: System MUST collect all local changes and send them to the server in a single batch save operation.
- **FR-012**: System MUST automatically close the detail dialog after a successful save operation.
- **FR-013**: System MUST update the URL with key name and type query parameters when a dialog is opened, and remove them when closed.
- **FR-014**: System MUST support deep linking — navigating to a URL with key/type parameters opens the corresponding dialog automatically.
- **FR-015**: System MUST preserve the Keys tab list context (search pattern, scroll position, loaded results) when opening and closing detail dialogs.
- **FR-016**: System MUST display a TTL picker in each collection key detail dialog for viewing and modifying key expiration.
- **FR-017**: System MUST show appropriate loading states (skeleton loaders for initial load, progress indicators for pagination and save operations).
- **FR-018**: System MUST show error states with retry options for failed data fetches and save operations.
- **FR-019**: System MUST track dirty state (unsaved changes) and visually indicate when changes exist.
- **FR-020**: System MUST display the member/item/entry count and indicate when more results are available beyond the current page.

### Key Entities

- **Set Member**: A single value belonging to a Redis set. Unique within the set.
- **List Item**: A value at a specific index position in a Redis list. May contain duplicates. Order is significant.
- **Sorted Set Entry**: A member-score pair in a Redis sorted set. Member is unique; score is a numeric value that determines ordering.
- **Collection Key Metadata**: The key name, type, TTL, and total member/item/entry count for a collection key.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can open detail dialogs for all five Redis data types (string, hash, set, list, sorted set) from the Keys tab — achieving 100% type coverage.
- **SC-002**: Users can view and paginate through collection members/items/entries within 2 seconds per page load.
- **SC-003**: Users can complete an edit-and-save workflow (open dialog, make a change, save) in under 30 seconds for simple modifications.
- **SC-004**: Deep links to collection key dialogs resolve correctly on first navigation 100% of the time.
- **SC-005**: The Keys tab list state (search results, scroll position) is fully preserved after opening and closing any detail dialog.
- **SC-006**: All three new detail dialogs follow the same interaction patterns as the existing string and hash detail dialogs, ensuring a consistent user experience.

## Assumptions

- Backend API endpoints for reading list items, set members, and sorted set entries either already exist or will be available by the time frontend implementation begins (read APIs are needed in addition to the existing create APIs).
- The existing cursor-based pagination pattern used for hash fields will be applicable to sets. Lists and sorted sets will use index-based pagination.
- The TTL picker component (TtlPicker.vue) and code editor integration (CodeMirror) are reusable without modification for the new dialogs.
- The dialog pattern established by StringKeyDetailDialog and HashKeyDetailDialog (ref-based open/close, emit events, AbortController for request cancellation) will be followed for all new dialogs.
- The existing deep linking mechanism (URL query parameters: key, type, tab) will be extended to support the three new types without changes to the routing structure.
- Batch save for sets means: additions via SADD and removals via SREM. For lists: additions via LPUSH/RPUSH and removals via LREM (value-based, not index-based). For sorted sets: additions/updates via ZADD and removals via ZREM.
