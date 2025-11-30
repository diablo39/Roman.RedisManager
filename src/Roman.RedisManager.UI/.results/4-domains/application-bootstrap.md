# Domain: Application Bootstrap

## Overview
This Angular application uses modern **standalone component bootstrapping** with function-based providers. The bootstrap process is centralized in `main.ts` and configuration is managed through `app.config.ts`.

## Bootstrap Process

### Main Entry Point
The application bootstraps from `src/main.ts` using the `bootstrapApplication()` function.

**From `src/main.ts`:**
```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
```

**Patterns:**
- **Function-based bootstrap**: Uses `bootstrapApplication()` instead of NgModule-based `platformBrowserDynamic()`
- **Standalone root component**: `App` component is standalone (no AppModule)
- **Configuration object**: `appConfig` contains all providers
- **Error handling**: Bootstrap errors logged to console

### Application Configuration
All application-level providers are configured in `src/app/app.config.ts`.

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

**Patterns:**
- **ApplicationConfig type**: Exported config object typed as `ApplicationConfig`
- **Provider functions**: All configuration uses provider functions (`provide*()`)
- **Centralized providers**: All app-level dependencies configured in one place

## Provider Configuration

### Core Angular Providers

#### Browser Global Error Listeners
```typescript
provideBrowserGlobalErrorListeners()
```
- Enables global error handling
- Catches uncaught errors in the application

#### Zone Change Detection
```typescript
provideZoneChangeDetection({ eventCoalescing: true })
```
- Configures Zone.js for change detection
- `eventCoalescing: true` optimizes performance by batching events

#### Router
```typescript
provideRouter(routes)
```
- Configures Angular Router
- Imports routes from `./app.routes`

#### Animations
```typescript
provideAnimationsAsync()
```
- Enables Angular animations
- Async loading for better initial bundle size
- Required for PrimeNG animations

### Third-Party Providers

#### PrimeNG
```typescript
providePrimeNG({
  theme: {
    preset: Aura
  }
})
```
- Configures PrimeNG with Aura theme preset
- Global configuration for all PrimeNG components
- Imported from `primeng/config`

## File Structure

### Bootstrap Files
1. **`src/main.ts`**: Application entry point
   - Imports root component and config
   - Calls `bootstrapApplication()`
   - Error handling

2. **`src/app/app.config.ts`**: Application configuration
   - Exports `ApplicationConfig` object
   - Contains all provider configurations
   - Imports routes and other configs

3. **`src/app/app.ts`**: Root component
   - Standalone root component
   - Named `App` (not `AppComponent`)
   - Imports `RouterOutlet` and other root-level dependencies

## Configuration Patterns

### Provider Order
Providers are added in a logical order:
1. **Error handling** (provideBrowserGlobalErrorListeners)
2. **Change detection** (provideZoneChangeDetection)
3. **Routing** (provideRouter)
4. **Animations** (provideAnimationsAsync)
5. **UI framework** (providePrimeNG)

### Adding New Providers
To add new application-level providers, add them to the `providers` array in `app.config.ts`:

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    // ... existing providers
    provideHttpClient(),  // Example: add HTTP client
    // ... more providers
  ]
};
```

## HTML Entry Point

### Index.html
The HTML entry point is minimal and references the root component selector.

**From `src/index.html`:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>RomanRedisManagerUi</title>
  <base href="/" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link rel="icon" type="image/x-icon" href="favicon.ico" />
</head>
<body>
  <app-root></app-root>
</body>
</html>
```

**Patterns:**
- Root selector matches component: `<app-root></app-root>`
- Minimal HTML structure
- Base href for routing
- Viewport meta for responsive design

## Summary

The application bootstrap domain follows these principles:

1. **Standalone bootstrapping** using `bootstrapApplication()`
2. **Function-based providers** for all configuration
3. **Centralized config** in `app.config.ts`
4. **No NgModules** - fully standalone architecture
5. **Logical provider ordering** for dependencies
6. **Modern Angular 20** approach throughout
