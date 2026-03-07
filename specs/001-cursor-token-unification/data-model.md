# Data Model: Unified Redis Key Search Cursor

## Entity: KeySearchRequest

- Purpose: Represents a single search request from API consumer.
- Fields:
  - `groupId` (Guid, required)
  - `pattern` (string, required, default `"*"` when omitted)
  - `pageSize` (int, required, positive)
  - `continuationToken` (string?, optional for first page)
- Validation Rules:
  - `groupId` must not be empty.
  - `pattern` must not be null; empty maps to `"*"`.
  - `pageSize` must be > 0 and within configured API limits.
  - `continuationToken`, if present, must be decodable and integrity-valid.

## Entity: ContinuationTokenEnvelope

- Purpose: Opaque, versioned representation of resumable scan state.
- Fields:
  - `version` (byte/string enum, required)
  - `contextHash` (string/bytes, required)
  - `mode` (`standalone` or `cluster`, required)
  - `issuedAtUtc` (timestamp, optional for expiry policy)
  - `state` (polymorphic state payload, required)
  - `signature` (bytes, required for tamper detection)
- Validation Rules:
  - `version` must be recognized.
  - `contextHash` must match current request context.
  - `signature` must verify successfully.
  - `state` must match declared `mode`.

## Entity: StandaloneCursorState

- Purpose: Internal resume state for standalone Redis SCAN.
- Fields:
  - `cursor` (long, required)
- Validation Rules:
  - `cursor >= 0`.

## Entity: ClusterCursorState

- Purpose: Internal resume state for multi-master cluster scans.
- Fields:
  - `nodeIndex` (int, required)
  - `nodeCursor` (long, required)
  - `topologyFingerprint` (string/int, optional but recommended)
- Validation Rules:
  - `nodeIndex >= 0`.
  - `nodeCursor >= 0`.
  - When topology fingerprint is present, it must match resumable topology or token is rejected.

## Entity: KeySearchPage

- Purpose: API response page for key search.
- Fields:
  - `keys` (collection of `RedisKeyDto`, required)
  - `hasMoreResults` (bool, required)
  - `continuationToken` (string?, nullable when no more results)
- Validation Rules:
  - If `hasMoreResults == true`, `continuationToken` must be present.
  - If `hasMoreResults == false`, `continuationToken` must be null or absent.
  - Response must not contain topology-specific cursor fields.

## Entity: ContinuationError

- Purpose: Normalized client-visible error for invalid continuation flow.
- Fields:
  - `code` (string, required; e.g., `invalid_continuation_token`, `continuation_context_mismatch`, `continuation_not_resumable`)
  - `message` (string, required)
- Validation Rules:
  - `code` must map to documented contract cases.
  - Error must indicate restart behavior when resume is impossible.

## Relationships

- `KeySearchRequest` optionally contains one `ContinuationTokenEnvelope`.
- `ContinuationTokenEnvelope` contains either one `StandaloneCursorState` or one `ClusterCursorState`.
- `KeySearchPage` may emit one next `ContinuationTokenEnvelope` serialized as opaque token.

## State Transitions

1. `NewSearch` -> `PageReturnedWithToken`: first request without token returns first page and next token if more data exists.
2. `PageReturnedWithToken` -> `ContinuationAccepted`: next request with valid token resumes search.
3. `ContinuationAccepted` -> `Completed`: final page returns `hasMoreResults=false` and no token.
4. `PageReturnedWithToken` -> `ContinuationRejected`: malformed/mismatched/expired/non-resumable token returns client error; user restarts search.
