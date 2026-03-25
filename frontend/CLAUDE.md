# Frontend — Claude Code Instructions

## Overview

Roman Redis Manager is a **frontend UI for managing Redis databases** — connection management, data browsing/manipulation (keys, values, data structures), and administration. This is a frontend-only app communicating with a separate backend API.

## Tech Stack

Vue 3 (v3.5) + Vuetify 3 (v3.10) + TypeScript (v5.9) + Vite (v7.1) + Pinia (v3.0) + Vue Router (v4.5) + unplugin-vue-router (file-based routing)

## Architecture & File Placement

| Category | Location | Naming | Notes |
|----------|----------|--------|-------|
| Components | `src/components/` | PascalCase (`KeyBrowser.vue`) | Auto-imported, reusable UI |
| Pages | `src/pages/` | kebab-case/index (`about.vue`, `redis/[id].vue`) | Filename = route path |
| Layouts | `src/layouts/` | lowercase (`default.vue`) | Contains `<v-main><router-view /></v-main>`, no `<v-app>` |
| Stores | `src/stores/` | camelCase file (`redisServers.ts`) | Export `useXxxStore`, Options API only |
| API clients | `src/api/` | camelCase (`redisKeys.ts`) | Authenticated fetch with error handling |
| Styles | `src/styles/settings.scss` | — | Global SCSS overrides and utilities |
| Plugins | `src/plugins/` | — | Registration order: Vuetify → Router → Pinia |
| Assets | `src/assets/` or `public/` | — | Static assets |

## Auto-imports (do NOT manually import these)

- Vue APIs: `ref`, `computed`, `watch`, `onMounted`, `reactive`, etc.
- Vue Router: `useRouter`, `useRoute`
- Pinia: `defineStore`, `storeToRefs`
- All stores from `src/stores/`
- All components from `src/components/`

## Auto-generated files (NEVER edit manually)

- `src/auto-imports.d.ts`
- `src/components.d.ts`
- `src/typed-router.d.ts`

---

## Conventions & Patterns

### Component Template

```vue
<template>
  <v-container>
    <v-card rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-database" size="20" />
          Title
        </div>
      </div>
      <v-card-text>Content</v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  // Vue APIs auto-imported — no imports needed
  const count = ref(0)
</script>
```

### Page Template

```vue
<template>
  <v-container fluid class="pa-6">
    <div class="mb-6">
      <div class="text-h5 font-weight-medium">Page Title</div>
      <div class="text-body-2 text-medium-emphasis">Subtitle</div>
    </div>
    <ComponentName />
  </v-container>
</template>

<script setup lang="ts">
  // Minimal — delegate to components
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Page Title'
</route>
```

### Store Template (Options API — required)

```typescript
export const useXxxStore = defineStore('xxx', {
  state: () => ({
    items: [] as Item[],
    loading: false,
    error: null as string | null,
  }),

  getters: {
    itemCount(): number {
      return this.items.length
    },
  },

  actions: {
    async fetchItems() {
      this.loading = true
      try {
        this.items = await api.getItems()
      } finally {
        this.loading = false
      }
    },
  },
})
```

### API Client Template

Follow `src/api/redisServers.ts` as the reference:

```typescript
import { getAuthHeaders } from './authentication'
import { apiBaseUrl } from './config'

interface ProblemDetails {
  title?: string | null
  detail?: string | null
}

async function parseErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const payload = (await response.json()) as ProblemDetails
    return payload.detail || payload.title || fallback
  } catch {
    return fallback
  }
}

export async function fetchSomething(
  id: string,
  signal?: AbortSignal,
): Promise<SomeResult> {
  const url = `${apiBaseUrl}api/endpoint/${encodeURIComponent(id)}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    throw new Error(await parseErrorMessage(response, 'Failed to fetch'))
  }

  return await response.json()
}
```

### Common UI Patterns

**Loading state:**
```vue
<v-progress-circular v-if="loading" indeterminate color="primary" />
<DataDisplay v-else :data="data" />
```

**Form handling:**
```vue
<v-form @submit.prevent="submit">
  <v-text-field v-model="form.name" label="Name" />
  <v-btn type="submit" color="primary">Submit</v-btn>
</v-form>
```

**Responsive grid:**
```vue
<v-row>
  <v-col cols="12" md="8"><MainContent /></v-col>
  <v-col cols="12" md="4"><Sidebar /></v-col>
</v-row>
```

---

## Design System (ArchitectUI-inspired)

Use `/ui-scaffold` skill to get full code templates for any UI pattern (page, card, table, search, dialog, etc.).

Copilot reference: `.github/instructions/design-system.instructions.md`

### Theme

- Primary `#1976D2`, Background `#f5f5f5`, Surface `#FFFFFF`
- Theme name: `architectLight` in `src/plugins/vuetify.ts`
- Never hardcode colors — use theme tokens

### Key UI Rules (always enforce)

1. **Every page**: `<v-container fluid class="pa-6">` → page header (`text-h5` + `text-body-2`) → content
2. **Every card**: `<v-card rounded="lg">` with `.card-header-separated` header (icon + bold title + bottom border)
3. **No elevation props** on cards — global `$shadow-architect` in `settings.scss` handles shadows
4. **Chips for types**: `variant="tonal"`, `size="x-small"`, color-coded (Cluster→primary, Standalone→teal, etc.)
5. **Tables**: `<v-table density="compact" hover>` with fixed-width columns for Type/Actions
6. **Loading**: skeleton loaders for first load, `v-progress-linear` for refreshes, `:loading` on buttons
7. **Empty state**: centered icon (48px) + `text-subtitle-1` title + `text-body-2` description
8. **Error state**: `v-alert type="error" variant="tonal"` with Retry button
9. **Dialogs**: `v-dialog max-width="440"` with card-header-separated pattern inside
10. **Search bars**: `variant="solo-filled"` with embedded button via `#append-inner` slot, or `variant="outlined"` for page-level filters

### Styling

- Icons: Material Design Icons with `mdi-` prefix
- Prefer Vuetify utility classes (`mb-4`, `pa-2`, `d-flex`, `text-medium-emphasis`) over custom CSS
- CSS classes in `src/styles/settings.scss`: `.card-header-separated`, `.card-header-title`, `.sidebar-section-header`, `.sidebar-brand`

---

## Constraints

**Required:**
- `<script setup lang="ts">` for all components
- Vuetify 3 components exclusively for UI
- Pinia Options API syntax (not Setup Stores)
- File-based routing via `src/pages/`
- TypeScript strict mode, all code fully typed
- Use inline type assertions: `const items = [] as Item[]`

**Prohibited:**
- Options API (`export default {}`)
- Other UI libraries (Bootstrap, Ant Design, etc.)
- Manual route registration in router config
- Vuex or other state libraries
- JSX/TSX syntax
- Modifying auto-generated `.d.ts` files
- Manual imports for auto-imported APIs
- Hardcoded colors (use theme variables)

---

## Creating a New Feature — Checklist

1. **Determine scope**: Does it need a component, store, page, API client, or a combination?
2. **API client** (if backend calls needed): Create in `src/api/`, follow `redisServers.ts` pattern
3. **Store** (if shared state needed): Create in `src/stores/`, Options API, `useXxxStore` naming
4. **Component**: Create in `src/components/`, PascalCase, `<script setup lang="ts">`
5. **Page** (if new route needed): Create in `src/pages/`, keep minimal — delegate to components
6. **Verify**: Run `npm run build` to check compilation, test in browser at `http://localhost:3000`

---

## Commands

```bash
npm run dev          # Dev server on port 3000
npm run build        # Production build
npm run type-check   # TypeScript checks
npm run lint         # ESLint with auto-fix
```

## Testing

- Unit tests: `tests/unit/`
- Component tests: `tests/component/`
- Do NOT use `src/__tests__/`
- Import source modules from `src/` using relative paths

## MCP Servers

### Playwright MCP (use proactively when available)

Use the Playwright MCP server to verify your work visually after making UI changes. Specifically:

- **After layout/styling changes**: Take a screenshot to confirm the result matches expectations before reporting done
- **Measuring and comparing**: Use `browser_evaluate` to measure element dimensions, computed styles, or DOM state when debugging visual issues
- **Verifying new views**: Navigate to the page, interact with it (click tabs, fill forms, trigger actions), and screenshot the result
- **Checking responsive behavior**: Resize the browser and screenshot at different viewport sizes
- **Debugging runtime issues**: Inspect console errors, network requests, and accessibility snapshots
- **E2E flows**: Test multi-step interactions like auth redirects, form submissions, and navigation

Save all Playwright screenshots into the `.playwright-mcp/` directory (e.g. `.playwright-mcp/dashboard.png`).

Do NOT use Playwright for fast unit tests or purely backend/API checks — use Vitest + @vue/test-utils for those.

### Vuetify MCP

Use when working with Vuetify components to verify correct API usage:

- `get_component_api_by_version` — props, events, slots for any component
- `get_directive_api_by_version` — directive API (v-ripple, etc.)
- `get_feature_guide` — feature docs (accessibility, theming, etc.)

## Code Intelligence

- When LSP is available, prefer using it (hover, goToDefinition, findReferences, documentSymbol, goToImplementation, etc.) to get type information instead of searching/reading source files manually.
