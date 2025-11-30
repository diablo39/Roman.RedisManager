# Domain: Theming and Styling

## Overview
The theming and styling approach combines **PrimeNG's theming system** with **component-scoped CSS**. Global theme configuration uses the Aura preset, while component styles are colocated with their components.

## PrimeNG Theming

### Theme Configuration
PrimeNG theme is configured globally using the `providePrimeNG()` function.

**From `src/app/app.config.ts`:**
```typescript
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
```

**Patterns:**
- **Aura preset**: Official PrimeNG theme from `@primeuix/themes/aura`
- **Global configuration**: Applied once in application config
- **Automatic application**: All PrimeNG components use this theme

### Theme Package Dependencies
**From `package.json`:**
```json
{
  "dependencies": {
    "@primeuix/themes": "^2.0.1",
    "primeicons": "^7.0.0",
    "primeng": "^20.3.0",
    "primeflex": "^3.3.1"
  }
}
```

**Packages:**
- **@primeuix/themes**: Theme presets including Aura
- **primeicons**: Icon library for PrimeNG
- **primeng**: Core PrimeNG component library
- **primeflex**: CSS utility library for layouts

## Global Styles

### Global CSS File
Global styles are defined in `src/styles.css`.

**From `src/styles.css`:**
```css
/* You can add global styles to this file, and also import other style files */
```

**Patterns:**
- **Minimal global styles**: Keep global styles to a minimum
- **Theme imports**: PrimeNG theme automatically included
- **Utility classes**: Can import PrimeFlex utilities here if needed

### Expected Global Style Usage
When global styles are needed:

```css
/* Global resets */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

/* PrimeFlex utilities (if imported) */
@import 'primeflex/primeflex.css';

/* Global typography */
body {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}
```

## Component Styles

### Component-Scoped CSS
Each component has its own CSS file referenced via `styleUrl`.

**From `src/app/app.ts`:**
```typescript
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'  // Component-specific styles
})
export class App {
  // ...
}
```

**Patterns:**
- **styleUrl property**: Points to component's CSS file
- **Automatic scoping**: Styles automatically scoped to component
- **Colocated files**: CSS file in same directory as component

### Embedded Styles in Template
The app template currently has embedded styles for demonstration purposes.

**From `src/app/app.html`:**
```html
<style>
  :host {
    --bright-blue: oklch(51.01% 0.274 263.83);
    --electric-violet: oklch(53.18% 0.28 296.97);
    --french-violet: oklch(47.66% 0.246 305.88);
    /* ... more CSS custom properties ... */
    
    font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
      Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji",
      "Segoe UI Symbol";
    box-sizing: border-box;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
  }

  h1 {
    font-size: 3.125rem;
    color: var(--gray-900);
    font-weight: 500;
    /* ... */
  }
  
  /* ... more styles ... */
</style>
```

**Note:** These embedded styles should be moved to `app.css` for better separation of concerns.

### Color System
The app uses **OKLCH color space** for modern, perceptually uniform colors.

**Color Variables:**
```css
--bright-blue: oklch(51.01% 0.274 263.83);
--electric-violet: oklch(53.18% 0.28 296.97);
--french-violet: oklch(47.66% 0.246 305.88);
--vivid-pink: oklch(69.02% 0.277 332.77);
--hot-red: oklch(61.42% 0.238 15.34);
--orange-red: oklch(63.32% 0.24 31.68);

--gray-900: oklch(19.37% 0.006 300.98);
--gray-700: oklch(36.98% 0.014 302.71);
--gray-400: oklch(70.9% 0.015 304.04);
```

**Patterns:**
- **CSS custom properties**: For theming and reusability
- **OKLCH format**: Modern color space for better color perception
- **Gradients**: Defined as CSS custom properties for reuse

### Typography
The application uses **Inter** font family.

```css
font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
  Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji",
  "Segoe UI Symbol";
```

**Patterns:**
- **Inter font**: Primary typeface (should be loaded via link tag or import)
- **System fallbacks**: Falls back to system fonts
- **Font smoothing**: Antialiasing enabled for better rendering

## PrimeFlex Integration

### Utility CSS Framework
PrimeFlex provides utility classes for layouts and spacing.

**Expected Usage:**
```html
<!-- Flexbox utilities -->
<div class="flex justify-content-center align-items-center">
  <!-- content -->
</div>

<!-- Spacing utilities -->
<div class="p-3 m-2">
  <!-- content -->
</div>

<!-- Grid system -->
<div class="grid">
  <div class="col-12 md:col-6 lg:col-4">
    <!-- content -->
  </div>
</div>
```

**Patterns:**
- **Responsive modifiers**: `md:`, `lg:`, etc.
- **Flexbox utilities**: `flex`, `justify-content-*`, `align-items-*`
- **Spacing scale**: `p-{0-8}`, `m-{0-8}`

## Styling Best Practices

### Component Style Isolation
1. **Keep component styles in .css files**: Better separation of concerns
2. **Use :host selector**: For component root element styling
3. **Avoid global styles**: Prefer component-scoped styles

### PrimeNG Component Styling
1. **Use theme variables**: Leverage PrimeNG's CSS variables
2. **Component-specific props**: Use PrimeNG component properties for styling when possible
3. **Custom overrides**: Use specific CSS selectors when needed

### Responsive Design
1. **Mobile-first**: Design for mobile, enhance for larger screens
2. **PrimeFlex breakpoints**: Use responsive modifiers
3. **CSS custom properties**: For themeable values

## Summary

The theming and styling domain follows these principles:

1. **PrimeNG Aura theme** as the foundation
2. **Component-scoped styles** in `.css` files
3. **Minimal global styles** in `styles.css`
4. **OKLCH color space** for modern color management
5. **CSS custom properties** for theming
6. **PrimeFlex** for layout utilities
7. **Inter font family** for typography
