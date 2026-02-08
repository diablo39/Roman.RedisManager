# Vue Layouts Style Guide

## Overview

Vue layouts provide consistent page structure across routes. They are automatically applied to pages via vite-plugin-vue-layouts-next.

## Layout Structure

### Standard Template

```vue
<template>
  <v-main>
    <router-view />
  </v-main>

  <SharedComponents />
</template>

<script setup lang="ts">
// Layout logic
</script>
```

## Unique Conventions

### 1. Minimal Layouts

Layouts in this project are intentionally simple:

```vue
<!-- ✅ Actual src/layouts/default.vue -->
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>

<script setup lang="ts">
//
</script>
```

**Key points**:

- `<v-main>` wraps the page content
- `<router-view />` renders the matched page
- Shared components (like `AppFooter`) included directly

### 2. No Root v-app

Layouts do NOT include `<v-app>` - that's in App.vue:

```vue
<!-- ❌ Don't do this in layouts -->
<template>
  <v-app>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<!-- ✅ Correct - no v-app -->
<template>
  <v-main>
    <router-view />
  </v-main>
</template>
```

The `<v-app>` wrapper exists only in `src/App.vue`.

### 3. Auto-Imported Components

Shared components are auto-imported:

```vue
<template>
  <v-main>
    <router-view />
  </v-main>

  <!-- ✅ AppFooter auto-imported from src/components/ -->
  <AppFooter />
</template>

<script setup lang="ts">
// ✅ No import needed

// ❌ Don't import manually
// import AppFooter from '@/components/AppFooter.vue'
</script>
```

### 4. Vuetify Layout Components

Layouts use Vuetify's layout system components:

- **v-main**: Main content area, adjusts for app bars and drawers
- **v-app-bar**: Top application bar
- **v-navigation-drawer**: Side navigation panel
- **v-footer**: Bottom footer

All with `app` prop for proper layout integration.

### 5. App Prop Pattern

Layout components need `app` prop:

```vue
<template>
  <!-- ✅ Use app prop -->
  <v-app-bar app>
    <v-toolbar-title>Title</v-toolbar-title>
  </v-app-bar>

  <v-navigation-drawer app>
    <!-- Navigation -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>

  <v-footer app>
    <!-- Footer content -->
  </v-footer>
</template>
```

The `app` prop makes them part of the application layout system.

### 6. Layout State Management

Layouts can have local state for UI interactions:

```vue
<script setup lang="ts">
// Local drawer state
const drawer = ref(true);

const toggleDrawer = () => {
  drawer.value = !drawer.value;
};
</script>

<template>
  <v-app-bar app>
    <v-app-bar-nav-icon @click="toggleDrawer" />
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" app>
    <!-- Navigation -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>
</template>
```

### 7. Shared State via Pinia

For persistent layout state across navigation:

```vue
<script setup lang="ts">
// ✅ Auto-imported store
const appStore = useAppStore();
const { sidebarOpen } = storeToRefs(appStore);
</script>

<template>
  <v-navigation-drawer v-model="sidebarOpen" app>
    <!-- Persists across routes -->
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>
</template>
```

## Layout Examples

### Default Layout (Actual)

```vue
<!-- src/layouts/default.vue -->
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>

<script setup lang="ts">
//
</script>
```

**Features**:

- Minimal structure
- Main content area
- Shared footer

### Layout with App Bar

```vue
<template>
  <v-app-bar app color="primary" dark>
    <v-toolbar-title>Redis Manager</v-toolbar-title>
  </v-app-bar>

  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>
```

### Layout with Navigation

```vue
<template>
  <v-app-bar app>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
    <v-toolbar-title>App Title</v-toolbar-title>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" app>
    <v-list>
      <v-list-item to="/" title="Home" />
      <v-list-item to="/about" title="About" />
    </v-list>
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

### Full Layout Structure

```vue
<template>
  <v-app-bar app color="primary" dark>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
    <v-toolbar-title>Redis Manager</v-toolbar-title>
    <v-spacer />
    <v-btn icon>
      <v-icon icon="mdi-cog" />
    </v-btn>
  </v-app-bar>

  <v-navigation-drawer v-model="drawer" app>
    <v-list>
      <v-list-item
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        :title="item.title"
        :prepend-icon="item.icon"
      />
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
const drawer = ref(true);

const navItems = [
  { title: "Home", to: "/", icon: "mdi-home" },
  { title: "Connections", to: "/connections", icon: "mdi-database" },
  { title: "Settings", to: "/settings", icon: "mdi-cog" },
];
</script>
```

## Responsive Layouts

### Mobile-Aware Navigation

```vue
<script setup lang="ts">
import { useDisplay } from "vuetify";

const { mobile, mdAndUp } = useDisplay();
const drawer = ref(true);
</script>

<template>
  <v-app-bar v-if="mobile" app>
    <v-app-bar-nav-icon @click="drawer = !drawer" />
    <v-toolbar-title>Title</v-toolbar-title>
  </v-app-bar>

  <v-navigation-drawer
    v-model="drawer"
    :permanent="mdAndUp"
    :temporary="mobile"
    app
  >
    <v-list>
      <!-- Navigation items -->
    </v-list>
  </v-navigation-drawer>

  <v-main>
    <router-view />
  </v-main>
</template>
```

## Layout Component Composition

### Extracted App Bar Component

```vue
<!-- src/components/AppBar.vue -->
<template>
  <v-app-bar app color="primary" dark>
    <v-app-bar-nav-icon @click="emit('toggle-drawer')" />
    <v-toolbar-title>{{ title }}</v-toolbar-title>
  </v-app-bar>
</template>

<script setup lang="ts">
interface Props {
  title: string;
}

defineProps<Props>();

const emit = defineEmits<{
  "toggle-drawer": [];
}>();
</script>
```

### Using in Layout

```vue
<template>
  <!-- ✅ Auto-imported component -->
  <AppBar title="Redis Manager" @toggle-drawer="drawer = !drawer" />

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

## What Makes Layouts Unique in This Project

1. **Minimal by Default**: Default layout is extremely simple (just v-main + footer)
2. **No v-app in Layouts**: Root v-app is only in App.vue
3. **Auto-Imported Components**: Shared components available without imports
4. **File-Based System**: Layouts automatically available from src/layouts/
5. **Vuetify Integration**: Uses Vuetify's app prop pattern for layout components
6. **Simple State**: Local ref() for drawer state, Pinia for persistent state

## Layout Creation Checklist

- [ ] Create `.vue` file in `src/layouts/` directory
- [ ] Use descriptive filename (referenced in page meta)
- [ ] Start with `<v-main><router-view /></v-main>` structure
- [ ] Do NOT include `<v-app>` wrapper
- [ ] Add `app` prop to v-app-bar, v-navigation-drawer, v-footer
- [ ] Use auto-imported components
- [ ] Keep logic minimal or use Pinia for shared state
- [ ] Consider responsive behavior with useDisplay()

## Common Layout Patterns

### Admin Layout

```vue
<template>
  <v-app-bar app color="primary" dark>
    <v-toolbar-title>Admin Panel</v-toolbar-title>
  </v-app-bar>

  <v-navigation-drawer app permanent>
    <v-list>
      <v-list-item
        to="/admin"
        title="Dashboard"
        prepend-icon="mdi-view-dashboard"
      />
      <v-list-item
        to="/admin/users"
        title="Users"
        prepend-icon="mdi-account-multiple"
      />
    </v-list>
  </v-navigation-drawer>

  <v-main>
    <v-container>
      <router-view />
    </v-container>
  </v-main>
</template>
```

### Auth Layout

```vue
<template>
  <v-main class="d-flex align-center justify-center fill-height">
    <v-card max-width="400" width="100%">
      <router-view />
    </v-card>
  </v-main>
</template>
```

### Simple Layout (Like Default)

```vue
<template>
  <v-main>
    <router-view />
  </v-main>

  <AppFooter />
</template>
```

## Layout Assignment

### Automatic (Default Layout)

Pages automatically use `default.vue`:

```vue
<!-- src/pages/index.vue -->
<template>
  <HelloWorld />
</template>
<!-- Uses default.vue automatically -->
```

### Explicit Layout

```vue
<!-- src/pages/admin.vue -->
<template>
  <AdminPanel />
</template>

<route lang="yaml">
meta:
  layout: admin
</route>
```

### No Layout

````vue
<template>
  <FullPageComponent />
</template>

<route lang="yaml">
meta:
  layout: false
</route>
```vue
<!-- src/pages/index.vue -->
<template>
  <HelloWorld />
</template>
<!-- Uses default.vue automatically -->
````

### Explicit Layout

```vue
<!-- src/pages/admin.vue -->
<template>
  <AdminPanel />
</template>

<route lang="yaml">
meta:
  layout: admin
</route>
```

### No Layout

```vue
<template>
  <FullPageComponent />
</template>

<route lang="yaml">
meta:
  layout: false
</route>
```
