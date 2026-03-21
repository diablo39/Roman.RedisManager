# Feature Specification: Redis Search Metadata Expansion

**Feature Branch**: `001-redis-search-attributes`  
**Created**: 2026-03-07  
**Status**: Ready for Implementation  
**Input**: User description: "Additional attributes returned from redis search endpoint. As an API user, when I search for redis keys I'd like to see additional attributes: type, ttl, and suggestions for any other useful attributes. Backward compatibility is not required. Limit number of requests to redis."

## User Scenarios & Testing *(mandatory)*

### Mutation Quality Requirement *(mandatory when tests change)*

- If this feature adds or modifies tests, the implementation MUST follow `run -> analyze -> improve -> rerun` for mutation quality.
- Mutation evidence MUST be recorded with:
- HTML and JSON mutation reports for each run.
- Analysis of surviving and no-coverage mutants.
- Assertion/test updates tied to actionable findings.
- A follow-up run showing improvement or approved non-actionable exceptions.

### User Story 1 - See Type and TTL in Search Results (Priority: P1)

As an API user, I want each returned key in search results to include its key type and TTL so I can understand what the key stores and how long it will remain available.

**Why this priority**: This is the core user value explicitly requested and directly improves key inspection workflows.

**Independent Test**: Can be fully tested by searching keys that include a mix of expiring and persistent keys and verifying each result includes `type` and `ttlMilliseconds`.

**Acceptance Scenarios**:

1. **Given** a user searches keys and matching keys exist, **When** the response is returned, **Then** each key entry includes `type` and `ttlMilliseconds` fields.
2. **Given** a matching key has no expiration, **When** the response is returned, **Then** `ttlMilliseconds` is `null` and `hasExpiration` is `false`.
3. **Given** a matching key has expiration set, **When** the response is returned, **Then** `ttlMilliseconds` contains a non-negative remaining lifetime value and `hasExpiration` is `true`.

---

### User Story 2 - Get Additional Useful Key Attributes (Priority: P2)

As an API user, I want additional attributes that reduce manual interpretation so I can quickly decide which keys to inspect further.

**Why this priority**: The user asked for useful additional attributes; derived expiration information provides immediate value without requiring users to infer state from raw TTL alone.

**Independent Test**: Can be tested by searching keys with mixed expiration behavior and verifying the response includes derived expiration attributes.

**Acceptance Scenarios**:

1. **Given** a search response includes `ttlMilliseconds`, **When** key entries are returned, **Then** each key includes `hasExpiration` to indicate whether an expiration policy exists.
2. **Given** keys with and without expiry are returned, **When** the response is returned, **Then** each key includes `hasExpiration` with values that distinguish expiring keys from persistent keys.

---

### User Story 3 - Keep Search Calls Efficient at Scale (Priority: P3)

As an API user, I want search to remain responsive even for larger pages so key discovery stays usable and does not overload the Redis backend.

**Why this priority**: Request-volume control protects system stability and keeps endpoint behavior predictable under load.

**Independent Test**: Can be tested by requesting a page size over the allowed maximum and verifying the system enforces a cap while still returning enriched attributes.

**Acceptance Scenarios**:

1. **Given** a user requests a page size above the allowed limit, **When** the search executes, **Then** the system enforces the configured maximum and returns a valid response.
2. **Given** a valid page request, **When** search executes, **Then** metadata enrichment issues at most two metadata commands per returned key plus scan/continuation commands needed to obtain the page.

---

### Edge Cases

- Search returns zero keys: response remains valid and includes an empty keys list.
- A key disappears between key discovery and metadata retrieval: response does not fail the entire request; affected key is represented with `type = "Unknown"`, `ttlMilliseconds = null`, and `hasExpiration = false`.
- TTL changes between retrieval steps: returned TTL reflects retrieval-time state and is allowed to vary across repeated calls.
- Mixed key populations (persistent and expiring) in one page: all returned keys still include consistent metadata fields.
- Requested page size is null, zero, negative, or very large: system applies safe defaults and maximum limits.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST include `type` for every key item returned by the Redis key search endpoint.
- **FR-002**: System MUST include `ttlMilliseconds` for every key item returned by the Redis key search endpoint.
- **FR-003**: System MUST represent keys without expiration as `ttlMilliseconds = null` and `hasExpiration = false`.
- **FR-004**: System MUST include `hasExpiration` for every returned key item.
- **FR-005**: System MUST return the same metadata field set (`key`, `type`, `ttlMilliseconds`, `hasExpiration`) for every key item in a response, regardless of key type.
- **FR-006**: System MUST define and validate a configured maximum allowed page size for key search requests.
- **FR-007**: System MUST apply the configured maximum page size as the effective page size when clients request values above the limit.
- **FR-008**: System MUST maintain functional correctness of pagination and continuation behavior when metadata fields are included.
- **FR-009**: System MUST tolerate per-key metadata retrieval issues without failing the entire search request, using fallback mapping `type = "Unknown"`, `ttlMilliseconds = null`, `hasExpiration = false`.
- **FR-010**: System MUST allow response schema changes required by this feature; backward compatibility with prior response shape is not required.
- **FR-011**: System MUST bound metadata enrichment request volume to at most two metadata commands per returned key, plus scan/continuation commands required to collect the page.

### Key Entities *(include if feature involves data)*

- **Key Search Result Item**: Represents one returned Redis key with identifier, type, ttlMilliseconds, and hasExpiration.
- **Key Search Response**: Represents paged search output containing key items and pagination-related information.
- **Search Constraints**: Represents request-side boundaries such as requested page size and enforced maximum page size.

## Assumptions

- `ttlMilliseconds` is represented consistently for all keys, with `null` as the explicit no-expiration representation for persistent keys.
- `hasExpiration` is derived metadata intended to improve usability and reduce client-side interpretation.
- The existing search endpoint remains the primary entry point for key discovery and will be extended rather than replaced.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of key items returned by search include non-missing `type`, `ttlMilliseconds`, and `hasExpiration` fields.
- **SC-002**: 100% of search requests with page size above the configured limit are safely capped and return successful, valid responses.
- **SC-003**: In scripted validation testing of at least 100 capped-size requests, at least 95% complete within 2 seconds under normal operating conditions.
- **SC-004**: In a structured validation checklist of at least 10 representative keys, at least 90% are correctly identified as persistent or expiring from one response view, without additional calls.
