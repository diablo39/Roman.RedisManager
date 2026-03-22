# Copilot Instructions for Roman Redis Manager Frontend

## Overview

This file provides AI coding assistants with comprehensive instructions for generating features in the Roman Redis Manager frontend application. All conventions and patterns described here are based on actual, observed patterns from the existing codebase—not invented best practices.

**Purpose**: Enable AI assistants to generate code that follows established project conventions, architectural patterns, and coding standards.

**Tech Stack**: Vue 3 + Vuetify 3 + TypeScript + Vite + Pinia + File-based Routing

---

## Vuetify MCP Server

This project has access to the **Vuetify Model Context Protocol (MCP) server** which provides real-time access to Vuetify documentation and API information. When working with Vuetify components, directives, or features:

**Available MCP Tools**:

- `get_component_api_by_version` - Get complete API documentation for any Vuetify component
- `get_directive_api_by_version` - Get API information for Vuetify directives (v-ripple, etc.)
- `get_feature_guide` - Access feature documentation (accessibility, theming, etc.)
- `get_vuetify_api_by_version` - Download and cache Vuetify API types by version

**When to Use MCP Server**:

- When unsure about component props, events, or slots
- When implementing complex Vuetify features
- To verify correct API usage for components
- To explore available Vuetify component options

**Example Usage**:

```
Need v-data-table props? → Use get_component_api_by_version('v-data-table')
Need theming guide? → Use get_feature_guide('theme')
```

---

## Playwright MCP Server

This project may use a **Playwright Model Context Protocol (MCP) server** for browser automation, end-to-end testing, and capturing runtime browser state. The Playwright MCP provides programmatic actions such as navigation, clicking, typing, file uploads, waiting for selectors or network activity, handling dialogs, and taking screenshots or accessibility snapshots.

When to Use Playwright MCP Server:

- Automating end-to-end UI flows (for example: sign-in redirect flows, OAuth/OIDC callback handling, complex multi-step interactions).
- Reproducing and debugging browser-only issues (cookies, localStorage/sessionStorage, CORS redirects, cross-origin flows).
- Capturing visual snapshots or accessibility snapshots for regression testing.
- Verifying integrations that require a real browser (third-party identity providers, redirects, popup flows, file uploads).
- Collecting page console logs and network traces to diagnose flaky UI behavior.

When NOT to use Playwright MCP Server:

- For fast unit tests or isolated component tests — use Vitest + @vue/test-utils instead.
- For purely backend or API contract checks that don't require a browser context.

Example Usage:

```
Need to verify login redirect behavior across providers? → Use Playwright to navigate to `/login`, click the provider button, follow the external redirect, and capture the final page snapshot and console logs.
```

## Project Context

### Application Domain

Roman Redis Manager is a **frontend UI for managing Redis databases**. It provides:

- Redis connection management
- Data browsing and manipulation (keys, values, data structures)
- Database administration interface
- Potential monitoring and configuration management

This is a **frontend-only** application that communicates with a separate backend API (not in this workspace).

### Core Technologies

- **Vue 3** (v3.5.21) with Composition API
- **Vuetify 3** (v3.10.1) - Material Design components
- **TypeScript** (v5.9.2) - Type-safe development
- **Vite** (v7.1.5) - Build tool and dev server
- **Pinia** (v3.0.3) - State management
- **Vue Router** (v4.5.1) + unplugin-vue-router - File-based routing

---

## File Categories Reference

### Vue Components

**Location**: `src/components/`  
**Examples**: `AppFooter.vue`, `HelloWorld.vue`

**Key Conventions**:

- Use `<script setup lang="ts">` syntax exclusively
- Build UI with Vuetify components only (v-container, v-card, v-btn, etc.)
- No manual imports for Vue APIs or custom components (auto-imported)
- Material Design Icons with `mdi-` prefix
- Vuetify utility classes for spacing/layout (mb-4, pa-2, d-flex, etc.)
- Scoped SCSS styles only when necessary (prefer utility classes)

**Template**:

```vue
<template>
  <v-container class="fill-height d-flex align-center">
    <v-card rounded="lg" variant="tonal">
      <v-card-title>Title</v-card-title>
      <v-card-text>Content</v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  // Vue APIs auto-imported (ref, computed, watch, etc.)
  const count = ref(0)
</script>

<style scoped lang="sass">
  // Custom styles only if needed
</style>
```

### Vue Pages

**Location**: `src/pages/`  
**Examples**: `index.vue`

**Key Conventions**:

- Filename determines route path (index.vue → /, about.vue → /about)
- Keep pages minimal—delegate to components
- Use `[param].vue` for dynamic routes
- Auto-imported components available without imports
- Specify layout with `<route lang="yaml">` block if needed

**Template**:

```vue
<template>
  <ComponentName />
</template>

<script setup lang="ts">
  // Minimal page logic
  const route = useRoute()
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Page Title'
</route>
```

### Vue Layouts

**Location**: `src/layouts/`  
**Examples**: `default.vue`

**Key Conventions**:

- Contain `<v-main><router-view /></v-main>` structure
- Do NOT include `<v-app>` (only in App.vue)
- Use `app` prop on v-app-bar, v-navigation-drawer, v-footer
- Auto-imported shared components available
- Local state for UI interactions (drawer open/close)

**Template**:

```vue
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>

<script setup lang="ts">
  // Minimal or no logic
</script>
```

### Pinia Stores

**Location**: `src/stores/`  
**Examples**: `app.ts`

**Key Conventions**:

- Use Options API syntax (not Setup Stores)
- `defineStore` and `storeToRefs` are auto-imported
- Naming: `useXxxStore` exported, `'xxx'` store ID
- TypeScript types inline with `as` assertions
- State as factory function: `state: () => ({})`

**Template**:

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

### Router Configuration

**Location**: `src/router/`  
**Examples**: `index.ts`

**Key Conventions**:

- Routes auto-generated from `src/pages/` files
- Import from virtual modules: `vue-router/auto-routes`
- Routes wrapped with layouts via `setupLayouts()`
- Dynamic import error handler for Vite chunk loading issues
- Keep configuration minimal—don't manually define routes

### Plugin Configuration

**Location**: `src/plugins/`  
**Examples**: `index.ts`, `vuetify.ts`

**Key Conventions**:

- Single `registerPlugins()` function for all plugins
- Order: Vuetify → Router → Pinia
- Vuetify config minimal (system theme default)
- Import MDI icons and Vuetify styles in vuetify.ts

---

## Feature Scaffold Guide

### Creating a New Feature

**Step 1: Determine File Structure**

For a new feature like "Connection Manager":

1. **Component**: `src/components/ConnectionManager.vue`
2. **Page** (if needed): `src/pages/connections.vue`
3. **Store** (if state needed): `src/stores/connections.ts`
4. **Types** (if needed): Define inline or in separate file

**Step 2: Create Component**

```vue
<!-- src/components/ConnectionManager.vue -->
<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-icon icon="mdi-database" class="mr-2" />
      Connection Manager
    </v-card-title>

    <v-card-text>
      <v-list>
        <v-list-item
          v-for="conn in connections"
          :key="conn.id"
          :title="conn.name"
          :subtitle="`${conn.host}:${conn.port}`"
          @click="selectConnection(conn.id)"
        >
          <template #prepend>
            <v-icon :color="conn.active ? 'success' : 'grey'" icon="mdi-circle" />
          </template>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
  const connectionsStore = useConnectionsStore()
  const { connections } = storeToRefs(connectionsStore)

  const selectConnection = (id: string) => {
    connectionsStore.setActive(id)
  }

  onMounted(() => {
    connectionsStore.fetchConnections()
  })
</script>
```

**Step 3: Create Store (if needed)**

```typescript
// src/stores/connections.ts
interface Connection {
  id: string
  name: string
  host: string
  port: number
  active: boolean
}

export const useConnectionsStore = defineStore('connections', {
  state: () => ({
    connections: [] as Connection[],
    activeId: null as string | null,
    loading: false,
  }),

  getters: {
    activeConnection(): Connection | null {
      return this.connections.find(c => c.id === this.activeId) || null
    },
  },

  actions: {
    async fetchConnections() {
      this.loading = true
      try {
        // API call
        this.connections = await api.getConnections()
      } finally {
        this.loading = false
      }
    },

    setActive(id: string) {
      this.activeId = id
    },
  },
})
```

**Step 4: Create Page (if needed)**

```vue
<!-- src/pages/connections.vue -->
<template>
  <v-container>
    <ConnectionManager />
  </v-container>
</template>

<script setup lang="ts">
  // Minimal - component does the work
</script>
```

### File Placement Rules

- **Reusable UI components** → `src/components/`
- **Page-level components** → `src/pages/` (becomes route)
- **Layout templates** → `src/layouts/`
- **Global state** → `src/stores/`
- **Types/interfaces** → Inline in files or separate types file
- **Static assets** → `src/assets/` or `public/`
- **Styles** → Component-scoped or `src/styles/settings.scss`

### Naming Conventions

- **Components**: PascalCase (`ConnectionManager.vue`)
- **Pages**: kebab-case or index (`connections.vue`, `users/[id].vue`)
- **Stores**: camelCase file, PascalCase export (`connections.ts` → `useConnectionsStore`)
- **Routes**: Derived from page filename

---

## Integration Rules

### UI Domain Constraints

**REQUIRED**:

- All components use `<script setup lang="ts">`
- UI built exclusively with Vuetify 3 components
- Components in `src/components/` auto-imported
- Icons use Material Design Icons with `mdi-` prefix

**PROHIBITED**:

- Options API (`export default {}`)
- Other UI libraries (Bootstrap, Ant Design, etc.)
- Manual imports for Vue APIs or custom components
- JSX/TSX syntax

### Routing Domain Constraints

**REQUIRED**:

- Routes created as `.vue` files in `src/pages/`
- File structure determines routes
- Dynamic routes use `[param].vue` syntax

**PROHIBITED**:

- Manual route registration in router config
- Routes defined outside `src/pages/`

### State Management Constraints

**REQUIRED**:

- All global state in Pinia stores
- Options API syntax (`state`, `getters`, `actions`)
- Stores located in `src/stores/`
- `defineStore` and `storeToRefs` auto-imported

**PROHIBITED**:

- Vuex or other state libraries
- Setup Stores syntax
- Manual Pinia imports

### Design System Constraints

**REQUIRED**:

- Vuetify theming system
- Material Design principles
- Vuetify utility classes for spacing/layout
- Theme customization via `src/plugins/vuetify.ts`

**PROHIBITED**:

- Custom UI libraries
- Hardcoded colors (use theme variables)
- Extensive custom CSS (prefer utility classes)

### Build System Constraints

**REQUIRED**:

- Vite as build tool
- Auto-import system for Vue APIs, components
- TypeScript strict mode
- `@` alias for `src/` imports

**PROHIBITED**:

- Manual imports for auto-imported APIs
- Relative paths when `@` alias available
- Modifying auto-generated `.d.ts` files

---

## Example Prompt Usage

### Example 1: New Feature

**Prompt**:

> "Create a searchable database key browser that displays Redis keys in a list and allows filtering by pattern"

**Expected Output**:

**Files Created**:

1. `src/components/KeyBrowser.vue` - Main component
2. `src/stores/keys.ts` - Store for key data
3. `src/pages/keys.vue` - Page route

**Component**:

```vue
<!-- src/components/KeyBrowser.vue -->
<template>
  <v-card>
    <v-card-title>Key Browser</v-card-title>

    <v-card-text>
      <v-text-field
        v-model="searchPattern"
        label="Search pattern"
        prepend-inner-icon="mdi-magnify"
        clearable
      />

      <v-list>
        <v-list-item v-for="key in filteredKeys" :key="key" :title="key" @click="selectKey(key)">
          <template #append>
            <v-icon icon="mdi-chevron-right" />
          </template>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
  const keysStore = useKeysStore()
  const { keys } = storeToRefs(keysStore)

  const searchPattern = ref('')

  const filteredKeys = computed(() => {
    if (!searchPattern.value) return keys.value
    const pattern = searchPattern.value.toLowerCase()
    return keys.value.filter(k => k.toLowerCase().includes(pattern))
  })

  const selectKey = (key: string) => {
    keysStore.setSelectedKey(key)
  }

  onMounted(() => {
    keysStore.fetchKeys()
  })
</script>
```

**Store**:

```typescript
// src/stores/keys.ts
export const useKeysStore = defineStore('keys', {
  state: () => ({
    keys: [] as string[],
    selectedKey: null as string | null,
    loading: false,
  }),

  actions: {
    async fetchKeys() {
      this.loading = true
      try {
        this.keys = await api.getKeys()
      } finally {
        this.loading = false
      }
    },

    setSelectedKey(key: string) {
      this.selectedKey = key
    },
  },
})
```

**Page**:

```vue
<!-- src/pages/keys.vue -->
<template>
  <v-container>
    <KeyBrowser />
  </v-container>
</template>
```

### Example 2: Adding to Existing Feature

**Prompt**:

> "Add a button to AppFooter that toggles dark mode"

**Expected Output**:

```vue
<!-- src/components/AppFooter.vue -->
<template>
  <v-footer app height="40">
    <!-- Existing social links -->

    <v-btn icon size="small" @click="toggleTheme" class="ml-2">
      <v-icon :icon="isDark ? 'mdi-weather-sunny' : 'mdi-weather-night'" />
    </v-btn>

    <!-- Existing copyright -->
  </v-footer>
</template>

<script setup lang="ts">
  import { useTheme } from 'vuetify'

  const theme = useTheme()

  const isDark = computed(() => theme.global.current.value.dark)

  const toggleTheme = () => {
    theme.global.name.value = isDark.value ? 'light' : 'dark'
  }

  // Existing items array...
</script>
```

---

## Development Workflow

### Running the Application

```bash
npm run dev          # Start dev server on port 3000
npm run build        # Build for production
npm run type-check   # Run TypeScript checks
npm run lint         # Run ESLint with auto-fix
```

### Adding Dependencies

```bash
npm install package-name        # Add runtime dependency
npm install -D package-name     # Add dev dependency
```

### File Creation Checklist

When creating files:

- [ ] Use correct directory (`components/`, `pages/`, `stores/`, etc.)
- [ ] Follow naming conventions (PascalCase for components, etc.)
- [ ] Use TypeScript (`lang="ts"`)
- [ ] No manual imports for auto-imported APIs
- [ ] Vuetify components only for UI
- [ ] Material Design Icons for icons
- [ ] Utility classes for styling

---

## Common Patterns

### Loading State

```vue
<script setup lang="ts">
  const loading = ref(false)
  const data = ref(null)

  onMounted(async () => {
    loading.value = true
    try {
      data.value = await fetchData()
    } finally {
      loading.value = false
    }
  })
</script>

<template>
  <v-progress-circular v-if="loading" indeterminate />
  <DataDisplay v-else :data="data" />
</template>
```

### Form Handling

```vue
<script setup lang="ts">
  const form = reactive({
    name: '',
    email: '',
  })

  const submit = async () => {
    await api.submit(form)
  }
</script>

<template>
  <v-form @submit.prevent="submit">
    <v-text-field v-model="form.name" label="Name" />
    <v-text-field v-model="form.email" label="Email" type="email" />
    <v-btn type="submit" color="primary">Submit</v-btn>
  </v-form>
</template>
```

### Responsive Layout

```vue
<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <MainContent />
      </v-col>
      <v-col cols="12" md="4">
        <Sidebar />
      </v-col>
    </v-row>
  </v-container>
</template>
```

---

## Additional Notes

### Auto-Generated Files

Never edit these files manually:

- `src/auto-imports.d.ts`
- `src/components.d.ts`
- `src/typed-router.d.ts`

### Asset Imports

Use `@` alias for imports:

```vue
<v-img src="@/assets/logo.png" />
```

### TypeScript

All code should be fully typed. Use inline type assertions:

```typescript
const items = [] as Item[]
const user = null as User | null
```

### Vuetify Resources

- Component API: https://vuetifyjs.com/components/all
- Icons: https://pictogrammers.com/library/mdi/

---

## Testing

- **Test locations**: Put unit tests in `tests/unit` and component tests in `tests/component`.
- **Do not use** `src/__tests__/` — move any tests from there into the `tests/` tree.
- **Imports**: Tests located in `tests/` should import source modules from `src/` (use relative paths like `../../src/...` as appropriate).

**End of Instructions**

Use these instructions as your guide when generating code for this project. Always follow established patterns and conventions to maintain consistency across the codebase.
