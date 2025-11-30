# UI Domain Analysis

## Overview
The UI domain in this codebase uses Angular 20.3.0 with the modern standalone component architecture, integrated with PrimeNG component library for UI elements.

## Key Patterns and Conventions

### Standalone Component Architecture
The application exclusively uses Angular's standalone component API, eliminating the need for NgModules:

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

**Key Observations:**
- Components import dependencies directly in the `imports` array
- No `standalone: true` declaration needed (implied by imports array presence)
- Component class names don't use "Component" suffix (e.g., `App` not `AppComponent`)
- Selector follows `app-*` naming convention

### Signal-Based Reactive State
The codebase uses Angular signals for reactive state management:

```typescript
protected readonly title = signal('roman-redis-manager-ui');
```

**Conventions:**
- Signals are typically `readonly` when the reference shouldn't change
- Signal properties are `protected` when used only in the template
- Primitive values wrapped in signals for reactivity

### File Structure Pattern
Components follow a three-file pattern:
- `{name}.ts` - Component logic
- `{name}.html` - Template
- `{name}.css` - Styles

**Example:**
- `app.ts`
- `app.html`
- `app.css`

This differs from the traditional Angular naming with `.component.` infix.

### PrimeNG Integration
All UI components leverage the PrimeNG library:

```typescript
import { ButtonModule } from 'primeng/button';
```

**Integration Pattern:**
- Import specific PrimeNG component modules as needed
- Add to component's `imports` array
- PrimeNG theme configured globally in `app.config.ts`

### Template Separation
Templates are external files referenced via `templateUrl`:

```typescript
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],
  templateUrl: './app.html',  // External template
  styleUrl: './app.css'        // External styles
})
```

**From `app.html`:**
```html
<main class="main">
  <div class="content">
    <h1>Hello, {{ title() }}</h1>
    <!-- Template content -->
  </div>
</main>

<router-outlet />
```

**Template Conventions:**
- Signal values accessed with function call syntax: `{{ title() }}`
- Router outlet included for routing support
- Semantic HTML elements (`<main>`, `<section>`, etc.)

### Component Styling
Styles use component-scoped CSS:

**Pattern:**
- Each component has its own `.css` file
- Styles are scoped to the component by default (ViewEncapsulation.Emulated)
- Global styles defined in `src/styles.css`

**From `app.html` (embedded style example):**
```html
<style>
  :host {
    --bright-blue: oklch(51.01% 0.274 263.83);
    --electric-violet: oklch(53.18% 0.28 296.97);
    /* CSS custom properties */
  }
</style>
```

Note: The template currently has embedded styles (likely placeholder), but the pattern supports external CSS files.

## Tools and Technologies

### Core UI Framework
- **Angular 20.3.0**: Latest Angular with standalone components
- **TypeScript 5.9.2**: Strict mode enabled
- **Zone.js 0.15.0**: Change detection mechanism

### Component Library
- **PrimeNG 20.3.0**: Primary UI component library
- **PrimeIcons 7.0.0**: Icon set
- **PrimeFlex 3.3.1**: CSS utility framework
- **@primeuix/themes 2.0.1**: Theming system with Aura preset

### Supporting Libraries
- **RxJS 7.8.0**: Reactive programming (though signals preferred for simple state)

## Implementation Guidelines

### Creating a New Component
1. Create three files: `{name}.ts`, `{name}.html`, `{name}.css`
2. Use standalone component decorator with imports array
3. Import required PrimeNG modules
4. Use signals for reactive state
5. Access signals in templates with function call syntax

### State Management
- Use signals for component-local state
- Mark signal properties as `readonly` when reference shouldn't change
- Use `protected` access modifier for template-only properties

### PrimeNG Usage
- Import specific component modules (e.g., `ButtonModule`, `InputTextModule`)
- Add to component's `imports` array
- Respect PrimeNG's theming system configured in app.config.ts

### TypeScript Strictness
All components must satisfy strict TypeScript settings:
- `strict: true`
- `noImplicitOverride: true`
- `noImplicitReturns: true`
- `noFallthroughCasesInSwitch: true`

### Naming Conventions
- **Component class**: `{Name}` (e.g., `App`, not `AppComponent`)
- **Selector**: `app-{name}` (e.g., `app-root`, `app-dashboard`)
- **Files**: `{name}.ts`, `{name}.html`, `{name}.css` (lowercase, no .component infix)
