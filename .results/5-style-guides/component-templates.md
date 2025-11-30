# Component Templates Style Guide

## Unique Conventions

This style guide focuses on **project-specific template patterns** found in this codebase.

### 1. Signal Function Call Syntax

**Project-specific pattern:**
- Access signal values using function call syntax in templates
- This is Angular's signal syntax, emphasized in this project

**From `src/app/app.html`:**
```html
<h1>Hello, {{ title() }}</h1>  <!-- Function call for signal values -->
```

**Corresponding TypeScript:**
```typescript
protected readonly title = signal('roman-redis-manager-ui');
```

**Pattern:**
```html
<!-- Signal values -->
{{ mySignal() }}

<!-- Regular properties -->
{{ myProperty }}
```

### 2. Self-Closing Router Outlet

**Project convention:**
- Use self-closing tag for router-outlet

**From `src/app/app.html`:**
```html
<router-outlet />  <!-- Self-closing syntax -->
```

**Not:**
```html
<router-outlet></router-outlet>
```

### 3. Embedded CSS Custom Properties

**Unique pattern in this codebase:**
- CSS custom properties defined directly in template using `:host` selector
- OKLCH color space for modern color definitions

**From `src/app/app.html`:**
```html
<style>
  :host {
    --bright-blue: oklch(51.01% 0.274 263.83);
    --electric-violet: oklch(53.18% 0.28 296.97);
    --french-violet: oklch(47.66% 0.246 305.88);
    --vivid-pink: oklch(69.02% 0.277 332.77);
    --hot-red: oklch(61.42% 0.238 15.34);
    --orange-red: oklch(63.32% 0.24 31.68);

    --gray-900: oklch(19.37% 0.006 300.98);
    --gray-700: oklch(36.98% 0.014 302.71);
    --gray-400: oklch(70.9% 0.015 304.04);

    --red-to-pink-to-purple-vertical-gradient: linear-gradient(180deg,
        var(--orange-red) 0%,
        var(--vivid-pink) 50%,
        var(--electric-violet) 100%);

    --red-to-pink-to-purple-horizontal-gradient: linear-gradient(90deg,
        var(--orange-red) 0%,
        var(--vivid-pink) 50%,
        var(--electric-violet) 100%);
  }
</style>
```

**Conventions:**
- OKLCH color space: `oklch(lightness% chroma hue)`
- Descriptive variable names (`--bright-blue`, not `--color-1`)
- Gradients defined as reusable custom properties
- All properties scoped to `:host`

### 4. Semantic HTML Structure

**Project pattern:**
- Use semantic HTML5 elements
- Clear content structure

**From `src/app/app.html`:**
```html
<main class="main">
  <div class="content">
    <div class="left-side">
      <!-- Content -->
    </div>
    <div class="right-side">
      <!-- Content -->
    </div>
  </div>
</main>
```

**Conventions:**
- `<main>` for primary content area
- Descriptive class names (`content`, `left-side`, `right-side`)
- Nested structure for layout organization

### 5. Angular Standalone Placeholder Comments

**Unique to this scaffold:**
- Extensive comments marking placeholder content
- Indicates areas for customization

**From `src/app/app.html`:**
```html
<!-- * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * -->
<!-- * * * * * * * * * * * The content below * * * * * * * * * * * -->
<!-- * * * * * * * * * * is only a placeholder * * * * * * * * * * -->
<!-- * * * * * * * * * * and can be replaced.  * * * * * * * * * * -->
<!-- * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * -->
<!-- * * * * * * * * * Delete the template below * * * * * * * * * -->
<!-- * * * * * * * to get started with your project! * * * * * * * -->
<!-- * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * -->
```

**Note:** This is Angular CLI scaffold content. Production code should replace these placeholders.

### 6. Optional Chaining for Safe Navigation

**Best practice shown in tests (applicable to templates):**
```typescript
expect(compiled.querySelector('h1')?.textContent).toContain('Hello');
```

**Template equivalent:**
```html
{{ user?.profile?.name }}
```

## Key Takeaways

When creating templates in this project:

1. **Signals**: Always use function call syntax `{{ signal() }}`
2. **Router**: Self-closing `<router-outlet />` tag
3. **Colors**: Use OKLCH color space in CSS custom properties
4. **Custom Properties**: Define in `<style>` block with `:host` scope
5. **Semantic HTML**: Use `<main>`, `<section>`, `<article>`, etc.
6. **Gradients**: Define as CSS custom properties for reusability
7. **Comments**: Remove placeholder comments in production code
8. **Structure**: Organize content with clear, descriptive class names

## Template Structure Pattern

Typical component template structure:
```html
<!-- Optional: Component-specific styles -->
<style>
  :host {
    /* CSS custom properties */
  }
</style>

<!-- Main content -->
<main class="component-name">
  <div class="component-section">
    <!-- Component content -->
    <h1>{{ title() }}</h1>
  </div>
</main>

<!-- Router outlet if needed -->
<router-outlet />
```
