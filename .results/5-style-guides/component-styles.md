# Component Styles Style Guide

## Unique Conventions

This style guide covers **project-specific styling patterns** for component CSS files.

### 1. Minimal Component Styles

**Project pattern:**
- Component CSS files are intentionally minimal or empty
- Styles defined in templates or rely on PrimeNG theming

**From `src/app/app.css`:**
```css
/* Component-specific styles */
```

**Current state:**
- File exists but is empty
- Serves as placeholder for future component-specific styles

### 2. Style Definition Preference

**Unique to this project:**
The project shows a preference for:
1. **Embedded template styles** (in `<style>` blocks within HTML)
2. **PrimeNG theme styles** (global styling via Aura preset)
3. **Component CSS files** as fallback/supplement

**From `src/app/app.html`:**
```html
<style>
  :host {
    --bright-blue: oklch(51.01% 0.274 263.83);
    /* More custom properties */
  }
  
  .main {
    /* Component styles */
  }
</style>
```

**This differs from standard Angular practice** where most developers prefer external CSS files over embedded styles.

### 3. CSS Custom Properties as Primary Theming Mechanism

**Project-specific approach:**
- Theme via CSS custom properties (CSS variables)
- Define on `:host` selector for component scope
- Use OKLCH color space

**Pattern:**
```css
:host {
  /* Color definitions using OKLCH */
  --primary-color: oklch(51.01% 0.274 263.83);
  --secondary-color: oklch(53.18% 0.28 296.97);
  
  /* Gradient definitions */
  --gradient-main: linear-gradient(180deg,
    var(--primary-color) 0%,
    var(--secondary-color) 100%);
}
```

**Usage in styles:**
```css
.button {
  background: var(--primary-color);
}

.header {
  background: var(--gradient-main);
}
```

### 4. ViewEncapsulation Strategy

**Project default:**
- Uses Angular's default ViewEncapsulation.Emulated
- Styles are scoped to component automatically
- No explicit encapsulation declaration needed

**Implied behavior:**
```typescript
@Component({
  selector: 'app-component',
  templateUrl: './component.html',
  styleUrl: './component.css'
  // ViewEncapsulation.Emulated is default
})
```

**Effect:**
- Selectors automatically namespaced
- Prevents style leakage to other components
- `:host` selector targets component's root element

### 5. External CSS File as Supplement

**When to use component CSS files in this project:**

1. **Complex component-specific layouts:**
   ```css
   .data-grid-container {
     display: grid;
     grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
     gap: 1rem;
   }
   ```

2. **Component-specific animations:**
   ```css
   @keyframes fadeIn {
     from { opacity: 0; }
     to { opacity: 1; }
   }
   
   .fade-in {
     animation: fadeIn 0.3s ease-in;
   }
   ```

3. **Responsive breakpoints:**
   ```css
   @media (max-width: 768px) {
     .sidebar {
       display: none;
     }
   }
   ```

### 6. Integration with PrimeFlex

**Project approach:**
- Leverage PrimeFlex utility classes in templates
- Minimize custom CSS when utilities suffice

**Example:**
```html
<!-- Prefer this (PrimeFlex utilities) -->
<div class="flex align-items-center justify-content-between p-3 gap-2">
  <!-- Content -->
</div>

<!-- Over this (custom CSS) -->
<div class="custom-flex-container">
  <!-- Content -->
</div>
```

**Custom CSS only when needed:**
```css
/* Only write custom CSS for truly unique layouts */
.custom-layout {
  display: grid;
  grid-template-areas:
    "header header"
    "sidebar main"
    "footer footer";
}
```

### 7. Global Styles Location

**Project convention:**
- Global styles in `src/styles.css`
- Component styles in component `.css` files
- Clear separation of concerns

**From `src/styles.css`:**
```css
/* You can add global styles to this file, and also import other style files */
```

**Current state:** Minimal/empty, reserving for global overrides or resets

## Key Takeaways

When styling components in this project:

1. **Minimal CSS Files**: Component CSS files are often empty or minimal
2. **Embedded Styles**: Template `<style>` blocks are acceptable for component-specific styles
3. **Custom Properties**: Use CSS variables with OKLCH color space
4. **PrimeFlex First**: Use utility classes before writing custom CSS
5. **PrimeNG Theme**: Rely on Aura preset for consistent component styling
6. **Scoping**: Leverage `:host` for component root styles
7. **Global Styles**: Reserve `styles.css` for truly global concerns

## Styling Decision Tree

```
Need to style something?
├─ Is it a PrimeNG component?
│  └─ Use PrimeNG theme configuration
├─ Is it a layout concern?
│  └─ Use PrimeFlex utility classes
├─ Is it component-specific?
│  ├─ Simple styles? → Template <style> block
│  └─ Complex styles? → External .css file
└─ Is it global?
   └─ Add to src/styles.css
```

## Example Component Style File

**Typical `component.css` structure (when needed):**
```css
/* Component-scoped custom properties */
:host {
  --component-spacing: 1rem;
  --component-border-radius: 4px;
}

/* Component root element styles */
:host {
  display: block;
  padding: var(--component-spacing);
}

/* Component-specific layout */
.component-grid {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: var(--component-spacing);
}

/* Responsive design */
@media (max-width: 768px) {
  .component-grid {
    grid-template-columns: 1fr;
  }
}

/* Component-specific animations */
@keyframes slideIn {
  from { transform: translateX(-100%); }
  to { transform: translateX(0); }
}

.slide-in {
  animation: slideIn 0.3s ease-out;
}
```
