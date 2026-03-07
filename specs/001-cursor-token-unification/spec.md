# Feature Specification: Unified Redis Key Search Cursor

**Feature Branch**: `[001-cursor-token-unification]`  
**Created**: 2026-03-07  
**Status**: Draft  
**Input**: User description: "Currently I have two types of cursors: one for standalone server and other for redis cluster. I'd like to have have coherent approach for cursor. Contract breaking changes are OK, noone use the api. Split in the code for standalone and cluster are ok. Invent encoding mechanism that will hide complexicity for the api user."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Continue searches with one cursor contract (Priority: P1)

As an API consumer browsing Redis keys, I want one continuation token model for both standalone and cluster groups so my client can page through results without knowing the Redis topology.

**Why this priority**: This is the core user value. The feature exists to remove contract ambiguity and topology leakage from the key search API.

**Independent Test**: Can be fully tested by starting a key search against a standalone group and a cluster group, then replaying the returned continuation token unchanged for the next page in both cases.

**Acceptance Scenarios**:

1. **Given** a first-page key search against a standalone group with more matching keys than fit in one page, **When** the user submits the search, **Then** the response includes a continuation token the user can pass back unchanged to retrieve the next page.
2. **Given** a first-page key search against a cluster group with more matching keys than fit in one page, **When** the user submits the search, **Then** the response uses the same continuation token contract as standalone search and does not expose node-level scan details.
3. **Given** a valid continuation token returned from a prior key search response, **When** the user submits it with the same search context, **Then** the system returns the next page of keys and, when applicable, a replacement continuation token.

---

### User Story 2 - Fail safely on invalid continuation state (Priority: P2)

As an API consumer, I want invalid or misused continuation tokens to fail clearly so I do not unknowingly restart a scan, skip results, or keep retrying a broken state.

**Why this priority**: A unified cursor contract is only dependable if invalid inputs are rejected predictably rather than being silently coerced into a different search state.

**Independent Test**: Can be fully tested by submitting malformed, mismatched, and stale continuation tokens and verifying that the API returns a client-visible validation failure instead of continuing with incorrect results.

**Acceptance Scenarios**:

1. **Given** a malformed continuation token, **When** the user submits a key search request, **Then** the system responds with 400 Bad Request and an `invalid_continuation_token` error code.
2. **Given** a continuation token issued for one search context, **When** the user reuses it with a different group, pattern, or incompatible page size, **Then** the system responds with 400 Bad Request and a `continuation_context_mismatch` error code.
3. **Given** a continuation token that can no longer be resumed, **When** the user submits it, **Then** the system returns a recoverable client error that tells the user the search must be restarted.

---

### User Story 3 - End searches predictably (Priority: P3)

As an API consumer, I want the API to clearly indicate when a search is finished so I can stop requesting more pages without interpreting special numeric values or topology-specific metadata.

**Why this priority**: Completion semantics are part of a coherent cursor contract and remove the current need to reason about numeric cursor values or cluster-only details.

**Independent Test**: Can be fully tested by paging until completion for both standalone and cluster groups and verifying that the final response clearly communicates that no more results remain.

**Acceptance Scenarios**:

1. **Given** a search that has reached its final page, **When** the system returns the response, **Then** the response includes `hasMoreResults=false` and no continuation token so the client knows to stop.
2. **Given** a search pattern that matches no keys, **When** the user submits the search, **Then** the system returns an empty result page with completion semantics that do not require a follow-up request.

### Edge Cases

- What happens when the user omits the continuation token on the first request? The system starts a new search without requiring topology-specific defaults.
- What happens when the user submits an empty continuation token after a completed search? The system treats it as a new search only when it is presented as a new search request rather than a continuation of a prior search.
- How does the system handle a continuation token after the Redis topology changes or the underlying search state can no longer be resumed? The system rejects the token with a clear restart-required error.
- How does the system handle a continuation token that was issued for one search pattern and replayed with another pattern? The system rejects the token instead of silently restarting or mixing contexts.
- How does the system handle a page size change during continuation? The system either validates that the change is compatible or rejects the token with a 400 response and a context‑mismatch error code.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide one continuation token contract and response shape for Redis key search across standalone and cluster server groups, hiding topology details.
- **FR-002**: The system MUST allow users to start a new key search without knowing whether the target group is standalone or cluster.
- **FR-003**: (see FR-001)
- **FR-004**: The system MUST treat the continuation token as an opaque value that users store and replay without inspecting or modifying it.
- **FR-005**: The system MUST NOT expose node indexes, node cursors, host identifiers, per-node progress, or other topology-specific scan state in the key search API contract.
- **FR-006**: The system MUST validate that a continuation token belongs to the same search context in which it was issued.
- **FR-007**: The system MUST reject malformed, mismatched, or no-longer-valid continuation tokens with a client-visible validation error instead of silently restarting the search.
- **FR-008**: The system MUST resume an accepted continuation token from the next eligible search position rather than restarting from the beginning.
- **FR-009**: The system MUST allow standalone and cluster searches to use different internal continuation strategies as long as the external contract remains identical.
- **FR-010**: The system MUST clearly indicate when no more key search results remain so users can stop requesting additional pages.
- **FR-011**: The system MUST continue to support pattern-based key search and caller-supplied page sizes while using the unified continuation token contract.
- **FR-012**: The system MUST allow breaking changes to the existing cursor-related response shape so the contract can be simplified before the API is adopted.
- **FR-013**: The system MUST provide examples and documentation that show the same continuation flow for standalone and cluster searches.

### Non-Functional Requirements

- **NFR-001 (Security/Integrity)**: Continuation tokens MUST be integrity-protected so tampered tokens are rejected with a client-visible 400 error.
- **NFR-002 (Performance)**: Key search continuation MUST NOT add extra network round-trips per request beyond the existing SCAN flow for the selected topology.
- **NFR-003 (Observability)**: Continuation token rejection paths MUST produce structured logs including an error code and correlation or trace identifier.
- **NFR-004 (Determinism)**: For the same valid continuation context, token replay MUST deterministically continue from the next eligible position.

### Key Entities *(include if feature involves data)*

- **Key Search Request**: A user-initiated search for keys within one Redis server group, including the pattern, requested page size, and optional continuation token.
- **Key Search Page**: A single page of key search results containing the returned keys, completion state, and any follow-up continuation token needed for the next page.
- **Continuation Token**: An opaque server-issued value representing resumable search state for a specific key search context.

## Assumptions

- Existing consumers do not rely on the current cursor contract, so simplifying the request and response shape is acceptable.
- Continuation tokens are intended for sequential reuse within the same key search context rather than as permanent bookmarks.
- If the underlying Redis topology or search context changes enough to invalidate continuation state, restarting the search is an acceptable recovery path.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A client can use one request and response continuation flow to retrieve at least three consecutive key-search pages from both a standalone group and a cluster group without topology-specific branching.
- **SC-002**: In feature validation, 100% of tested valid continuation tokens resume the intended search context without requiring the client to interpret numeric cursor values or node-level metadata.
- **SC-003**: In feature validation, 100% of tested malformed, mismatched, or stale continuation tokens are rejected with a client-visible error instead of silently restarting the search.
- **SC-004**: Users can determine from the final response alone whether a search is complete, without relying on special zero values or cluster-only metadata.
