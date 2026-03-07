# Phase 0 Research: Unified Redis Key Search Cursor

## Decision 1: Use a single opaque continuation token for all key-search pages

- Decision: Replace outward numeric/composite cursor exposure with one opaque token string in both request and response.
- Rationale: Meets FR-001 through FR-005 by giving clients one topology-agnostic continuation mechanism.
- Alternatives considered:
  - Keep numeric cursor for standalone and composite cursor for cluster: rejected because it keeps contract divergence.
  - Keep mixed response shape (`cursor` + `nodeCursors`): rejected because it leaks internal topology state.

## Decision 2: Encode token as versioned Base64URL payload with integrity protection

- Decision: Use a versioned payload encoded as Base64URL and integrity-protected with HMAC-SHA256.
- Rationale: Provides opaque, URL-safe, stateless continuation tokens while preventing tampering and accidental edits.
- Alternatives considered:
  - Plain Base64 JSON without signature: rejected due to tampering risk and context spoofing.
  - Server-side token session store: rejected due to extra state management and expiry cleanup complexity.
  - JWT/JWE: rejected as unnecessary overhead for endpoint-local pagination state.

## Decision 3: Bind token to search context and reject mismatches

- Decision: Token payload includes context-binding material (group, pattern, page-size fingerprint and token version), validated on resume.
- Rationale: Enforces FR-006 and FR-007, preventing silent restarts or cross-context token replay.
- Alternatives considered:
  - Accept token with changed search parameters and restart automatically: rejected because it hides errors and can skip/duplicate results.
  - Validate only `groupId`: rejected as insufficient protection for pattern/page-size mismatches.

## Decision 4: Keep internal split for standalone vs cluster scan state

- Decision: Maintain separate internal state models for standalone and cluster scans, but map both to the same external token envelope.
- Rationale: Aligns with FR-009 and current repository behavior while preserving consumer simplicity.
- Alternatives considered:
  - Force one internal algorithm for both topologies: rejected because cluster scan orchestration differs from standalone cursor progression.
  - Expose topology mode in API response: rejected due to abstraction leak.

## Decision 5: Completion semantics use `hasMoreResults` + nullable next token

- Decision: Response indicates completion by `hasMoreResults=false` and `continuationToken=null` (or omitted), otherwise provides next token.
- Rationale: Meets FR-010 without requiring consumers to interpret magic numeric values.
- Alternatives considered:
  - Return `"0"` sentinel token at completion: rejected as legacy-cursor leakage.
  - Return completion only via token absence, no explicit boolean: rejected due to weaker readability and client ergonomics.

## Decision 6: Define explicit client error categories for token failures

- Decision: Invalid, malformed, mismatched, expired, or non-resumable tokens return client-visible validation errors with restart guidance.
- Rationale: Required by FR-007 and user story 2; reduces retry loops and ambiguous failures.
- Alternatives considered:
  - Treat invalid token as new search: rejected as data-integrity risk and hidden behavior.
  - Return generic 500 error: rejected because failures are client input/state issues.

## Research Outcome

All technical ambiguities are resolved for planning:

- Token format: versioned opaque token with integrity validation.
- Resume rules: strict context validation, no silent restart.
- Internal architecture: standalone and cluster split retained internally.
- Contract direction: one request/response token field and no `nodeCursors` exposure.

## Migration Notes

- Request query parameter `cursor` is replaced by `continuationToken` for `GET /api/redis-keys`.
- Response field `cursor` is removed and replaced by `continuationToken`.
- Response field `nodeCursors` is removed from the public contract.
- Clients must treat `continuationToken` as opaque and replay it unchanged.
- Completion is now determined by `hasMoreResults=false` with no next token.
