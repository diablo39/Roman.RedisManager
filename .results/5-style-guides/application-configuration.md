# Application Configuration Style Guide

## Unique Conventions

This style guide covers **project-specific application configuration patterns**.

### 1. Centralized Provider Configuration

**Project convention:**
- All application providers in single `app.config.ts` file
- Export const named `appConfig` of type `ApplicationConfig`

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

### 2. Global Error Listeners

**Unique to this project:**
- Explicit global error listener registration
- First provider in the array

**Pattern:**
```typescript
provideBrowserGlobalErrorListeners()
```

**Purpose:**
- Captures unhandled errors and promise rejections
- Improves debugging and error tracking
- Not commonly seen in basic Angular projects

### 3. Zone.js with Event Coalescing

**Project-specific optimization:**
- Zone.js configured with event coalescing enabled
- Performance optimization from the start

**Pattern:**
```typescript
provideZoneChangeDetection({ eventCoalescing: true })
```

**Benefits:**
- Batches multiple events into single change detection cycle
- Reduces unnecessary change detection runs
- Better performance for event-heavy applications

**Not used:**
```typescript
// ✗ Default zone.js (no optimization)
provideZoneChangeDetection()
```

### 4. Async Animations Provider

**Project convention:**
- Animations provided asynchronously for better initial load

**Pattern:**
```typescript
provideAnimationsAsync()
```

**Benefit:**
- Lazy loads animation module
- Smaller initial bundle size
- Animations only loaded when needed

**Not used:**
```typescript
// ✗ Synchronous animations (larger initial bundle)
import { provideAnimations } from '@angular/platform-browser/animations';
provideAnimations()
```

### 5. PrimeNG Configuration with Theme

**Unique project requirement:**
- PrimeNG configured at application level
- Aura theme preset specifically chosen
- Theme configuration object pattern

**Pattern:**
```typescript
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

providePrimeNG({
  theme: {
    preset: Aura
  }
})
```

**Theme is central to this project** - all PrimeNG components will use Aura styling.

### 6. Functional Provider Pattern

**Project approach:**
- All providers are functions (modern Angular pattern)
- No class-based providers in app config

**Example pattern:**
```typescript
providers: [
  provideRouter(routes),              // Function provider
  provideHttpClient(),                // Function provider
  provideAnimationsAsync(),           // Function provider
  // NOT: new RouterModule.forRoot()  // Old module pattern
]
```

### 7. Provider Ordering

**Implicit convention in this project:**
```typescript
providers: [
  // 1. Error handling first
  provideBrowserGlobalErrorListeners(),
  
  // 2. Core Angular features
  provideZoneChangeDetection({ eventCoalescing: true }),
  
  // 3. Routing
  provideRouter(routes),
  
  // 4. UI enhancements
  provideAnimationsAsync(),
  
  // 5. Third-party libraries
  providePrimeNG({ theme: { preset: Aura } })
]
```

**This ordering ensures proper initialization sequence.**

## Key Takeaways

When modifying application configuration:

1. **Location**: All providers in `app.config.ts`
2. **Export**: Named export `appConfig` of type `ApplicationConfig`
3. **Pattern**: Functional providers only
4. **Error Handling**: Global error listeners included
5. **Performance**: Event coalescing enabled, async animations
6. **Theme**: PrimeNG Aura theme configured globally
7. **Ordering**: Logical provider sequence (errors, core, routing, UI, libraries)

## Adding New Providers

### HTTP Client
```typescript
import { provideHttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... existing providers
    provideHttpClient(),
  ]
};
```

### HTTP Interceptors
```typescript
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
  ]
};
```

### Custom Services
```typescript
import { MyService } from './services/my.service';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... existing providers
    MyService,  // or { provide: MyService, useClass: MyService }
  ]
};
```

### Environment-Specific Providers
```typescript
import { environment } from '../environments/environment';

const devProviders = environment.production ? [] : [
  // Development-only providers
];

export const appConfig: ApplicationConfig = {
  providers: [
    // ... existing providers
    ...devProviders
  ]
};
```

## Complete Configuration Example

**Expanded `app.config.ts` with additional providers:**
```typescript
import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    // Error handling
    provideBrowserGlobalErrorListeners(),
    
    // Core Angular
    provideZoneChangeDetection({ eventCoalescing: true }),
    
    // Routing
    provideRouter(routes),
    
    // HTTP
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    
    // Animations
    provideAnimationsAsync(),
    
    // PrimeNG
    providePrimeNG({
      theme: {
        preset: Aura
      }
    }),
    
    // Application services (if needed)
    // MyService
  ]
};
```

## Integration with Bootstrap

**The config is used in `main.ts`:**
```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
```

**This is the modern standalone bootstrap pattern** - no app.module.ts needed.
