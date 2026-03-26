# Quickstart: String Key Details View

**Feature**: 009-string-key-details
**Date**: 2026-03-25

## Prerequisites

- Node.js 18+ and npm
- Running Redis Manager backend (for API calls)
- OIDC authentication configured

## Setup

```bash
cd frontend

# Install new dependency for JSON syntax highlighting
npm install vue-codemirror @codemirror/lang-json

# Start dev server
npm run dev
```

## Files to Create

| File | Purpose |
|------|---------|
| `src/pages/redis/[id]/string.vue` | Key detail page (file-based route: `/redis/:id/string?key=...`) |
| `src/components/StringKeyDetail.vue` | Main detail component with edit form |

## Files to Modify

| File | Change |
|------|--------|
| `src/api/redisKeys.ts` | Add `getStringKeyValue()` and `getKeyMetadata()` functions |
| `src/components/RedisKeysExplorer.vue` | Add click handler on key names to navigate to detail page |

## Development Flow

1. **API client first**: Add the two new fetch functions to `redisKeys.ts`
2. **Component**: Build `StringKeyDetail.vue` with loading, display, edit, and save states
3. **Page**: Create `src/pages/redis/[id]/string.vue` that extracts route params and renders the component
4. **Navigation**: Modify `RedisKeysExplorer.vue` to make key names clickable (link to detail page)
5. **Test**: Add unit tests for JSON detection logic and component tests for the detail view

## Verification

```bash
# Type check
npm run type-check

# Lint
npm run lint

# Build
npm run build
```

### Manual Test Scenarios

1. Navigate to a Redis server → Keys tab → search for keys → click a string key → verify detail page loads
2. Verify JSON values are pretty-printed with syntax highlighting
3. Edit value → verify Save button enables → save → verify success
4. Edit TTL → save → verify TTL updated
5. Click Cancel → verify return to keys list with search preserved
6. Copy URL → paste in new tab → authenticate → verify detail page loads correctly
7. Navigate to a non-existent key URL → verify error state with back navigation
