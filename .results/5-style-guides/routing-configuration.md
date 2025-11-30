# Routing Configuration Style Guide

## Unique Conventions

This style guide covers **project-specific routing patterns**.

### 1. Centralized Route Definition

**Project convention:**
- All routes defined in `app.routes.ts`
- Export const named `routes` of type `Routes`

**From `src/app/app.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [];
```

**Pattern:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  // More routes...
];
```

### 2. Minimal Route File

**Unique to this project:**
- Extremely minimal route configuration file
- Currently empty array (placeholder)
- No additional exports or helper functions

**Standard pattern:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [];
```

**Not used in this project (but common elsewhere):**
```typescript
// ✗ No default export
export default routes;

// ✗ No additional configuration objects
export const routeConfig = { ... };

// ✗ No route constants
export const ROUTE_PATHS = { ... };
```

### 3. Standalone Component Routes

**Project requirement:**
- Routes reference standalone components directly
- No lazy loading modules (use lazy loading with standalone components if needed)

**Future pattern for this project:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'dashboard',
    component: Dashboard  // Direct standalone component reference
  },
  {
    path: 'settings',
    loadComponent: () => import('./settings/settings').then(m => m.Settings)  // Lazy load standalone component
  }
];
```

### 4. Integration with Application Config

**Project pattern:**
- Routes imported and provided in `app.config.ts`
- Uses `provideRouter` function

**From `src/app/app.config.ts`:**
```typescript
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    provideRouter(routes),
  ]
};
```

**This is the modern standalone routing approach** (no RouterModule).

### 5. Backend SPA Fallback Coordination

**Critical project constraint:**
- Backend has SPA fallback routing via `MapFallbackToFile("index.html")`
- API routes must be distinguishable from Angular routes

**From backend `Program.cs`:**
```csharp
app.MapControllers();  // API routes first
app.MapFallbackToFile("index.html");  // SPA fallback
```

**Implication for route design:**
- Avoid route names that conflict with controller names
- Convention: API routes use `[controller]` pattern (e.g., `/weatherforecast`)
- Angular routes can use any path not matching API routes

**Safe routing strategy:**
```typescript
export const routes: Routes = [
  // These won't conflict with /weatherforecast API endpoint
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'connections', component: ConnectionList },
  { path: 'data', component: DataBrowser },
];
```

## Key Takeaways

When adding routes to this project:

1. **Location**: All routes in `app.routes.ts`
2. **Export**: Named export `routes` of type `Routes`
3. **Components**: Direct standalone component references
4. **Lazy Loading**: Use `loadComponent` for code splitting
5. **Minimal**: No additional exports or helpers in route file
6. **API Conflicts**: Avoid route names matching controller names
7. **Provider**: Routes registered via `provideRouter()` in app.config

## Route Definition Patterns

### Basic Route
```typescript
{ path: 'dashboard', component: Dashboard }
```

### Route with Parameters
```typescript
{ path: 'connection/:id', component: ConnectionDetail }
```

### Lazy Loaded Route
```typescript
{
  path: 'admin',
  loadComponent: () => import('./admin/admin').then(m => m.Admin)
}
```

### Redirect
```typescript
{ path: '', redirectTo: '/dashboard', pathMatch: 'full' }
```

### 404 Route
```typescript
{ path: '**', component: NotFound }
```

### Nested Routes
```typescript
{
  path: 'settings',
  component: SettingsLayout,
  children: [
    { path: 'profile', component: ProfileSettings },
    { path: 'security', component: SecuritySettings }
  ]
}
```

## Complete Route File Example

**Future `app.routes.ts` structure:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  // Root redirect
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  
  // Main routes
  { path: 'dashboard', component: Dashboard },
  { path: 'connections', component: ConnectionList },
  { path: 'connection/:id', component: ConnectionDetail },
  
  // Lazy loaded routes
  {
    path: 'settings',
    loadComponent: () => import('./settings/settings').then(m => m.Settings)
  },
  
  // 404 catch-all
  { path: '**', component: NotFound }
];
```

## Route Guards (When Needed)

**Modern functional guard pattern:**
```typescript
import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';

const authGuard = () => {
  const authService = inject(AuthService);
  return authService.isAuthenticated();
};

export const routes: Routes = [
  {
    path: 'admin',
    component: AdminPanel,
    canActivate: [authGuard]  // Functional guard
  }
];
```

**Not used in this project (class-based guards):**
```typescript
// ✗ Older class-based guard approach
export class AuthGuard implements CanActivate {
  canActivate() { ... }
}
```
