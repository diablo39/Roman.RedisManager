# Angular Components Style Guide

## Unique Conventions

This style guide focuses on the **project-specific patterns** that distinguish this codebase from standard Angular practices.

### 1. File Naming Pattern (No .component Suffix)

**Unique to this project:**
- Component files omit the `.component` infix
- Pattern: `{name}.ts`, `{name}.html`, `{name}.css`

**Example:**
```
✓ app.ts, app.html, app.css
✗ app.component.ts, app.component.html, app.component.css
```

**From `src/app/app.ts`:**
```typescript
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],
  templateUrl: './app.html',    // No .component in filename
  styleUrl: './app.css'          // No .component in filename
})
export class App {  // Class name without "Component" suffix
  // ...
}
```

### 2. Class Naming (No Component Suffix)

**Unique to this project:**
- Component class names don't include "Component" suffix
- Use concise, descriptive names

**Example:**
```typescript
✓ export class App { }
✓ export class Dashboard { }
✓ export class ConnectionList { }

✗ export class AppComponent { }
✗ export class DashboardComponent { }
```

### 3. Standalone Component Pattern with Explicit Imports

**Project convention:**
- All components are standalone (no NgModules)
- Import dependencies directly in the `imports` array
- No `standalone: true` declaration (implied by imports array)

**Example from `src/app/app.ts`:**
```typescript
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule],  // Explicit imports array
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('roman-redis-manager-ui');
}
```

### 4. Signal-Based State Management

**Project-specific pattern:**
- Use Angular signals for reactive state
- Signals typically marked as `readonly` for immutable references
- Use `protected` access modifier for template-only properties

**Example:**
```typescript
export class App {
  // Signal with protected access for template-only use
  protected readonly title = signal('roman-redis-manager-ui');
  
  // Signal with private access for internal logic
  private readonly isLoading = signal(false);
  
  // Public signal for parent component access
  readonly count = signal(0);
}
```

**In templates:**
```html
<h1>{{ title() }}</h1>  <!-- Function call syntax for signals -->
```

### 5. Separate Template and Style Files

**Project convention:**
- Always use external template files (`templateUrl`)
- Always use external style files (`styleUrl`)
- Colocate template and style files with component

**Example:**
```typescript
@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, ButtonModule],
  templateUrl: './dashboard.html',  // External template
  styleUrl: './dashboard.css'        // External styles
})
```

**Not used in this project:**
```typescript
// ✗ Inline templates
template: `<div>...</div>`

// ✗ Inline styles
styles: [`h1 { color: blue; }`]
```

### 6. PrimeNG Component Integration

**Project-specific requirement:**
- Import PrimeNG modules at component level (not app-wide)
- Import specific component modules as needed

**Example:**
```typescript
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-data-grid',
  imports: [ButtonModule, InputTextModule, TableModule],
  // ...
})
```

### 7. TypeScript Access Modifiers

**Project pattern:**
- `protected` for properties used only in templates
- `private` for internal logic
- `public` (or no modifier) for API surface

**Example:**
```typescript
export class UserProfile {
  // Template-accessible properties
  protected readonly userName = signal('');
  protected readonly avatarUrl = signal('');
  
  // Internal state
  private readonly loadingState = signal(false);
  
  // Public API
  readonly userId = signal(0);
  
  updateProfile(name: string) {
    this.userName.set(name);
  }
}
```

### 8. Router Integration

**Project convention:**
- Import `RouterOutlet` in components that render routes
- Use self-closing tag syntax in templates

**Example:**
```typescript
import { RouterOutlet } from '@angular/router';

@Component({
  imports: [RouterOutlet],
  templateUrl: './app.html'
})
```

**In template:**
```html
<router-outlet />  <!-- Self-closing tag -->
```

## Key Takeaways

When creating a new component in this project:

1. **Naming**: `{name}.ts`, not `{name}.component.ts`
2. **Class**: `export class {Name}`, not `export class {Name}Component`
3. **Standalone**: Always use standalone with imports array
4. **State**: Use signals with appropriate access modifiers
5. **Files**: Separate .ts, .html, .css files
6. **PrimeNG**: Import component modules as needed
7. **Templates**: External files only (no inline)
8. **Router**: Import RouterOutlet when needed
