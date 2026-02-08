# Vue Components Style Guide

## Overview

Vue components in this project use the Composition API with `<script setup>` syntax, Vuetify 3 for UI, and TypeScript for type safety.

## Component Structure

### Standard Template

```vue
<template>
  <v-container>
    <!-- Vuetify components -->
  </v-container>
</template>

<script setup lang="ts">
// Component logic with auto-imported APIs
</script>

<style scoped lang="sass">
// Optional scoped styles
</style>
```

## Unique Conventions

### 1. No Explicit Imports

Components leverage the auto-import system:

```vue
<script setup lang="ts">
// ✅ Correct - no imports needed
const count = ref(0);
const doubled = computed(() => count.value * 2);

// ❌ Incorrect - don't import
// import { ref, computed } from 'vue'
</script>
```

Auto-imported:

- All Vue APIs (ref, reactive, computed, watch, onMounted, etc.)
- Custom components from `src/components/`

### 2. Vuetify-First UI

All UI elements use Vuetify components exclusively:

```vue
<template>
  <!-- ✅ Use Vuetify components -->
  <v-container class="fill-height d-flex align-center" max-width="900">
    <v-row>
      <v-col cols="12">
        <v-card color="surface-variant" rounded="lg" variant="tonal">
          <v-card-title>Title</v-card-title>
          <v-card-text>Content</v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>

  <!-- ❌ Don't use plain HTML for layout -->
  <!-- <div class="container">
    <div class="card">
      <h2>Title</h2>
      <p>Content</p>
    </div>
  </div> -->
</template>
```

### 3. Utility-First Styling

Prefer Vuetify utility classes over custom CSS:

```vue
<template>
  <!-- ✅ Use Vuetify utilities -->
  <div class="mb-4 py-4 text-center fill-height d-flex align-center">

  <!-- ❌ Avoid custom CSS when utilities exist -->
  <!-- <div class="custom-spacing custom-layout"> -->
</template>

<style scoped lang="sass">
// Only add custom styles when absolutely necessary
// ❌ Don't replicate what utilities do
// .custom-spacing
//   margin-bottom: 16px
//   padding-top: 16px
</style>
```

### 4. Template Slots for Vuetify Components

Use named slots for component structure:

```vue
<template>
  <v-card>
    <template #title>
      <h2 class="text-h5 font-weight-bold">Get started</h2>
    </template>

    <template #subtitle>
      <div class="text-subtitle-1">Change this page by updating components</div>
    </template>
  </v-card>
</template>
```

### 5. Reactive Data with Composition API

```vue
<script setup lang="ts">
// Simple reactive values
const count = ref(0);
const name = ref("John");

// Reactive objects
const user = reactive({
  name: "John",
  age: 30,
});

// Computed properties
const doubled = computed(() => count.value * 2);

// Methods as regular functions
const increment = () => {
  count.value++;
};
</script>
```

### 6. TypeScript Prop Types

```vue
<script setup lang="ts">
// Define props with TypeScript
interface Props {
  title: string;
  count?: number;
  items: string[];
}

const props = withDefaults(defineProps<Props>(), {
  count: 0,
});

// Emit events
const emit = defineEmits<{
  update: [value: number];
  delete: [];
}>();
</script>
```

### 7. v-for with :key

Always use `:key` with `v-for`:

```vue
<template>
  <!-- ✅ Correct -->
  <v-col v-for="link in links" :key="link.href" cols="6">
    <v-card :href="link.href">{{ link.title }}</v-card>
  </v-col>

  <!-- ❌ Missing key -->
  <!-- <v-col v-for="link in links" cols="6"> -->
</template>
```

### 8. Material Design Icons

Icons use `mdi-` prefix:

```vue
<template>
  <v-icon icon="mdi-github" />
  <v-card prepend-icon="mdi-rocket-launch-outline">
  <v-btn append-icon="mdi-open-in-new">
</template>
```

### 9. Asset References

Use `@` alias for assets:

```vue
<template>
  <!-- ✅ Use @ alias -->
  <v-img src="@/assets/logo.png" />

  <!-- ❌ Don't use relative paths -->
  <!-- <v-img src="../../assets/logo.png" /> -->
</template>
```

### 10. Scoped Styles with SCSS

When custom styles are needed, use scoped SCSS:

```vue
<style scoped lang="sass">
.social-link :deep(.v-icon)
  color: rgba(var(--v-theme-on-background), var(--v-disabled-opacity))
  transition: .2s ease-in-out

  &:hover
    color: rgba(25, 118, 210, 1)
</style>
```

**Key points**:

- `scoped` attribute for component-specific styles
- `lang="sass"` for SCSS syntax
- `:deep()` for styling child components
- Use Vuetify CSS variables for theme-aware colors

## Component Examples from Codebase

### HelloWorld.vue Pattern

```vue
<template>
  <v-container class="fill-height d-flex align-center" max-width="900">
    <div>
      <v-img class="mb-4" height="150" src="@/assets/logo.png" />

      <div class="mb-8 text-center">
        <div class="text-body-2 font-weight-light mb-n1">Welcome to</div>
        <h1 class="text-h2 my-0 font-weight-bold">Vuetify</h1>
      </div>

      <v-row>
        <v-col v-for="link in links" :key="link.href" cols="6">
          <v-card
            append-icon="mdi-open-in-new"
            :href="link.href"
            :title="link.title"
            variant="tonal"
          />
        </v-col>
      </v-row>
    </div>
  </v-container>
</template>

<script setup lang="ts">
const links = [
  {
    href: "https://vuetifyjs.com/",
    icon: "mdi-text-box-outline",
    title: "Documentation",
  },
];
</script>
```

### AppFooter.vue Pattern

```vue
<template>
  <v-footer app height="40">
    <a
      v-for="item in items"
      :key="item.title"
      class="d-inline-block mx-2 social-link"
      :href="item.href"
      target="_blank"
    >
      <v-icon :icon="item.icon" />
    </a>
  </v-footer>
</template>

<script setup lang="ts">
const items = [
  {
    title: "GitHub",
    icon: "mdi-github",
    href: "https://github.com/...",
  },
];
</script>

<style scoped lang="sass">
.social-link :deep(.v-icon)
  color: rgba(var(--v-theme-on-background), var(--v-disabled-opacity))
</style>
```

## What Makes This Project Unique

1. **Complete Auto-Import**: No manual imports for Vue APIs or components
2. **Vuetify Exclusive**: 100% Vuetify components, no mixing with other UI libraries
3. **Utility-First**: Vuetify utility classes preferred over custom CSS
4. **Composition API Only**: No Options API usage
5. **TypeScript Throughout**: All components use TypeScript
6. **SCSS with Theme Variables**: Custom styles use Vuetify theme variables
7. **Material Design Strict**: Icons and design patterns follow Material Design

## Component Creation Checklist

- [ ] Use `<script setup lang="ts">` syntax
- [ ] Build UI with Vuetify components only
- [ ] Use Vuetify utility classes for spacing/layout
- [ ] No manual imports for Vue APIs
- [ ] Use `mdi-` icons
- [ ] Add `:key` to all `v-for` loops
- [ ] Use `@` alias for asset imports
- [ ] Add scoped SCSS styles only if necessary
- [ ] Define TypeScript types for props/emits
- [ ] File placed in `src/components/` for auto-import
- [ ] Use Vuetify utility classes for spacing/layout
- [ ] No manual imports for Vue APIs
- [ ] Use `mdi-` icons
- [ ] Add `:key` to all `v-for` loops
- [ ] Use `@` alias for asset imports
- [ ] Add scoped SCSS styles only if necessary
- [ ] Define TypeScript types for props/emits
- [ ] File placed in `src/components/` for auto-import
