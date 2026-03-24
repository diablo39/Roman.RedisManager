---
name: ui-scaffold
description: Output design system templates for creating new pages, cards, tables, search bars, dialogs, and other UI patterns. Use when creating or modifying Vue pages/components to ensure visual consistency with the ArchitectUI-inspired design.
argument-hint: [pattern-name]
---

# UI Scaffold — ArchitectUI Design System Templates

Use these templates when building new pages or components. All patterns are established in the codebase and MUST be followed for visual consistency.

Available patterns: `page`, `card`, `card-tabs`, `card-clickable`, `table`, `search-inline`, `search-page`, `loading`, `empty`, `error`, `dialog`, `grid`, `chips`

Requested pattern: $ARGUMENTS

---

## Page Layout

Every page using the default layout:

```vue
<template>
  <v-container fluid class="pa-6">
    <!-- Page header -->
    <div class="mb-6">
      <div class="text-h5 font-weight-medium">Page Title</div>
      <div class="text-body-2 text-medium-emphasis">Subtitle or description</div>
    </div>

    <!-- Content -->
  </v-container>
</template>

<route lang="yaml">
meta:
  layout: default
  title: 'Page Title'
</route>
```

Page header with actions (search, buttons):

```vue
<div class="d-flex align-center justify-space-between flex-wrap ga-4 mb-6">
  <div>
    <div class="text-h5 font-weight-medium">Page Title</div>
    <div class="text-body-2 text-medium-emphasis">Subtitle</div>
  </div>
  <!-- Actions: search fields, buttons, etc. -->
</div>
```

---

## Card — Standard

```vue
<v-card rounded="lg">
  <div class="card-header-separated">
    <div class="card-header-title">
      <v-icon color="primary" icon="mdi-icon-name" size="20" />
      Card Title
    </div>
    <!-- Optional right side: chips, buttons -->
  </div>
  <v-card-text>
    <!-- Body content -->
  </v-card-text>
</v-card>
```

Rules:
- Always `rounded="lg"` on v-card
- Do NOT set `elevation` — global `$shadow-architect` handles shadows
- Header uses `.card-header-separated` (flex row, bottom border `rgba(26, 54, 126, 0.125)`)
- Title uses `.card-header-title` (flex row, icon + bold text)
- Icon: `color="primary"`, `size="20"`

---

## Card — With Tabs

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
      <v-card-text>First tab content</v-card-text>
    </v-window-item>
    <v-window-item value="second">
      <v-card-text>Second tab content</v-card-text>
    </v-window-item>
  </v-window>
</v-card>
```

---

## Card — Clickable (Overview/Dashboard)

```vue
<v-card hover rounded="lg" :to="`/path/${item.id}`">
  <div class="card-header-separated">
    <div class="card-header-title">
      <v-icon color="primary" icon="mdi-database" size="20" />
      {{ item.name }}
    </div>
    <v-chip :color="chipColor" size="x-small" variant="tonal">
      {{ item.type }}
    </v-chip>
  </div>
  <v-card-text>
    <div class="d-flex align-center text-body-2 text-medium-emphasis">
      <v-icon class="mr-2" icon="mdi-server" size="18" />
      {{ item.count }} items
    </div>
  </v-card-text>
</v-card>
```

---

## Data Table

```vue
<v-table density="compact" hover>
  <thead>
    <tr>
      <th>Name</th>
      <th style="width: 120px;">Type</th>
      <th style="width: 140px;">Value</th>
      <th class="text-end" style="width: 80px;">Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr v-for="item in items" :key="item.id">
      <td class="font-weight-medium" style="font-family: monospace; font-size: 0.85rem;">
        {{ item.name }}
      </td>
      <td>
        <v-chip :color="typeColor(item.type)" size="small" variant="tonal">
          {{ item.type }}
        </v-chip>
      </td>
      <td class="text-body-2 text-medium-emphasis">{{ item.value }}</td>
      <td class="text-end">
        <v-btn color="error" icon="mdi-delete-outline" size="small" title="Delete" variant="text" />
      </td>
    </tr>
  </tbody>
</v-table>
```

Rules:
- Fixed-width columns for Type, Value, Actions via `style="width: Xpx;"`
- Action column: `class="text-end"`
- Action buttons: icon + `size="small"` + `variant="text"`
- Monospace for key/code values

---

## Search Bar — Inline (inside a card)

Button embedded inside the text field:

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
      <v-btn
        color="primary"
        :loading="loading"
        rounded="lg"
        size="small"
        variant="elevated"
        @click="search"
      >
        <v-icon icon="mdi-arrow-right" size="18" />
      </v-btn>
    </template>
  </v-text-field>
</div>
```

Required scoped CSS:
```css
.search-bar :deep(.v-field) {
  background-color: #f8f9fa;
  border-radius: 12px;
}
.search-bar :deep(.v-field--focused) {
  background-color: #fff;
}
```

---

## Search Bar — Page Level (filter in header)

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

---

## Type/Status Chips

```vue
<v-chip :color="chipColor" size="x-small" variant="tonal">
  {{ label }}
</v-chip>
```

Color mappings:
- Server types: Cluster → `primary`, Standalone → `teal`
- Redis key types: string → `blue`, hash → `orange`, list → `green`, set → `purple`, zset → `teal`
- General: `variant="tonal"` for subtle, `variant="elevated"` for prominent

---

## Loading States

| Context | Pattern |
|---------|---------|
| First load (no data) | `<v-skeleton-loader type="card" />` or `type="table-thead, table-tbody"` |
| Refresh (data exists) | `<v-progress-linear v-if="loading" color="primary" indeterminate />` above content |
| Button action | `:loading="loading"` prop on `v-btn` |
| Full-page load | `v-overlay` with `v-progress-circular` (see `layouts/default.vue`) |

---

## Empty State

```vue
<div class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis">
  <v-icon class="mb-3" icon="mdi-relevant-icon" size="48" />
  <div class="text-subtitle-1">No items found</div>
  <div class="text-body-2">Description of why empty and what to do.</div>
</div>
```

---

## Error State

```vue
<v-alert type="error" variant="tonal">
  <div class="d-flex align-center justify-space-between flex-wrap ga-2">
    <span>{{ errorMessage }}</span>
    <v-btn color="primary" size="small" variant="text" @click="retry">Retry</v-btn>
  </div>
</v-alert>
```

---

## Confirmation Dialog

```vue
<v-dialog v-model="showDialog" max-width="440">
  <v-card rounded="lg">
    <div class="card-header-separated">
      <div class="card-header-title">
        <v-icon color="error" icon="mdi-delete-alert" size="20" />
        Confirm Action
      </div>
    </div>
    <v-card-text>
      Are you sure you want to delete <strong>{{ item.name }}</strong>?
      This action cannot be undone.
    </v-card-text>
    <v-card-actions>
      <v-spacer />
      <v-btn variant="text" @click="cancel">Cancel</v-btn>
      <v-btn color="error" :loading="processing" variant="elevated" @click="confirm">
        Delete
      </v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>
```

---

## Responsive Grid

```vue
<v-row>
  <v-col v-for="item in items" :key="item.id" cols="12" sm="6" md="4" lg="3">
    <v-card>...</v-card>
  </v-col>
</v-row>
```

Breakpoints: `cols="12"` mobile, `sm="6"` 2-col, `md="4"` 3-col, `lg="3"` 4-col
