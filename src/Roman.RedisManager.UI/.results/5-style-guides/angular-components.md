# Style Guide: Angular Components

## Unique Patterns in This Codebase

### 1. Class Naming Without "Component" Suffix
**Unique to this project:** Component classes are named without the "Component" suffix.

**Example:**
```typescript
export class App {  // Not "AppComponent"
  protected readonly title = signal('roman-redis-manager-ui');
}
```

**Convention:**
- Use simple, descriptive class names (e.g., `App`, `Dashboard`, `KeyBrowser`)
- Omit the "Component" suffix entirely
- This differs from many Angular projects that use `AppComponent`, `DashboardComponent`, etc.

---

### 2. Signal-Based State with Protected Access
**Unique pattern:** All component state uses signals with protected or private visibility.

**Example:**
```typescript
export class App {
  protected readonly title = signal('roman-redis-manager-ui');
}
```

**Convention:**
- **Signals for all state**: Use `signal()` instead of plain properties
- **Protected access**: Make signals `protected` for use in templates
- **Readonly references**: Mark signal references as `readonly` when the reference won't change
- **Private for internal state**: Use `private` for purely internal state

---

### 3. Standalone Components with Explicit Imports
**Standard in Angular 20 but critical here:** All components must explicitly declare their dependencies.

**Example:**
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
  // ...
}
```

**Convention:**
- **Explicit imports array**: List all template dependencies
- **PrimeNG modules**: Import specific modules (e.g., `ButtonModule`)
- **Router dependencies**: Include `RouterOutlet` if using `<router-outlet />`
- **No standalone property**: Implicit in Angular 20

---

### 4. Three-File Component Structure
**Unique organizational pattern:** Every component consists of exactly three files.

**Files:**
1. `{name}.ts` - Component logic
2. `{name}.html` - Template
3. `{name}.css` - Styles

**Example:**
```
app/
  app.ts
  app.html
  app.css
  app.spec.ts  (test file)
```

**Convention:**
- **No inline templates**: Always use `templateUrl`
- **No inline styles**: Always use `styleUrl`
- **Colocated files**: All files in the same directory
- **Consistent naming**: Use same base name for all three files

---

### 5. PrimeNG-Only UI Components
**Project-specific constraint:** All UI components must come from PrimeNG.

**Example:**
```typescript
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  imports: [ButtonModule, TableModule, InputTextModule],
  // ...
})
```

**Convention:**
- **Import PrimeNG modules**: Each component type has its own module
- **No mixing frameworks**: Don't use Angular Material, Bootstrap, etc.
- **PrimeIcons for icons**: Use `pi pi-*` classes
- **Follow PrimeNG patterns**: Use PrimeNG's API conventions

---

### 6. Root Component Selector Pattern
**Specific naming:** Root component uses `app-root` selector.

**Example:**
```typescript
@Component({
  selector: 'app-root',
  // ...
})
export class App {
  // ...
}
```

**Convention:**
- **Root selector**: Always `app-root` for the main component
- **Kebab-case**: Use kebab-case for all selectors
- **Prefix**: Use `app-` prefix for all custom components

---

## Summary Checklist

When creating a new component in this codebase:

- [ ] Class name without "Component" suffix
- [ ] Use signals for component state
- [ ] Make signals `protected readonly`
- [ ] Create three files: `.ts`, `.html`, `.css`
- [ ] Use `templateUrl` and `styleUrl` (never inline)
- [ ] Import all template dependencies explicitly
- [ ] Use only PrimeNG for UI components
- [ ] Follow `app-` prefix convention for selectors
- [ ] Include unit test file (`.spec.ts`)
