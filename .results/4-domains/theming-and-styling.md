# Theming and Styling Domain Analysis

## Overview
The application uses PrimeNG's theming system with the Aura preset, combined with PrimeFlex utilities and component-scoped CSS.

## Key Patterns and Conventions

### PrimeNG Theme Configuration
Theme is configured at the application level:

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

**Pattern:**
- Import `providePrimeNG` from 'primeng/config'
- Import theme preset (Aura) from '@primeuix/themes/aura'
- Configure theme in application providers
- Theme applies globally to all PrimeNG components

### Global Styles
Global styles are defined in a central stylesheet:

**From `src/styles.css`:**
```css
/* You can add global styles to this file, and also import other style files */
```

**Usage:**
- Currently minimal (placeholder)
- Location for application-wide CSS
- Imported automatically by Angular build process
- Referenced in `angular.json` styles array

### Component-Scoped Styles
Each component has its own stylesheet:

**Pattern (from component structure):**
```typescript
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'  // Component-scoped styles
})
```

**From `src/app/app.css`:**
```css
/* Component-specific styles */
```

**Scoping:**
- Styles are scoped to the component by default (Angular's ViewEncapsulation.Emulated)
- Selectors automatically namespaced to prevent global pollution
- `:host` selector targets the component's host element

### CSS Custom Properties
The application uses modern CSS custom properties for theming:

**From `src/app/app.html` (embedded styles example):**
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
- Use OKLCH color space for modern color definitions
- Define CSS custom properties on `:host` for component-level themes
- Create reusable gradient definitions as custom properties
- Descriptive naming (e.g., `--bright-blue`, `--red-to-pink-to-purple-vertical-gradient`)

### PrimeFlex Utility Classes
PrimeFlex 3.3.1 is available for layout and spacing:

**Available Utilities:**
- Flexbox utilities: `flex`, `flex-column`, `flex-wrap`, etc.
- Grid utilities: `grid`, `col-*`, etc.
- Spacing: `p-*` (padding), `m-*` (margin)
- Typography: Text alignment, sizing, etc.
- Display utilities: `block`, `inline-block`, `hidden`, etc.

**Usage Pattern:**
```html
<div class="flex align-items-center justify-content-between p-3 gap-2">
  <!-- Content -->
</div>
```

### PrimeIcons
Icon library available via PrimeIcons 7.0.0:

**Usage:**
```html
<i class="pi pi-check"></i>
<i class="pi pi-times"></i>
<i class="pi pi-search"></i>
```

**Pattern:**
- Base class: `pi`
- Icon-specific class: `pi-{icon-name}`

## Tools and Technologies

### Theming Stack
- **@primeuix/themes 2.0.1**: Theme system
- **Aura Preset**: Default theme configuration
- **PrimeNG 20.3.0**: Component library respecting theme

### Styling Tools
- **PrimeFlex 3.3.1**: CSS utility framework
- **PrimeIcons 7.0.0**: Icon library
- **CSS Custom Properties**: Modern CSS variables
- **OKLCH Color Space**: Advanced color definitions

### Build Integration
- **Angular Build System**: Compiles and bundles styles
- **Component Style Encapsulation**: ViewEncapsulation.Emulated by default

## Implementation Guidelines

### Adding Component Styles
1. Create `{component}.css` file alongside component
2. Reference via `styleUrl: './{component}.css'`
3. Use `:host` selector for component root styling
4. Leverage CSS custom properties for themeable values

### Using PrimeNG Theme
- All PrimeNG components automatically use configured Aura theme
- No additional configuration needed per component
- Theme can be customized by modifying providePrimeNG configuration

### Layout with PrimeFlex
Preferred approach for layouts:
```html
<div class="grid">
  <div class="col-12 md:col-6 lg:col-4">
    <!-- Responsive grid -->
  </div>
</div>
```

### Global vs Component Styles
**Global Styles (`src/styles.css`):**
- CSS resets
- Global utility classes
- Application-wide typography
- Theme overrides

**Component Styles:**
- Component-specific layouts
- Local design patterns
- Scoped animations

### Color Management
- Use OKLCH for color definitions (modern, perceptually uniform)
- Define colors as CSS custom properties for reusability
- Create semantic color names (`--primary`, `--danger`, etc.)

### Responsive Design
- Use PrimeFlex breakpoint utilities: `sm:`, `md:`, `lg:`, `xl:`
- Mobile-first approach
- Test across viewport sizes

### Icon Usage
- Import PrimeIcons if not globally available
- Use semantic icon names
- Combine with PrimeFlex for sizing: `<i class="pi pi-check text-2xl"></i>`
