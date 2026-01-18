# Vue Pages Style Guide

## Overview

Vue pages are route components that live in `src/pages/` and are automatically registered as routes through the file-based routing system.

## Page Structure

### Standard Template

```vue
<template>
  <ComponentName />
</template>

<script setup lang="ts">
// Page logic
</script>

<route lang="yaml">
meta:
  layout: default
</route>
```

## Unique Conventions

### 1. File-Based Routing

Page files automatically become routes:

```
src/pages/
├── index.vue           → /
├── about.vue           → /about
├── users/
│   ├── index.vue       → /users
│   └── [id].vue        → /users/:id
└── settings.vue        → /settings
```

**Key points**:

- Filename determines route path
- `index.vue` represents the base path
- `[param].vue` creates dynamic routes

### 2. Minimal Page Components

Pages in this project are extremely minimal, delegating to components:

```vue
<!-- ✅ Correct - from src/pages/index.vue -->
<template>
  <HelloWorld />
</template>

<script setup lang="ts">
//
</script>
```

**Pattern**: Pages are thin wrappers that compose components rather than containing complex logic.

### 3. No Explicit Imports for Components

Components are auto-imported:

```vue
<script setup lang="ts">
// ✅ Correct - no import needed
// HelloWorld component is automatically available
</script>

<template>
  <HelloWorld />
</template>

<!-- ❌ Incorrect -->
<!-- <script setup lang="ts">
import HelloWorld from '@/components/HelloWorld.vue'
</script> -->
```

### 4. Layout Integration

Pages are automatically wrapped by layouts. Can specify layout in route meta:

```vue
<template>
  <div>Page content</div>
</template>

<route lang="yaml">
meta:
  layout: default # Uses src/layouts/default.vue
</route>
```

**Default behavior**: If no layout specified, uses `default.vue` automatically.

### 5. Route Meta Configuration

Use `<route>` block for route-level configuration:

```vue
<route lang="yaml">
meta:
  layout: admin
  requiresAuth: true
  title: "Dashboard"
</route>
```

This is unique to unplugin-vue-router and allows defining route metadata directly in the page.

### 6. Page Composition Pattern

Pages compose multiple components:

```vue
<template>
  <v-container>
    <PageHeader />
    <MainContent />
    <PageFooter />
  </v-container>
</template>

<script setup lang="ts">
// Component logic
</script>
```

### 7. Auto-Imported Composables

Router composables available without imports:

```vue
<script setup lang="ts">
// ✅ Auto-imported
const router = useRouter();
const route = useRoute();

// Access route params
const userId = route.params.id;

// Navigate
const goBack = () => router.back();
</script>
```

### 8. Store Access

Pinia stores auto-imported:

```vue
<script setup lang="ts">
// ✅ Auto-imported
const appStore = useAppStore();
const { loading } = storeToRefs(appStore);
</script>
```

## Page Examples

### Simple Page (index.vue)

```vue
<template>
  <HelloWorld />
</template>

<script setup lang="ts">
//
</script>
```

**Pattern**: Minimal page that renders a single component.

### Page with Data Fetching

```vue
<template>
  <v-container v-if="!loading">
    <UserProfile :user="user" />
  </v-container>
</template>

<script setup lang="ts">
const route = useRoute();
const userId = route.params.id;

const loading = ref(true);
const user = ref(null);

onMounted(async () => {
  user.value = await fetchUser(userId);
  loading.value = false;
});
</script>
```

### Page with Multiple Components

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

<script setup lang="ts">
//
</script>
```

### Page with Custom Layout

```vue
<template>
  <AdminPanel />
</template>

<script setup lang="ts">
//
</script>

<route lang="yaml">
meta:
  layout: admin
  requiresAuth: true
</route>
```

## Dynamic Routes

### Parameter-Based Routes

```vue
<!-- src/pages/users/[id].vue → /users/:id -->
<template>
  <UserDetail :userId="route.params.id" />
</template>

<script setup lang="ts">
const route = useRoute();
</script>
```

### Catch-All Routes

```vue
<!-- src/pages/[...all].vue → /* (catch-all) -->
<template>
  <NotFound :path="route.params.all" />
</template>
```

## Navigation in Pages

### Programmatic Navigation

```vue
<script setup lang="ts">
const router = useRouter();

const navigateToUser = (id: string) => {
  router.push(`/users/${id}`);
};

const goBack = () => {
  router.back();
};
</script>

<template>
  <v-btn @click="goBack">Back</v-btn>
  <v-btn @click="navigateToUser('123')">View User</v-btn>
</template>
```

### Declarative Navigation

```vue
<template>
  <!-- Using Vuetify button with 'to' prop -->
  <v-btn to="/">Home</v-btn>
  <v-btn :to="{ name: 'user-detail', params: { id: '123' } }">
    User Detail
  </v-btn>
</template>
```

## What Makes Pages Unique in This Project

1. **Extreme Simplicity**: Pages are intentionally minimal, often just rendering a component
2. **File-Based Routes**: No manual route registration - filename is the route
3. **Auto-Wrapped by Layouts**: Pages are automatically wrapped by layout system
4. **Route Meta in File**: Route configuration lives in the page file with `<route>` block
5. **No Imports**: Components, composables, and stores are auto-imported
6. **Composition Over Implementation**: Pages compose components rather than implementing logic

## Page Creation Checklist

- [ ] Create `.vue` file in `src/pages/` directory
- [ ] Use descriptive filename (becomes route path)
- [ ] Keep page minimal - delegate to components
- [ ] Use `<script setup lang="ts">` if logic needed
- [ ] No manual imports for components/composables
- [ ] Add `<route>` block if custom meta needed
- [ ] Use dynamic params `[param].vue` for dynamic routes
- [ ] Test auto-generated route works correctly

## Route Meta Examples

### Auth Protection

```vue
<route lang="yaml">
meta:
  requiresAuth: true
</route>
```

### Custom Layout

```vue
<route lang="yaml">
meta:
  layout: admin
</route>
```

### Page Title

```vue
<route lang="yaml">
meta:
  title: "User Dashboard"
</route>
```

### Multiple Properties

```vue
<route lang="yaml">
meta:
  layout: admin
  requiresAuth: true
  title: "Admin Panel"
  roles: ["admin", "moderator"]
</route>
```

## Common Patterns

### Loading State

```vue
<template>
  <v-container>
    <v-progress-circular v-if="loading" indeterminate />
    <DataDisplay v-else :data="data" />
  </v-container>
</template>

<script setup lang="ts">
const loading = ref(true);
const data = ref(null);

onMounted(async () => {
  data.value = await fetchData();
  loading.value = false;
});
</script>
```

### Error Handling

```vue
<template>
  <v-container>
    <v-alert v-if="error" type="error">{{ error }}</v-alert>
    <Content v-else />
  </v-container>
</template>

<script setup lang="ts">
const error = ref(null);

onMounted(async () => {
  try {
    await loadData();
  } catch (err) {
    error.value = err.message;
  }
});
</script>
```

### Responsive Layout

```vue
<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <MainContent />
      </v-col>
      <v-col cols="12" md="4" class="d-none d-md-block">
        <Sidebar />
      </v-col>
    </v-row>
  </v-container>
</template>
```

onMounted(async () => {
try {
await loadData();
} catch (err) {
error.value = err.message;
}
});
</script>

````

### Responsive Layout

```vue
<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <MainContent />
      </v-col>
      <v-col cols="12" md="4" class="d-none d-md-block">
        <Sidebar />
      </v-col>
    </v-row>
  </v-container>
</template>
````
