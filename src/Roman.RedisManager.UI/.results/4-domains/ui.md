# Domain: UI

## Overview
The UI domain in this project is built on **Angular 20** with **standalone components** and **PrimeNG** as the UI component library. The architecture embraces modern Angular patterns including signal-based reactivity and separation of concerns with distinct files for logic, templates, and styles.

## Component Architecture

### Standalone Components
All components in this project must be standalone (no NgModules). This is the default and required pattern in Angular 20.

**Example from `src/app/app.ts`:**
```typescript
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('roman-redis-manager-ui');
}
```

**Key Patterns:**
- **Explicit imports**: Component dependencies declared in `imports` array
- **No standalone property**: Implicit in Angular 20 (standalone by default)
- **PrimeNG module imports**: UI components imported from PrimeNG (e.g., `ButtonModule`)
- **File separation**: `templateUrl` and `styleUrl` point to separate files

### Signal-Based State Management
Components use Angular's **signal()** API for reactive state instead of traditional class properties.

**Example from `src/app/app.ts`:**
```typescript
export class App {
  protected readonly title = signal('roman-redis-manager-ui');
}
```

**Patterns:**
- Use `signal()` for component state that changes over time
- Signals are `protected` or `private` for encapsulation 
- Marked as `readonly` when the signal reference itself shouldn't change
- Accessed in templates with function call syntax: `{{ title() }}`

### Template Structure
Templates are separated into `.html` files and referenced via `templateUrl`.

**Example from `src/app/app.html`:**
```html
<main class="main">
  <div class="content">
    <div class="left-side">
      <h1>Hello, {{ title() }}</h1>
      <p>Congratulations! Your app is running. 🎉</p>
      <div style="margin-top: 1.5rem;">
        <p-button label="PrimeNG Works!" icon="pi pi-check" />
      </div>
    </div>
    <!-- ... more content ... -->
  </div>
</main>

<router-outlet />
```

**Patterns:**
- **Signal interpolation**: `{{ title() }}` - signals called as functions
- **PrimeNG components**: `<p-button>` with PrimeNG-specific attributes (`label`, `icon`)
- **PrimeIcons**: Icon classes use `pi pi-*` format
- **Router outlet**: Always included in root component template
- **Modern Angular syntax**: `@for` loops, standalone syntax

### Component Styles
Styles are scoped to components using separate `.css` files referenced via `styleUrl`.

**Example from `src/app/app.css`:**
- Currently empty for the root component since styles are embedded in template
- Component-specific styles should be defined here for better separation

**Patterns:**
- One `.css` file per component
- Styles are scoped to the component automatically
- Global styles go in `src/styles.css`

## PrimeNG Integration

### Component Usage
All UI components must come from the **PrimeNG** library.

**Import Pattern:**
```typescript
import { ButtonModule } from 'primeng/button';

@Component({
  imports: [ButtonModule],
  // ...
})
```

**Template Usage:**
```html
<p-button label="PrimeNG Works!" icon="pi pi-check" />
```

### Theming
PrimeNG theme is configured globally in the application config.

**From `src/app/app.config.ts`:**
```typescript
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

export const appConfig: ApplicationConfig = {
  providers: [
    // ...
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
```

**Patterns:**
- Theme configured once in `app.config.ts`
- Uses **Aura** preset from `@primeuix/themes`
- Applied globally to all PrimeNG components

### Icons
PrimeIcons library is used for all icons.

**Pattern:**
```html
<p-button icon="pi pi-check" />
```

- Icon class format: `pi pi-{icon-name}`
- Complete icon set from PrimeIcons package

## File Naming Conventions

### Component Files
Each component consists of three files:

1. **Logic**: `{component-name}.ts`
2. **Template**: `{component-name}.html`
3. **Styles**: `{component-name}.css`

**Example:**
- `app.ts` - Component class
- `app.html` - Template
- `app.css` - Styles

### Component Class Naming
Component classes use PascalCase without "Component" suffix.

**Example:**
```typescript
export class App {  // Not "AppComponent"
  // ...
}
```

## Summary

The UI domain follows these core principles:

1. **Standalone components** with explicit imports
2. **Signal-based reactivity** for component state
3. **PrimeNG** as the exclusive UI component library
4. **File separation** for logic, templates, and styles
5. **Aura theme** for consistent PrimeNG styling
6. **PrimeIcons** for iconography
7. **Modern Angular 20** patterns throughout
