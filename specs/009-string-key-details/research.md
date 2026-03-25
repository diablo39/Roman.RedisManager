# Research: String Key Details View

**Feature**: 009-string-key-details
**Date**: 2026-03-25

## Research Topics

### 1. JSON Syntax Highlighting Library for Editable Value Field

**Decision**: Use `vue-codemirror` (v6) with `@codemirror/lang-json`

**Rationale**:
- CodeMirror 6 is a purpose-built code editor (~50-80 KB gzipped), not a full IDE like Monaco (~4+ MB)
- `vue-codemirror` provides Vue 3 integration with `v-model` support, matching the project's reactive patterns
- Supports both JSON syntax highlighting and plain text editing by conditionally applying the `json()` language extension
- Fully typed in TypeScript, aligns with the project's strict TypeScript requirement
- Actively maintained; modular architecture allows tree-shaking
- If the wrapper ever becomes unmaintained, the fallback to raw `@codemirror/*` packages requires only ~30 lines of custom code

**Alternatives considered**:
- **Shiki**: Display-only; does not support editing. Rejected.
- **highlight.js + textarea overlay**: Fragile scroll sync, caret issues, accessibility problems. Rejected.
- **Monaco Editor**: ~4+ MB bundle; massively oversized for a single JSON field. Rejected.
- **Plain `<v-textarea>` with `JSON.stringify` formatting**: No syntax highlighting. Does not meet FR-003 requirement. Rejected.
- **Raw CodeMirror 6 (no Vue wrapper)**: Viable but requires manual lifecycle wiring. `vue-codemirror` eliminates this boilerplate at minimal cost (~1-2 KB).

**Install**:
```bash
npm install vue-codemirror @codemirror/lang-json
```

---

### 2. URL Routing Strategy for Key Detail Deep Links

**Decision**: Use file-based routing with a query parameter for the key name, not a path parameter.

**Rationale**:
- Redis key names can contain any character, including `/`, `.`, `{`, `}`, spaces, and binary data. Path parameters would conflict with route matching even with encoding.
- Using a query parameter (`?key=my:key:name`) avoids path-parsing ambiguity and naturally supports URL-encoding of special characters.
- The page file will be `src/pages/redis/[id]/string.vue`, giving the route `/redis/:id/string?key=my:key:name`.
- The `groupId` is already part of the path as `:id`, and the key name is passed as a query parameter.
- This allows `router.push({ path: '/redis/${groupId}/string', query: { key: keyName } })` for clean navigation.

**Alternatives considered**:
- **Path parameter `[key].vue`**: Redis keys with `/` or `.` break path matching. Rejected.
- **Base64-encoded path parameter**: Unreadable URLs, cumbersome to share. Rejected.
- **Catch-all route `[...key].vue`**: Overly complex for a single parameter. Rejected.

---

### 3. State Preservation for Cancel Navigation

**Decision**: Use `router.back()` when navigating from the keys list; fall back to `router.push` to the server page with Keys tab when arriving via direct URL.

**Rationale**:
- When the user navigated from the keys list, `router.back()` naturally restores Vue Router's history state, including scroll position and any in-memory search state held by the `RedisKeysExplorer` component (which uses `keep-alive` behavior through Vue Router's default caching).
- For direct URL access, there is no browser history to go back to, so we push to `/redis/:id` with a query parameter to activate the Keys tab.
- Detection: Check `window.history.length > 1` and/or store a `from` query parameter when navigating to the detail page.

**Alternatives considered**:
- **Pinia store for search state**: Would require persisting search pattern, results, and scroll position. Adds complexity for marginal benefit since `router.back()` handles this naturally. Rejected for now.
- **LocalStorage for search state**: Same complexity as Pinia, plus stale state risks. Rejected.

---

### 4. JSON Detection Strategy

**Decision**: Use `JSON.parse()` with try/catch to detect valid JSON. Apply the CodeMirror `json()` language extension conditionally.

**Rationale**:
- `JSON.parse()` is the authoritative way to determine if a string is valid JSON in JavaScript.
- Detection happens once on load and whenever the user switches between raw/formatted views.
- The detection is display-side only; the save operation always sends the raw string value regardless of whether it was detected as JSON.
- Performance: `JSON.parse()` on typical Redis string values (< 1 MB) completes in < 10ms.

**Alternatives considered**:
- **Regex-based detection**: Unreliable for edge cases (nested structures, escaped characters). Rejected.
- **Content-Type header from API**: The API does not provide content-type metadata for values. Not available.

---

### 5. API Client Functions Needed

**Decision**: Add two new functions to `src/api/redisKeys.ts`:
1. `getStringKeyValue(groupId, key, signal?)` - calls `GET /api/redis/data/strings?groupId=...&key=...`
2. `getKeyMetadata(key, groupId, signal?)` - calls `GET /api/redis-keys/{key}/metadata?groupId=...`

Reuse the existing `createStringKey` function for saves (it calls `POST /api/redis/data/strings` which sets/updates).

**Rationale**:
- The `GET /api/redis/data/strings` endpoint returns just `{ value: string | null }`, which is the simplest way to get the string value.
- The `GET /api/redis-keys/{key}/metadata` endpoint returns `{ metadata: { type, ttlMilliseconds } }`, providing the TTL and type.
- These two calls together provide all fields needed for the detail page.
- The existing `createStringKey` already wraps `POST /api/redis/data/strings` with `SetStringRequest { groupId, key, value, ttl, condition }`, which is exactly what's needed for save.

**Alternatives considered**:
- **Use `GET /api/redis-keys/{key}/value`**: Returns a polymorphic result with all types (`stringValue`, `listValues`, etc.). More data than needed for a string-only detail page. Could be used, but the dedicated string endpoint is cleaner.
