# Domain: Routing

## Overview
Routing in this Angular application uses the **Angular Router** with a centralized route configuration and modern standalone component patterns. The router is configured using provider functions and integrates seamlessly with the standalone component architecture.

## Route Configuration

### Routes Definition
All routes are defined in a central `app.routes.ts` file.

**From `src/app/app.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [];
```

**Patterns:**
- Routes exported as a `Routes` array constant named `routes`
- File location: `src/app/app.routes.ts`
- Currently empty - ready for route definitions as features are added

### Future Route Structure
When routes are added, they will follow this pattern:

```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'connections', loadComponent: () => import('./features/connections/connections.component').then(m => m.ConnectionsComponent) },
  // ... more routes
];
```

**Expected Patterns:**
- Lazy-loaded components using `loadComponent`
- Dynamic imports for code splitting
- Standalone components (no modules needed)

## Router Integration

### Application Configuration
The router is configured in the application config using `provideRouter()`.

**From `src/app/app.config.ts`:**
```typescript
import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),  // Router configured here
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
```

**Patterns:**
- Router configured with `provideRouter(routes)` function
- Routes imported from `./app.routes`
- Part of the root `ApplicationConfig`
- No additional router options currently (can add features, debugging, etc.)

### Router Outlet
The application includes a `<router-outlet />` in the root component template.

**From `src/app/app.html`:**
```html
<!-- Main content -->
<main class="main">
  <!-- ... app content ... -->
</main>

<!-- Router outlet for routed components -->
<router-outlet />
```

**Patterns:**
- Router outlet placed at the end of the root template
- Self-closing tag syntax: `<router-outlet />`
- Routed components will render at this location

### Router Dependencies
The router outlet requires importing `RouterOutlet` in the component.

**From `src/app/app.ts`:**
```typescript
import { RouterOutlet } from '@angular/router';

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

**Patterns:**
- `RouterOutlet` imported from `@angular/router`
- Added to component's `imports` array
- Required for `<router-outlet />` to work

## Routing Conventions

### Path Naming
- Use **kebab-case** for route paths
- Examples: `dashboard`, `key-browser`, `server-info`

### Component Loading
- Use **lazy loading** with `loadComponent` for scalability
- Dynamic imports for code splitting
- Standalone components loaded directly (no modules)

### Route Organization
- All routes centralized in `app.routes.ts`
- Child routes can be defined inline or split into feature route files
- Route guards, resolvers defined alongside routes

## Summary

The routing domain follows these principles:

1. **Centralized configuration** in `app.routes.ts`
2. **Provider-based setup** using `provideRouter()`
3. **Router outlet** in root component template
4. **Lazy loading** pattern for routes (when added)
5. **Standalone components** throughout (no route modules)
6. **Modern Angular router** patterns
