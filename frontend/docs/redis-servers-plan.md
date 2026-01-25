# Redis Servers Integration Plan

## Goals

- Load Redis server list from backend using BACKEND_URL, with pagination and full-page loading overlay.
- Replace hardcoded servers in [src/components/Menu.vue](src/components/Menu.vue) with live data.
- Keep implementation small, typed, and aligned with existing Vue 3 + Vuetify + Pinia patterns.

## Environment & Config

- Add `BACKEND_URL` to `.env` (e.g., `BACKEND_URL=http://localhost:5000`).
- Create [src/api/config.ts](src/api/config.ts) exporting `apiBaseUrl`:
  - Resolve from `import.meta.env.BACKEND_URL`; default to `/` when unset.
  - Ensure trailing slash handling so `apiBaseUrl + 'api/RedisServers'` is correct.

## API Client

- File: [src/api/redisServers.ts](src/api/redisServers.ts)
- Types:
  - `RedisServerDto` → `{ id: string; name: string; }`
  - `RedisServersQueryResult` → `{ servers: RedisServerDto[]; totalCount: number; pageNumber: number; pageSize: number; }`
- Function: `getRedisServers(pageNumber: number, pageSize: number, signal?: AbortSignal)`
  - Build URL: `${apiBaseUrl}api/RedisServers?pageNumber=${pageNumber}&pageSize=${pageSize}`
  - Use `fetch`; on non-2xx throw `Error` with status text.
  - Parse JSON to `RedisServersQueryResult`.
  - Pass through `signal` for cancellation (optional now, helpful later).

## Store

- File: [src/stores/redisServers.ts](src/stores/redisServers.ts)
- State: `servers: RedisServerDto[]`, `pageNumber: number`, `pageSize: number`, `totalCount: number`, `loading: boolean`, `error: string | null`.
- Getters: `hasPrev` (pageNumber > 1), `hasNext` (pageNumber \* pageSize < totalCount).
- Actions:
  - `async fetchServers(direction?: "next" | "prev")`
    - Adjust pageNumber within bounds (min 1, max based on totalCount when known).
    - Set `loading = true`, `error = null`; call `getRedisServers`.
    - Update list and pagination fields; ensure loading reset in `finally`.
    - On error, set friendly message.

## Menu Integration

- File: [src/components/Menu.vue](src/components/Menu.vue)
- On mount: call `fetchServers()` for first page.
- Render states:
  - Loading: show small `v-progress-circular` or text within list area while global overlay covers page.
  - Error: show `v-alert`/`v-list-item` with retry button that calls `fetchServers()`.
  - Success: list servers via `v-list-item` with `:title="server.name"`.
- Pagination controls inside Redis Servers section footer:
  - Prev/Next buttons (`v-btn` small/flat) bound to `fetchServers('prev')` / `fetchServers('next')` and disabled when `!hasPrev` / `!hasNext`.
  - Display page info (e.g., `Page X of Y` using `Math.ceil(totalCount / pageSize)` when totalCount > 0).
- Selection: keep existing selection handler; optionally emit selected server id/name.

## Full-Page Loader

- File: [src/layouts/default.vue](src/layouts/default.vue)
- Add `v-overlay` (or absolute container) that covers `v-main` and `v-navigation-drawer` area.
- Bind `model-value` to the store’s `loading` so any fetch shows the overlay.
- Content: centered `v-progress-circular` with accessible label.

## Error Handling

- Keep error messages concise; avoid leaking raw status text.
- Retry path via button in Menu and allow user to continue paging.

## Testing & Verification

- Manual: navigate to page, see loader overlay during initial fetch; verify prev/next enable/disable; ensure server names render.
- Edge: empty list shows friendly “No servers found”.
- Commands: `npm run type-check`, `npm run lint`.

## Notes

- Use `<script setup lang="ts">`; no manual imports for Vue/Pinia helpers (auto-imported).
- Stick to Vuetify components; avoid custom CSS unless utility classes are insufficient.
- Keep ASCII-only content; avoid non-ASCII characters.
