# Design System Domain Analysis

## Overview

The design system is built entirely on Vuetify 3's Material Design implementation, providing a consistent visual language across the Roman Redis Manager application.

## Core Technologies

- **Vuetify 3** (v3.10.1) - Material Design component framework
- **Material Design Icons** (@mdi/font v7.4.47)
- **Roboto Font** (@fontsource/roboto v5.2.7)
- **SCSS** - For theme customization

## Theme Configuration

### Vuetify Plugin (src/plugins/vuetify.ts)

```typescript
/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";

// Composables
import { createVuetify } from "vuetify";

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    defaultTheme: "system",
  },
});
```

### Theme System

- **Default Theme**: `'system'` - Automatically switches between light/dark based on system preferences
- **Available Themes**: light, dark, system
- Theme customization via Vuetify's theme configuration

### SCSS Settings (src/styles/settings.scss)

```scss
/**
 * src/styles/settings.scss
 *
 * Configures SASS variables and Vuetify overwrites
 */

// https://vuetifyjs.com/features/sass-variables/`
// @use 'vuetify/settings' with (
//   $color-pack: false
// );
```

Currently minimal - ready for SCSS variable customization.

## Typography

### Font Family

**Roboto** - Material Design's standard typeface

- Loaded via `@fontsource/roboto`
- Multiple weights: 100, 300, 400, 500, 700, 900
- Styles: normal, italic

### Typography Classes

Vuetify provides semantic typography classes:

```vue
<!-- Headings -->
<h1 class="text-h1">Heading 1</h1>
<h2 class="text-h2">Heading 2</h2>
<h3 class="text-h3">Heading 3</h3>
<h4 class="text-h4">Heading 4</h4>
<h5 class="text-h5">Heading 5</h5>
<h6 class="text-h6">Heading 6</h6>

<!-- Body Text -->
<p class="text-body-1">Body 1</p>
<p class="text-body-2">Body 2</p>

<!-- Special -->
<div class="text-subtitle-1">Subtitle 1</div>
<div class="text-subtitle-2">Subtitle 2</div>
<div class="text-caption">Caption</div>
<div class="text-overline">Overline</div>
```

### Font Weight

```vue
<div class="font-weight-thin">Thin</div>
<div class="font-weight-light">Light</div>
<div class="font-weight-regular">Regular</div>
<div class="font-weight-medium">Medium</div>
<div class="font-weight-bold">Bold</div>
<div class="font-weight-black">Black</div>
```

### Example from HelloWorld.vue

```vue
<div class="text-body-2 font-weight-light mb-n1">Welcome to</div>
<h1 class="text-h2 my-0 font-weight-bold">Vuetify</h1>
```

## Color System

### Theme Colors

Vuetify provides semantic color names:

- `primary` - Primary brand color
- `secondary` - Secondary brand color
- `accent` - Accent color
- `error` - Error states
- `warning` - Warning states
- `info` - Informational states
- `success` - Success states

### Surface Colors

- `surface` - Background surface
- `surface-variant` - Variant surface (used in cards)
- `on-surface` - Text on surface
- `on-background` - Text on background

### Color Usage

```vue
<!-- Component colors -->
<v-btn color="primary">Primary Button</v-btn>
<v-card color="surface-variant">Card</v-card>

<!-- Text colors -->
<div class="text-primary">Primary text</div>
<div class="text-disabled">Disabled text</div>
```

### Example from HelloWorld.vue

```vue
<v-card
  class="py-4"
  color="surface-variant"
  prepend-icon="mdi-rocket-launch-outline"
  variant="tonal"
>
```

### Theme Variables in CSS

```scss
// From AppFooter.vue
.social-link :deep(.v-icon)
  color: rgba(var(--v-theme-on-background), var(--v-disabled-opacity))

  &:hover
    color: rgba(25, 118, 210, 1)
```

## Icons

### Material Design Icons

All icons use MDI (Material Design Icons):

```vue
<!-- Using icon prop -->
<v-icon icon="mdi-github" />
<v-icon icon="mdi-rocket-launch-outline" />

<!-- In component props -->
<v-card prepend-icon="mdi-rocket-launch-outline">
<v-card append-icon="mdi-open-in-new">
```

### Icon Sizes

```vue
<v-icon icon="mdi-github" size="16" />
<v-icon icon="mdi-github" size="24" />
<v-icon icon="mdi-github" size="32" />

<!-- Or size names -->
<v-icon icon="mdi-github" size="small" />
<v-icon icon="mdi-github" size="default" />
<v-icon icon="mdi-github" size="large" />
<v-icon icon="mdi-github" size="x-large" />
```

### Custom SVG Icons

From AppFooter.vue, custom paths are supported:

```typescript
{
  title: 'Vuetify X',
  icon: ['M2.04875 3.00002L9.77052...'],  // SVG path array
  href: 'https://x.com/vuetifyjs',
}
```

### Special Icons

```typescript
icon: `$vuetify`; // Vuetify logo icon
```

## Spacing System

### Spacing Scale

Vuetify uses a 4px base spacing unit:

- 0: 0px
- 1: 4px
- 2: 8px
- 3: 12px
- 4: 16px
- 5: 20px
- 6: 24px
- ...continues

### Margin Classes

```vue
<!-- All sides -->
<div class="ma-4">margin: 16px</div>

<!-- Horizontal/Vertical -->
<div class="mx-4">margin-left & right: 16px</div>
<div class="my-4">margin-top & bottom: 16px</div>

<!-- Individual sides -->
<div class="mt-4">margin-top: 16px</div>
<div class="mb-4">margin-bottom: 16px</div>
<div class="ml-4">margin-left: 16px</div>
<div class="mr-4">margin-right: 16px</div>

<!-- Negative margins -->
<div class="mb-n1">margin-bottom: -4px</div>
```

### Padding Classes

```vue
<!-- All sides -->
<div class="pa-4">padding: 16px</div>

<!-- Horizontal/Vertical -->
<div class="px-4">padding-left & right: 16px</div>
<div class="py-4">padding-top & bottom: 16px</div>

<!-- Individual sides -->
<div class="pt-4">padding-top: 16px</div>
<div class="pb-4">padding-bottom: 16px</div>
<div class="pl-4">padding-left: 16px</div>
<div class="pr-4">padding-right: 16px</div>
```

### Example from Components

```vue
<!-- HelloWorld.vue -->
<v-img class="mb-4" />
<div class="mb-8 text-center">
<v-card class="py-4">

<!-- AppFooter.vue -->
<a class="d-inline-block mx-2 social-link">
```

## Layout Utilities

### Display

```vue
<div class="d-flex">Flexbox</div>
<div class="d-inline-block">Inline block</div>
<div class="d-block">Block</div>
<div class="d-none">Hidden</div>

<!-- Responsive display -->
<div class="d-none d-sm-inline-block">Show on SM+</div>
```

### Flexbox

```vue
<div class="d-flex align-center">Vertically centered</div>
<div class="d-flex justify-center">Horizontally centered</div>
<div class="d-flex flex-column">Column direction</div>
```

### Sizing

```vue
<div class="fill-height">100% height</div>
<div class="fill-width">100% width</div>
```

### Text Alignment

```vue
<div class="text-center">Centered text</div>
<div class="text-left">Left-aligned</div>
<div class="text-right">Right-aligned</div>
```

## Component Variants

### Card Variants

```vue
<v-card variant="elevated">Elevated (default)</v-card>
<v-card variant="flat">No elevation</v-card>
<v-card variant="tonal">Tonal background</v-card>
<v-card variant="outlined">Outlined border</v-card>
<v-card variant="text">No background</v-card>
```

### Button Variants

```vue
<v-btn variant="elevated">Elevated</v-btn>
<v-btn variant="flat">Flat</v-btn>
<v-btn variant="tonal">Tonal</v-btn>
<v-btn variant="outlined">Outlined</v-btn>
<v-btn variant="text">Text</v-btn>
<v-btn variant="plain">Plain</v-btn>
```

### Elevation

```vue
<v-card elevation="0">No shadow</v-card>
<v-card elevation="2">Small shadow</v-card>
<v-card elevation="4">Medium shadow</v-card>
<v-card elevation="8">Large shadow</v-card>
```

### Rounding

```vue
<v-card rounded="0">No rounding</v-card>
<v-card rounded="sm">Small</v-card>
<v-card rounded="lg">Large (used in HelloWorld)</v-card>
<v-card rounded="xl">Extra large</v-card>
<v-card rounded="pill">Pill shape</v-card>
```

## Grid System

### 12-Column Grid

```vue
<v-container>
  <v-row>
    <v-col cols="12">Full width</v-col>
    <v-col cols="6">Half width</v-col>
    <v-col cols="4">One third</v-col>
    <v-col cols="3">One quarter</v-col>
  </v-row>
</v-container>
```

### Responsive Columns

```vue
<v-col
  cols="12"      <!-- Mobile: full width -->
  sm="6"         <!-- Small: half width -->
  md="4"         <!-- Medium: one third -->
  lg="3"         <!-- Large: one quarter -->
>
```

### Container Sizes

```vue
<v-container>Default container</v-container>
<v-container fluid>Full width</v-container>
<v-container max-width="900">Fixed max width</v-container>
```

## Customization Patterns

### SCSS Variable Overrides

In settings.scss:

```scss
@use "vuetify/settings" with (
  $color-pack: false,
  $body-font-family: "Roboto",
  $heading-font-family: "Roboto"
);
```

### Runtime Theme Customization

```typescript
// In vuetify.ts
export default createVuetify({
  theme: {
    defaultTheme: "light",
    themes: {
      light: {
        colors: {
          primary: "#1976D2",
          secondary: "#424242",
          accent: "#82B1FF",
        },
      },
      dark: {
        colors: {
          primary: "#2196F3",
          secondary: "#424242",
          accent: "#FF4081",
        },
      },
    },
  },
});
```

## Best Practices

1. **Use Vuetify components exclusively** - Don't mix with other UI libraries
2. **Leverage utility classes** - Use spacing/layout classes before custom CSS
3. **Follow Material Design** - Use appropriate variants and elevations
4. **Use semantic colors** - primary, secondary, error, etc. instead of hardcoded colors
5. **MDI icons only** - Maintain consistency with Material Design Icons
6. **Responsive by default** - Use breakpoint props and responsive utilities
7. **Theme-aware styles** - Use CSS variables for custom styles
8. **Scoped custom styles** - Only add custom CSS when Vuetify utilities insufficient

## Common Patterns from Codebase

### Card with Icon and Content

```vue
<v-card
  class="py-4"
  color="surface-variant"
  prepend-icon="mdi-rocket-launch-outline"
  rounded="lg"
  variant="tonal"
>
  <template #title>
    <h2 class="text-h5 font-weight-bold">
      Title
    </h2>
  </template>
  <template #subtitle>
    <div class="text-subtitle-1">
      Subtitle
    </div>
  </template>
</v-card>
```

### Icon Link

```vue
<a
  class="d-inline-block mx-2 social-link"
  :href="item.href"
  target="_blank"
  rel="noopener noreferrer"
>
  <v-icon :icon="item.icon" :size="24" />
</a>
```

### Centered Container

```vue
<v-container class="fill-height d-flex align-center" max-width="900">
  <!-- Content -->
</v-container>
```

### Card with Icon and Content

```vue
<v-card
  class="py-4"
  color="surface-variant"
  prepend-icon="mdi-rocket-launch-outline"
  rounded="lg"
  variant="tonal"
>
  <template #title>
    <h2 class="text-h5 font-weight-bold">
      Title
    </h2>
  </template>
  <template #subtitle>
    <div class="text-subtitle-1">
      Subtitle
    </div>
  </template>
</v-card>
```

### Icon Link

```vue
<a
  class="d-inline-block mx-2 social-link"
  :href="item.href"
  target="_blank"
  rel="noopener noreferrer"
>
  <v-icon :icon="item.icon" :size="24" />
</a>
```

### Centered Container

```vue
<v-container class="fill-height d-flex align-center" max-width="900">
  <!-- Content -->
</v-container>
```
