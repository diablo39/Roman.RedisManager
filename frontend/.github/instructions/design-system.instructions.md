---
description: 'ArchitectUI-inspired design system — consistent page layout, card patterns, and UI conventions'
applyTo: '**/*.vue, **/*.scss'
---

# Design System Instructions

This application follows an ArchitectUI-inspired admin dashboard design. All new pages and components MUST follow these patterns for visual consistency.

## Theme

- **Primary**: `#1976D2` (blue)
- **Background**: `#f5f5f5` (light gray — content area)
- **Surface**: `#FFFFFF` (white — cards, sidebar, app bar)
- Theme name: `architectLight` (defined in `src/plugins/vuetify.ts`)
- Never hardcode colors — use Vuetify theme tokens or the values above

## Page Layout Structure

Every page using the default layout MUST follow this structure:

```vue
<template>
  <v-container fluid class="pa-6">
    <!-- Page header -->
    <div class="mb-6">
      <div class="text-h5 font-weight-medium">Page Title</div>
      <div class="text-body-2 text-medium-emphasis">Subtitle or description</div>
    </div>

    <!-- Page content (cards, tables, etc.) -->
  </v-container>
</template>

<route lang="yaml">
meta:
  layout: default
  title: 'Page Title'
</route>
```

Key rules:
- Always use `v-container fluid class="pa-6"` as the outermost wrapper
- Page header: `text-h5 font-weight-medium` for title, `text-body-2 text-medium-emphasis` for subtitle
- `mb-6` gap between header and content
- Set `route.meta.title` — it's displayed in the top app bar

### Page header with actions (search, buttons)

When a page header includes actions (search field, buttons), use a flex row:

```vue
<div class="d-flex align-center justify-space-between flex-wrap ga-4 mb-6">
  <div>
    <div class="text-h5 font-weight-medium">Page Title</div>
    <div class="text-body-2 text-medium-emphasis">Subtitle</div>
  </div>
  <!-- Actions go here (search fields, buttons, etc.) -->
</div>
```

## Card Pattern

All cards MUST use the ArchitectUI separated header pattern:

```vue
<v-card rounded="lg">
  <div class="card-header-separated">
    <div class="card-header-title">
      <v-icon color="primary" icon="mdi-icon-name" size="20" />
      Card Title
    </div>
    <!-- Optional: actions on the right side (chips, buttons, etc.) -->
  </div>
  <v-card-text>
    <!-- Card body content -->
  </v-card-text>
</v-card>
```

Key rules:
- Always use `rounded="lg"` on `v-card`
- Do NOT set `elevation` — the global `$shadow-architect` CSS in `settings.scss` handles it
- Header uses `.card-header-separated` class (flex row with bottom border)
- Title uses `.card-header-title` class (flex row with icon + bold text)
- Icon in header: `color="primary"` and `size="20"`
- The border color is `rgba(26, 54, 126, 0.125)` — a subtle blue-tinted separator

### Card with tabs in header

When a card has tabs, place them inside the header:

```vue
<v-card rounded="lg">
  <div class="card-header-separated">
    <v-tabs v-model="tab" color="primary" density="compact">
      <v-tab value="first">First Tab</v-tab>
      <v-tab value="second">Second Tab</v-tab>
    </v-tabs>
  </div>
  <v-window v-model="tab">
    <v-window-item value="first">
      <v-card-text>...</v-card-text>
    </v-window-item>
  </v-window>
</v-card>
```

### Clickable card (e.g. dashboard overview)

```vue
<v-card hover rounded="lg" :to="`/path/${item.id}`">
  <div class="card-header-separated">
    <div class="card-header-title">
      <v-icon color="primary" icon="mdi-database" size="20" />
      {{ item.name }}
    </div>
    <v-chip color="primary" size="x-small" variant="tonal">
      {{ item.type }}
    </v-chip>
  </div>
  <v-card-text>
    <!-- Summary content -->
  </v-card-text>
</v-card>
```

## Type/Status Chips

Use color-coded `v-chip` components for types and statuses:

```vue
<v-chip :color="chipColor" size="x-small" variant="tonal">
  {{ label }}
</v-chip>
```

Standard color mappings:
- **Server types**: Cluster → `primary` (blue), Standalone → `teal`
- **Redis key types**: string → `blue`, hash → `orange`, list → `green`, set → `purple`, zset → `teal`
- **General**: Use `variant="tonal"` for subtle, `variant="elevated"` for prominent

## Data Tables

Use `v-table` with `density="compact"` and `hover`:

```vue
<v-table density="compact" hover>
  <thead>
    <tr>
      <th>Column</th>
      <th style="width: 120px;">Type</th>
      <th class="text-end" style="width: 80px;">Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr v-for="item in items" :key="item.id">
      <td>{{ item.name }}</td>
      <td><v-chip color="blue" size="small" variant="tonal">{{ item.type }}</v-chip></td>
      <td class="text-end">
        <v-btn color="error" icon="mdi-delete-outline" size="small" variant="text" />
      </td>
    </tr>
  </tbody>
</v-table>
```

- Fixed-width columns for Type, TTL, Actions using `style="width: Xpx;"`
- Action column: `class="text-end"` for right-alignment
- Action buttons: `icon` + `size="small"` + `variant="text"`

## Search Bars

### Inline search (embedded button)

For search within a card/tab — button embedded inside the input:

```vue
<div class="search-bar mb-4">
  <v-text-field
    v-model="pattern"
    density="compact"
    hide-details
    placeholder="Search..."
    prepend-inner-icon="mdi-magnify"
    variant="solo-filled"
    flat
    @keydown.enter="search"
  >
    <template #append-inner>
      <v-btn color="primary" :loading="loading" rounded="lg" size="small" variant="elevated" @click="search">
        <v-icon icon="mdi-arrow-right" size="18" />
      </v-btn>
    </template>
  </v-text-field>
</div>
```

Style the search bar with scoped CSS:
```css
.search-bar :deep(.v-field) {
  background-color: #f8f9fa;
  border-radius: 12px;
}
.search-bar :deep(.v-field--focused) {
  background-color: #fff;
}
```

### Page-level filter (standalone)

For filtering content at the page header level:

```vue
<v-text-field
  v-model="search"
  clearable
  density="compact"
  hide-details
  max-width="320"
  placeholder="Search..."
  prepend-inner-icon="mdi-magnify"
  rounded="lg"
  variant="outlined"
/>
```

## Loading States

Use the appropriate loading pattern based on context:

| Context | Pattern |
|---------|---------|
| First page load (no data) | `<v-skeleton-loader type="card" />` or `type="table-thead, table-tbody"` |
| Refresh with existing data | `<v-progress-linear v-if="loading" color="primary" indeterminate />` above content |
| Button action | `:loading="loading"` prop on `v-btn` |
| Full-page load | `v-overlay` with `v-progress-circular` (see `layouts/default.vue`) |

## Empty States

Center vertically with icon + title + description:

```vue
<div class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis">
  <v-icon class="mb-3" icon="mdi-relevant-icon" size="48" />
  <div class="text-subtitle-1">No items found</div>
  <div class="text-body-2">Description of why empty and what to do.</div>
</div>
```

## Error States

Use tonal alerts with retry action:

```vue
<v-alert type="error" variant="tonal">
  <div class="d-flex align-center justify-space-between flex-wrap ga-2">
    <span>{{ errorMessage }}</span>
    <v-btn color="primary" size="small" variant="text" @click="retry">Retry</v-btn>
  </div>
</v-alert>
```

## Dialogs (Popups)

All dialogs MUST use the card-header-separated AND card-footer-separated pattern — header and footer are visually separated from the body with borders:

```vue
<v-dialog v-model="showDialog" max-width="440">
  <v-card rounded="lg">
    <div class="card-header-separated">
      <div class="card-header-title">
        <v-icon color="error" icon="mdi-delete-alert" size="20" />
        Confirm Action
      </div>
    </div>
    <v-card-text>Are you sure?</v-card-text>
    <v-card-actions class="card-footer-separated">
      <v-spacer />
      <v-btn variant="text" @click="cancel">Cancel</v-btn>
      <v-btn color="error" :loading="processing" variant="elevated" @click="confirm">Delete</v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>
```

Key rules for dialogs:
- Header: `.card-header-separated` with icon + title (top border separator)
- Footer: `.card-footer-separated` on `v-card-actions` (bottom border separator)
- Both classes add a subtle blue-tinted border (`rgba(26, 54, 126, 0.125)`)
- Cancel button: `variant="text"` (no color)
- Primary action button: appropriate color + `variant="elevated"`
- Always include `:loading` on primary action button for async operations

## Grid Layout

Use Vuetify grid for responsive card layouts:

```vue
<v-row>
  <v-col v-for="item in items" :key="item.id" cols="12" sm="6" md="4" lg="3">
    <v-card>...</v-card>
  </v-col>
</v-row>
```

Standard breakpoints: `cols="12"` (mobile full), `sm="6"` (2-col), `md="4"` (3-col), `lg="3"` (4-col)

## CSS Classes Reference

These custom classes are defined in `src/styles/settings.scss`:

| Class | Usage |
|-------|-------|
| `.card-header-separated` | Card header row with bottom border separator |
| `.card-header-title` | Flex row inside header: icon + bold title |
| `.card-footer-separated` | Card/dialog footer row with top border separator |
| `.sidebar-section-header` | Uppercase section labels in sidebar (e.g. "SERVERS") |
| `.sidebar-brand` | App brand area at top of sidebar |

Global overrides applied automatically:
- `.v-card` — ArchitectUI soft multi-layer shadow
- `.v-app-bar` — ArchitectUI soft multi-layer shadow
