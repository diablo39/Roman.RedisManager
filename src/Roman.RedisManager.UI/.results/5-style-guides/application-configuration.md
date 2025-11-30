# Style Guide: Application Configuration

## Unique Patterns in This Codebase

### 1. Centralized Provider Configuration
**Project pattern:** All application providers in a single `app.config.ts` file.

**Example:**
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
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
```

**Convention:**
- **Single config file**: `src/app/app.config.ts`
- **Named export**: Export as `appConfig`
- **ApplicationConfig type**: Type the config object
- **Provider functions only**: Use `provide*()` functions, no class providers directly

---

### 2. Provider Ordering
**Specific pattern:** Providers are ordered logically by dependency and purpose.

**Order:**
1. **Error handling**: `provideBrowserGlobalErrorListeners()`
2. **Change detection**: `provideZoneChangeDetection()`
3. **Routing**: `provideRouter()`
4. **Animations**: `provideAnimationsAsync()`
5. **UI framework**: `providePrimeNG()`

**Convention:**
- **Core first**: Angular core providers at the top
- **Framework second**: UI framework providers after core
- **Features last**: Feature-specific providers at the end

---

### 3. Zone Change Detection Configuration
**Performance pattern:** Event coalescing enabled for better performance.

**Example:**
```typescript
provideZoneChangeDetection({ eventCoalescing: true })
```

**Convention:**
- **Event coalescing**: Always enable with `{ eventCoalescing: true }`
- **Performance optimization**: Batches multiple events for single change detection cycle
- **No other options**: Keep it simple, don't add unnecessary configuration

---

### 4. Async Animations Provider
**Bundle optimization:** Animations loaded asynchronously.

**Example:**
```typescript
provideAnimationsAsync()
```

**Convention:**
- **Use async variant**: `provideAnimationsAsync()` not `provideAnimations()`
- **Better initial load**: Reduces main bundle size
- **Required for PrimeNG**: Many PrimeNG components need animations

---

### 5. PrimeNG Theme Configuration
**UI framework pattern:** PrimeNG configured with Aura preset theme.

**Example:**
```typescript
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

// In providers array:
providePrimeNG({
  theme: {
    preset: Aura
  }
})
```

**Convention:**
- **Aura preset**: Use Aura theme from `@primeuix/themes/aura`
- **Object configuration**: Pass config object to `providePrimeNG()`
- **Global theme**: Applied to all PrimeNG components
- **No custom theme**: Stick with Aura unless specific need

---

### 6. Router Configuration Import
**Dependency pattern:** Routes imported from separate file.

**Example:**
```typescript
import { routes } from './app.routes';

// In providers:
provideRouter(routes)
```

**Convention:**
- **Import from routes file**: Don't define routes inline
- **Relative import**: Use `'./app.routes'`
- **No router options**: Keep simple unless specific needs (debugging, tracing, etc.)

---

### 7. Global Error Listeners
**Error handling pattern:** Global error listeners enabled.

**Example:**
```typescript
provideBrowserGlobalErrorListeners()
```

**Convention:**
- **Always include**: Catches uncaught errors globally
- **First in list**: Place at top of providers array
- **No configuration**: Use default settings

---

### 8. Adding New Providers
**Extension pattern:** When adding new providers, follow the established patterns.

**Example:**
```typescript
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),  // New provider
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
```

**Convention:**
- **Use provider functions**: Modern `provide*()` syntax
- **Logical placement**: Insert in appropriate position (HTTP typically after router)
- **Feature helpers**: Use `with*()` helper functions for options
- **Import at top**: Add imports at top of file

---

## Summary Checklist

When working with application configuration:

- [ ] All providers in `src/app/app.config.ts`
- [ ] Export as `appConfig` with `ApplicationConfig` type
- [ ] Use `provideBrowserGlobalErrorListeners()` first
- [ ] Include `provideZoneChangeDetection({ eventCoalescing: true })`
- [ ] Use `provideRouter(routes)` with imported routes
- [ ] Use `provideAnimationsAsync()` (not sync version)
- [ ] Configure PrimeNG with Aura theme preset
- [ ] Order providers logically (core → framework → features)
- [ ] Use provider functions, not class providers
- [ ] Import dependencies from separate files
