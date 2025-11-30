# Style Guide: Component Templates

## Unique Patterns in This Codebase

### 1. Signal Interpolation with Function Calls
**Unique to signal-based components:** Signals are called as functions in templates.

**Example:**
```html
<h1>Hello, {{ title() }}</h1>
```

**Convention:**
- **Function call syntax**: Always use `()` when accessing signals
- **Not property access**: Don't use `{{ title }}` - won't work
- **Applies to all signals**: Both writable and computed signals

---

### 2. Self-Closing Tag Syntax
**Modern Angular pattern:** Self-closing tags for components without content.

**Example:**
```html
<router-outlet />
<p-button label="Click me" icon="pi pi-check" />
```

**Convention:**
- **Use `/>`**: Self-closing syntax for empty tags
- **Cleaner markup**: More concise than `<router-outlet></router-outlet>`
- **PrimeNG components**: Many PrimeNG components work well with this syntax

---

### 3. PrimeNG Component Conventions
**PrimeNG-specific patterns:** Use PrimeNG component tag names and attributes.

**Example:**
```html
<p-button label="PrimeNG Works!" icon="pi pi-check" />
<p-table [value]="data">
  <!-- table content -->
</p-table>
```

**Convention:**
- **`p-` prefix**: All PrimeNG components use the `p-` prefix
- **Attribute bindings**: Use PrimeNG's specific attributes (e.g., `label`, `icon`)
- **PrimeIcons**: Icon classes use `pi pi-{name}` format
- **Component API**: Follow PrimeNG's documentation for each component

---

### 4. Router Outlet Placement
**Project pattern:** Router outlet placed at the end of root template.

**Example:**
```html
<main class="main">
  <!-- App shell content -->
</main>

<!-- Router outlet for routed content -->
<router-outlet />
```

**Convention:**
- **End of template**: Place `<router-outlet />` after main app content
- **Separate from shell**: Keep routed content separate from app shell
- **Self-closing**: Use `<router-outlet />` syntax

---

### 5. Modern Angular Control Flow (Future)
**Expected pattern:** Use `@for`, `@if`, `@switch` instead of structural directives.

**Current example in template:**
```html
@for (item of [
  { title: 'Explore the Docs', link: 'https://angular.dev' },
  { title: 'Learn with Tutorials', link: 'https://angular.dev/tutorials' },
  // ...
]; track item.title) {
  <a class="pill" [href]="item.link" target="_blank" rel="noopener">
    <span>{{ item.title }}</span>
  </a>
}
```

**Convention:**
- **`@for` loops**: Use `@for` instead of `*ngFor`
- **Track expression**: Always include `track` for performance
- **`@if` conditionals**: Use `@if` instead of `*ngIf`
- **Block syntax**: Wrap content in `{ }` blocks

---

### 6. Inline Data Arrays in Templates
**Unique pattern:** Small static data arrays defined directly in templates.

**Example:**
```html
@for (item of [
  { title: 'Explore the Docs', link: 'https://angular.dev' },
  { title: 'Learn with Tutorials', link: 'https://angular.dev/tutorials' },
  { title: 'CLI Docs', link: 'https://angular.dev/tools/cli' },
]; track item.title) {
  <a [href]="item.link">{{ item.title }}</a>
}
```

**Convention:**
- **Small static arrays**: Acceptable for simple, static data
- **Array of objects**: Use object literal syntax
- **Track by unique property**: Use stable identifier for tracking

---

### 7. Accessibility Attributes
**Standard but emphasized:** Always include proper accessibility attributes.

**Example:**
```html
<div class="divider" role="separator" aria-label="Divider"></div>
<a href="https://github.com/angular/angular" aria-label="Github" target="_blank" rel="noopener">
  <svg><!-- icon --></svg>
</a>
```

**Convention:**
- **ARIA labels**: Add `aria-label` to non-text elements
- **Roles**: Use `role` attribute for semantic meaning
- **Link safety**: Use `rel="noopener"` for external links
- **Descriptive labels**: Make labels meaningful

---

## Summary Checklist

When creating templates in this codebase:

- [ ] Use `{{ signal() }}` syntax for signals (with parentheses)
- [ ] Use self-closing tags `<component />` where appropriate
- [ ] Use PrimeNG components with `p-` prefix
- [ ] Use PrimeIcons with `pi pi-{name}` classes
- [ ] Place `<router-outlet />` at end of root template
- [ ] Use `@for`, `@if`, `@switch` (modern control flow)
- [ ] Include `track` expression in `@for` loops
- [ ] Add proper accessibility attributes (aria-label, role, etc.)
- [ ] Use `rel="noopener"` for external links
- [ ] Keep templates in separate `.html` files (never inline)
