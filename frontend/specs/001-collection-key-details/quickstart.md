# Quickstart: Collection Key Details

**Feature**: 001-collection-key-details
**Date**: 2026-03-28

## Prerequisites

- Node.js (for frontend dev server)
- Backend API running (provides Redis data endpoints)
- Redis instance with sample set, list, and sorted set keys

## Development Setup

```bash
cd frontend
npm install
npm run dev    # Dev server on http://localhost:3000
```

## Files to Create

| File | Purpose |
|------|---------|
| `src/components/SetKeyDetailDialog.vue` | Set member viewing/editing dialog |
| `src/components/ListKeyDetailDialog.vue` | List item viewing/editing dialog |
| `src/components/SortedSetKeyDetailDialog.vue` | Sorted set entry viewing/editing dialog |

## Files to Modify

| File | Change |
|------|--------|
| `src/api/redisKeys.ts` | Add 6 new API functions (3 read + 3 remove) and their TypeScript interfaces |
| `src/components/RedisKeysExplorer.vue` | Make set/list/zset keys clickable (currently only string/hash) |
| `src/pages/redis/[id].vue` | Mount 3 new dialogs, extend `onOpenKey` and `checkDeepLink` for new types |

## Verification

1. Run `npm run build` — TypeScript compilation must succeed
2. Run `npm run type-check` — no type errors
3. Run `npm run lint` — no lint errors
4. Open browser at `http://localhost:3000`, navigate to a Redis server
5. In Keys tab, click a set/list/zset key — dialog should open
6. Test deep link: manually add `?key=mykey&type=set&tab=keys` to URL
7. Use Playwright MCP to screenshot and validate UI rendering

## Key Patterns to Follow

- **Dialog pattern**: Follow `HashKeyDetailDialog.vue` — ref-based `open()` method, emit `close`, AbortController
- **API pattern**: Follow `getHashFields()` / `removeHashFields()` — same error handling and auth headers
- **Deep link pattern**: Follow existing `onOpenKey` / `checkDeepLink` — extend type conditions
- **Design system**: `.card-header-separated`, `.card-footer-separated`, type chips with `variant="tonal"` and `size="x-small"`
