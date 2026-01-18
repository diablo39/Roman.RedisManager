# Layout System Domain Analysis

## Overview

The layout system uses vite-plugin-vue-layouts-next to provide a flexible layout structure for the Roman Redis Manager application.

## Core Technologies

- **vite-plugin-vue-layouts-next** (v1.0.0) - Layout plugin
- **Vue Router** - Route integration
- **Vuetify** - Layout components

## Layout Architecture

### Root Application (src/App.vue)

```vue
<template>
  <v-app>
    <router-view />
  </v-app>
</template>

<script lang="ts" setup>
//
</script>
```

- **v-app**: Vuetify root wrapper, required for all Vuetify applications
- **router-view**: Renders the matched layout/page

### Default Layout (src/layouts/default.vue)

```vue
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>

<script lang="ts" setup>
//
</script>
```

- **v-main**: Main content area wrapper
- **router-view**: Renders the matched page component
- **AppFooter**: Shared footer component

## Layout Structure

### File Organization

```
src/
├── App.vue              # Root app wrapper
├── layouts/
│   ├── default.vue      # Default layout
│   └── README.md
└── pages/
    └── index.vue        # Pages use layouts
```

### Component Hierarchy

```
App.vue (<v-app>)
  └── default.vue (<v-main>)
      └── index.vue (page content)
  └── AppFooter.vue
```

## Vuetify Layout Components

### v-app

Root component that provides:

- Theme context
- Layout structure
- Responsive breakpoints
- Default styling

Must wrap entire application.

### v-main

Main content area that:

- Provides proper spacing for app bars, navigation drawers, footers
- Adjusts content position based on surrounding components
- Ensures proper scrolling behavior

### v-footer

Footer component with:

- **app** prop: Makes it part of application layout
- **height** prop: Fixed height specification
- Positioning options

From AppFooter.vue:

```vue
<v-footer
  app
  height="40"
>
```

## Layout Patterns

### Default Layout Pattern

Used by most pages automatically:

```vue
<!-- src/layouts/default.vue -->
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>
```

**Features**:

- Main content area
- Shared footer
- No header/navigation (can be added)

### Full Layout Structure (Expandable)

Common Vuetify layout structure:

```vue
<template>
  <!-- App Bar -->
  <v-app-bar app>
    <v-toolbar-title>App Title</v-toolbar-title>
  </v-app-bar>

  <!-- Navigation Drawer -->
  <v-navigation-drawer app>
    <v-list>
      <v-list-item to="/">Home</v-list-item>
    </v-list>
  </v-navigation-drawer>

  <!-- Main Content -->
  <v-main>
    <router-view />
  </v-main>

  <!-- Footer -->
  <AppFooter />
</template>
```

## Layout Usage in Pages

### Automatic Layout Application

Pages in `src/pages/` automatically use `default.vue` layout:

```vue
<!-- src/pages/index.vue -->
<template>
  <HelloWorld />
</template>

<script lang="ts" setup>
//
</script>
```

This page is automatically wrapped by the default layout.

### Explicit Layout Specification

Can specify layout in page using route meta:

```vue
<!-- src/pages/custom.vue -->
<template>
  <div>Custom page</div>
</template>

<route lang="yaml">
meta:
  layout: custom
</route>
```

### No Layout

To disable layout:

```vue
<route lang="yaml">
meta:
  layout: false
</route>
```

## Router Integration

### Setup in Router (src/router/index.ts)

```typescript
import { setupLayouts } from "virtual:generated-layouts";
import { routes } from "vue-router/auto-routes";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: setupLayouts(routes), // Wraps routes with layouts
});
```

The `setupLayouts()` function automatically wraps routes with their specified layout.

## Shared Components in Layouts

### AppFooter Component

Used in default layout:

```vue
<v-footer app height="40">
  <a
    v-for="item in items"
    :key="item.title"
    class="d-inline-block mx-2 social-link"
    :href="item.href"
    rel="noopener noreferrer"
    target="_blank"
    :title="item.title"
  >
    <v-icon
      :icon="item.icon"
      :size="item.icon === '$vuetify' ? 24 : 16"
    />
  </a>

  <div
    class="text-caption text-disabled"
    style="position: absolute; right: 16px;"
  >
    &copy; 2016-{{ (new Date()).getFullYear() }} <span class="d-none d-sm-inline-block">Vuetify, LLC</span>
    —
    <a
      class="text-decoration-none on-surface"
      href="https://vuetifyjs.com/about/licensing/"
      rel="noopener noreferrer"
      target="_blank"
    >
      MIT License
    </a>
  </div>
</v-footer>
```

**Features**:

- Social media links with icons
- Copyright notice
- Responsive text (hides company name on small screens)
- External links with proper attributes

## Creating New Layouts

### Step 1: Create Layout File

```vue
<!-- src/layouts/admin.vue -->
<template>
  <v-app-bar app color="primary" dark>
    <v-toolbar-title>Admin Panel</v-toolbar-title>
  </v-app-bar>

  <v-navigation-drawer app>
    <v-list>
      <v-list-item to="/admin">Dashboard</v-list-item>
      <v-list-item to="/admin/users">Users</v-list-item>
    </v-list>
  </v-navigation-drawer>

  <v-main>
    <v-container>
      <router-view />
    </v-container>
  </v-main>

  <AppFooter />
</template>

<script setup lang="ts">
//
</script>
```

### Step 2: Use in Page

```vue
<!-- src/pages/admin.vue -->
<template>
  <div>Admin content</div>
</template>

<route lang="yaml">
meta:
  layout: admin
</route>
```

## Common Layout Patterns

### With App Bar

```vue
<template>
  <v-app-bar app>
    <v-app-bar-title>Title</v-app-bar-title>
  </v-app-bar>

  <v-main>
    <router-view />
  </v-main>
</template>
```

### With Navigation Drawer

```vue
<template>
  <v-navigation-drawer app permanent>
    <!-- Navigation content -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>
</template>
```

### With Both

```vue
<template>
  <v-app-bar app>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
    <v-app-bar-title>Title</v-app-bar-title>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" app>
    <!-- Navigation content -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>
</template>

<script setup lang="ts">
const drawer = ref(true);
</script>
```

## Layout State Management

### Local State

```vue
<script setup lang="ts">
// Local to layout
const drawer = ref(true);
const railMode = ref(false);
</script>
```

### Store State

```vue
<script setup lang="ts">
// Shared via Pinia
const appStore = useAppStore();
const { sidebarOpen } = storeToRefs(appStore);
</script>

<template>
  <v-navigation-drawer v-model="sidebarOpen" app></v-navigation-drawer>
</template>
```

## Responsive Layout

### Breakpoint-Aware

```vue
<script setup lang="ts">
import { useDisplay } from "vuetify";

const { mobile, mdAndUp } = useDisplay();
</script>

<template>
  <v-navigation-drawer
    :permanent="mdAndUp"
    :temporary="mobile"
    app
  ></v-navigation-drawer>
</template>
```

### Conditional Rendering

```vue
<template>
  <v-app-bar v-if="mobile" app>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
  </v-app-bar>

  <v-navigation-drawer
    v-model="drawer"
    :permanent="!mobile"
    app
  ></v-navigation-drawer>
</template>
```

## Best Practices

1. **Use v-app wrapper**: Always have `<v-app>` in App.vue
2. **Use v-main**: Wrap page content with `<v-main>` in layouts
3. **App prop**: Add `app` prop to v-app-bar, v-navigation-drawer, v-footer for proper layout integration
4. **Shared components**: Place reusable layout components in src/components/
5. **Layout state**: Use Pinia for shared layout state (drawer open/close, theme, etc.)
6. **Responsive**: Consider mobile layout with temporary drawers
7. **Default layout**: Provide sensible default layout for most pages
8. **Specific layouts**: Create specific layouts only when needed (admin, auth, etc.)

## Adding Layout Components

### App Bar Example

```vue
<!-- src/components/AppBar.vue -->
<template>
  <v-app-bar app color="primary" dark>
    <v-app-bar-nav-icon @click="toggleDrawer" />
    <v-toolbar-title>Redis Manager</v-toolbar-title>
    <v-spacer />
    <v-btn icon>
      <v-icon icon="mdi-cog" />
    </v-btn>
  </v-app-bar>
</template>

<script setup lang="ts">
const emit = defineEmits(["toggle-drawer"]);

const toggleDrawer = () => {
  emit("toggle-drawer");
};
</script>
```

### Using in Layout

```vue
<!-- src/layouts/default.vue -->
<template>
  <AppBar @toggle-drawer="drawer = !drawer" />

  <v-navigation-drawer v-model="drawer" app>
    <!-- Navigation -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>

<script setup lang="ts">
const drawer = ref(true);
</script>
```

## Layout Plugin Configuration

### Vite Config (vite.config.mts)

```typescript
import Layouts from "vite-plugin-vue-layouts-next";

export default defineConfig({
  plugins: [
    VueRouter({
      dts: "src/typed-router.d.ts",
    }),
    Layouts(), // No config needed, uses defaults
    // ... other plugins
  ],
});
```

Default behavior:

- Looks for layouts in `src/layouts/`
- Default layout is `default.vue`
- Integrates with unplugin-vue-router
  <v-navigation-drawer v-model="drawer" app>
  <!-- Navigation -->
  </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>

    <AppFooter />
  </template>

<script setup lang="ts">
const drawer = ref(true);
</script>

````

## Layout Plugin Configuration

### Vite Config (vite.config.mts)

```typescript
import Layouts from "vite-plugin-vue-layouts-next";

export default defineConfig({
  plugins: [
    VueRouter({
      dts: "src/typed-router.d.ts",
    }),
    Layouts(), // No config needed, uses defaults
    // ... other plugins
  ],
});
````

Default behavior:

- Looks for layouts in `src/layouts/`
- Default layout is `default.vue`
- Integrates with unplugin-vue-router
