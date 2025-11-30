# Style Guide: Routing Configuration

## Unique Patterns in This Codebase

### 1. Centralized Routes Export
**Project pattern:** All routes defined in a single `app.routes.ts` file.

**Example:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [];
```

**Convention:**
- **Single file**: All routes in `src/app/app.routes.ts`
- **Named export**: Export as `routes` constant
- **Typed**: Use `Routes` type from `@angular/router`
- **Empty array start**: Begin with empty array, add routes as needed

---

### 2. Expected Lazy Loading Pattern
**Future pattern:** When routes are added, use lazy-loaded standalone components.

**Example:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.Dashboard)
  },
  {
    path: 'connections',
    loadComponent: () => import('./features/connections/connections.component')
      .then(m => m.Connections)
  }
];
```

**Convention:**
- **loadComponent**: Use for lazy loading standalone components
- **Dynamic imports**: Use `import()` function for code splitting
- **Component class name**: Reference the actual class (e.g., `m.Dashboard`)
- **No loadChildren**: Standalone components don't need module loading

---

### 3. Route Path Naming
**Expected pattern:** Use kebab-case for all route paths.

**Convention:**
- **Lowercase**: All route paths in lowercase
- **Kebab-case**: Use hyphens for multi-word paths
- **Examples**: `dashboard`, `key-browser`, `connection-settings`
- **No slashes at start**: Paths don't start with `/` (except root redirect)

---

### 4. Root Redirect Pattern
**Expected pattern:** Empty path redirects to default route.

**Example:**
```typescript
{
  path: '',
  redirectTo: '/dashboard',
  pathMatch: 'full'
}
```

**Convention:**
- **Empty string path**: Use `''` for root
- **Full path match**: Always use `pathMatch: 'full'` for redirects
- **Leading slash in redirectTo**: Include `/` in redirect target

---

### 5. No Feature Modules
**Important:** This project uses standalone components, no feature modules.

**DON'T:**
```typescript
// ❌ Don't do this - no modules!
{
  path: 'admin',
  loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
}
```

**DO:**
```typescript
// ✓ Correct - load standalone component
{
  path: 'admin',
  loadComponent: () => import('./features/admin/admin.component').then(m => m.Admin)
}
```

**Convention:**
- **loadComponent only**: Never use `loadChildren` for modules
- **Standalone components**: All routed components are standalone
- **No NgModules**: This is a module-free architecture

---

## Summary Checklist

When working with routes in this codebase:

- [ ] All routes defined in `src/app/app.routes.ts`
- [ ] Export routes array as `routes` constant
- [ ] Use `Routes` type annotation
- [ ] Use `loadComponent` for lazy loading
- [ ] Use dynamic `import()` for code splitting
- [ ] Reference component class name from module
- [ ] Use kebab-case for route paths
- [ ] Include root redirect with `pathMatch: 'full'`
- [ ] Never use `loadChildren` or feature modules
- [ ] All routed components are standalone
