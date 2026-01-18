# UI Domain Analysis

## Overview

The UI domain encompasses all visual components and user interface elements in the Roman Redis Manager application. The UI is built entirely on Vue 3 with Vuetify 3, following Material Design principles.

## Core Technologies

- **Vue 3** with Composition API (`<script setup>` syntax)
- **Vuetify 3** Material Design component library
- **TypeScript** for type safety
- **Material Design Icons** (@mdi/font)

## Component Structure Pattern

### Standard Component Structure

All Vue components in this project follow this structure:

```vue
<template>
  <v-container class="fill-height d-flex align-center" max-width="900">
    <!-- Vuetify components here -->
  </v-container>
</template>

<script setup lang="ts">
// Composables and reactive state
// All Vue APIs and Vuetify components are auto-imported
</script>

<style scoped lang="sass">
// Component-specific styles (optional)
</style>
```

### Example from HelloWorld.vue

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
        <v-col cols="12">
          <v-card
            class="py-4"
            color="surface-variant"
            prepend-icon="mdi-rocket-launch-outline"
            rounded="lg"
            variant="tonal"
          >
            <template #title>
              <h2 class="text-h5 font-weight-bold">Get started</h2>
            </template>
          </v-card>
        </v-col>

        <v-col v-for="link in links" :key="link.href" cols="6">
          <v-card
            append-icon="mdi-open-in-new"
            class="py-4"
            color="surface-variant"
            :href="link.href"
            :prepend-icon="link.icon"
            rel="noopener noreferrer"
            rounded="lg"
            :subtitle="link.subtitle"
            target="_blank"
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
    subtitle: "Learn about all things Vuetify in our documentation.",
    title: "Documentation",
  },
  // ... more links
];
</script>
```

## Key Conventions

### 1. Composition API with `<script setup>`

- All components use `<script setup lang="ts">` syntax
- No Options API (`export default { ... }`)
- Reactive state defined with `ref()`, `reactive()`, `computed()`
- Auto-imported Vue APIs (no need to import ref, computed, etc.)

### 2. Vuetify Components

All UI elements use Vuetify components:

- **Layout**: `v-app`, `v-main`, `v-container`, `v-row`, `v-col`
- **Display**: `v-card`, `v-img`, `v-footer`
- **Typography**: Use Vuetify typography classes (`text-h2`, `text-body-2`, `font-weight-bold`)
- **Spacing**: Vuetify utility classes (`mb-4`, `py-4`, `fill-height`)

### 3. Material Design Icons

Icons use the `mdi-` prefix:

```vue
<v-card prepend-icon="mdi-rocket-launch-outline">
<v-icon icon="mdi-github" />
```

### 4. Component Auto-Import

Components in `src/components/` are automatically available:

- No import statements needed for custom components
- `HelloWorld` component used in pages without import
- Configured via `unplugin-vue-components`

### 5. Responsive Design

Uses Vuetify's 12-column grid system:

```vue
<v-row>
  <v-col cols="12">        <!-- Full width -->
  <v-col cols="6">         <!-- Half width -->
  <v-col cols="12" md="6"> <!-- Responsive: full on mobile, half on desktop -->
</v-row>
```

## Component Categories

### Layout Components

Located in `src/components/`:

- **AppFooter.vue** - Application footer with social links and copyright

Example from AppFooter.vue:

```vue
<template>
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
      <v-icon :icon="item.icon" :size="item.icon === '$vuetify' ? 24 : 16" />
    </a>

    <div
      class="text-caption text-disabled"
      style="position: absolute; right: 16px;"
    >
      &copy; 2016-{{ new Date().getFullYear() }}
    </div>
  </v-footer>
</template>

<script setup lang="ts">
const items = [
  {
    title: "Vuetify Documentation",
    icon: `$vuetify`,
    href: "https://vuetifyjs.com/",
  },
  // ... more items
];
</script>
```

### Page Components

Components displaying main content, used in pages:

- **HelloWorld.vue** - Welcome/home page component

## Styling Patterns

### 1. Vuetify Utility Classes

Preferred over custom CSS:

```vue
<!-- Spacing -->
<div class="mb-4 py-4 px-2">

<!-- Typography -->
<h1 class="text-h2 font-weight-bold">

<!-- Layout -->
<div class="fill-height d-flex align-center">

<!-- Colors -->
<v-card color="surface-variant">
```

### 2. Scoped Styles with SCSS

When custom styles are needed:

```vue
<style scoped lang="sass">
.social-link :deep(.v-icon)
  color: rgba(var(--v-theme-on-background), var(--v-disabled-opacity))
  text-decoration: none
  transition: .2s ease-in-out

  &:hover
    color: rgba(25, 118, 210, 1)
</style>
```

### 3. Theme Variables

Use Vuetify theme variables for consistency:

```scss
color: rgba(var(--v-theme-on-background), var(--v-disabled-opacity));
```

## Asset Management

### Images

- Located in `src/assets/`
- Referenced with `@` alias: `src="@/assets/logo.png"`
- Supports PNG and SVG formats

### Icons

- Material Design Icons via `@mdi/font`
- Custom icons can use SVG paths in icon prop
- Vuetify icon (`$vuetify`) available

## Template Patterns

### v-for with :key

Always use `:key` with `v-for`:

```vue
<v-col v-for="link in links" :key="link.href" cols="6">
```

### Dynamic Binding

Use `:` prefix for dynamic props:

```vue
<v-card :href="link.href" :title="link.title" :subtitle="link.subtitle" />
```

### Template Slots

Vuetify components support named slots:

```vue
<v-card>
  <template #title>
    <h2 class="text-h5 font-weight-bold">
      Get started
    </h2>
  </template>
  
  <template #subtitle>
    <div class="text-subtitle-1">
      Content here
    </div>
  </template>
</v-card>
```

## Best Practices

1. **Always use Vuetify components** - Don't create custom buttons, cards, or form elements
2. **Leverage auto-imports** - No need to import Vue APIs or custom components
3. **Use typed props** - Define prop types with TypeScript when components accept props
4. **Follow Material Design** - Use Vuetify's built-in variants (tonal, outlined, text, etc.)
5. **Responsive by default** - Always consider mobile layout with Vuetify's breakpoint system
6. **Accessibility** - Use semantic HTML and proper ARIA attributes when needed

## Adding New UI Components

To add a new component:

1. Create `.vue` file in `src/components/`
2. Use `<script setup lang="ts">` syntax
3. Build UI with Vuetify components
4. Component automatically available in pages/layouts (no import needed)
5. Use Material Design Icons for any icons
6. Apply Vuetify utility classes for spacing/styling
7. Add scoped styles only if custom styling is absolutely necessary

Example template for new component:

````vue
<template>
  <v-card>
    <v-card-title>My Component</v-card-title>
    <v-card-text>
      <!-- Component content -->
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// Component logic
</script>

<style scoped lang="sass">
// Custom styles if needed
</style>
```vue
<template>
  <v-card>
    <v-card-title>My Component</v-card-title>
    <v-card-text>
      <!-- Component content -->
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// Component logic
</script>

<style scoped lang="sass">
// Custom styles if needed
</style>
````
