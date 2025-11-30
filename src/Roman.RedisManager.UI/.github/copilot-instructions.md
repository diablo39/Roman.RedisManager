# GitHub Copilot Instructions for Roman Redis Manager UI

## Purpose of This Document

This file provides comprehensive instructions to AI coding assistants (like GitHub Copilot, Cursor, and others) to generate features and code that align with the established architecture, patterns, and conventions of the **Roman Redis Manager UI** project.

**Key Points:**
- These instructions are based on **actual, observed patterns** from the codebase - not invented practices
- Following these guidelines ensures consistency with the existing architecture
- All code examples are taken from real project files

---

## Project Overview

### Tech Stack Summary

**Core Framework:** Angular 20.3.0
- Standalone components (no NgModules)
- Signal-based reactivity
- Modern Angular patterns (@for, @if, etc.)
- TypeScript 5.9.2 with strict mode

**UI Library:** PrimeNG 20.3.0
- Aura theme preset
- PrimeIcons for iconography
- PrimeFlex for CSS utilities
- Exclusive UI framework (no mixing with other libraries)

**State Management:** Angular Signals
- Built-in signal() API for reactive state
- No external state management library

**Testing:** Jasmine 5.9.0 + Karma 6.4.0

**Build:** Angular CLI 20.3.12 with esbuild application builder

### Domain: Redis Database Management

This is a **management UI for Redis databases** providing:
- Connection management to Redis servers
- Key-value CRUD operations
- Data structure visualization (strings, lists, sets, hashes, sorted sets, streams)
- Real-time monitoring and metrics
- Command execution interface

**Domain Boundaries:**
- ✅ Frontend-only Redis management features
- ✅ Client-side operations and visualization
- ❌ Backend server implementation
- ❌ Non-Redis database support

---

## File Categories Reference

### Angular Components
**Location:** `src/app/**/*.ts`

**Conventions:**
- Class name WITHOUT "Component" suffix (e.g., `App`, not `AppComponent`)
- Protected readonly signals for state
- Standalone with explicit imports array
- Three-file structure: `.ts`, `.html`, `.css`

**Example:**
```typescript
import { Component, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-dashboard',
  imports: [ButtonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  protected readonly title = signal('Redis Dashboard');
}
```

### Component Templates
**Location:** `src/app/**/*.html`

**Conventions:**
- Separate .html files (never inline)
- Signal interpolation with `{{ signal() }}` syntax
- PrimeNG components with `p-` prefix
- Modern control flow (@for, @if, @switch)
- Self-closing tags for empty elements

**Example:**
```html
<div class="dashboard">
  <h1>{{ title() }}</h1>
  <p-button label="Connect" icon="pi pi-check" />
  
  @for (server of servers(); track server.id) {
    <div class="server-card">{{ server.name }}</div>
  }
</div>
```

### Component Styles
**Location:** `src/app/**/*.css`

**Conventions:**
- Separate .css files (never inline)
- OKLCH color space for colors
- CSS custom properties for theming
- :host selector for component root
- Scoped to component

**Example:**
```css
:host {
  display: block;
  --primary-color: oklch(51.01% 0.274 263.83);
}

.dashboard {
  background-color: var(--primary-color);
}
```

### Routing Configuration
**Location:** `src/app/app.routes.ts`

**Conventions:**
- All routes in single file
- Lazy loading with loadComponent
- Kebab-case paths
- No feature modules (standalone components only)

**Example:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.Dashboard)
  }
];
```

### Application Configuration
**Location:** `src/app/app.config.ts`

**Conventions:**
- All providers in ApplicationConfig
- Provider functions (provide*)
- Specific ordering: error handling → change detection → routing → animations → UI framework

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
      theme: { preset: Aura }
    })
  ]
};
```

### Unit Tests
**Location:** `src/app/**/*.spec.ts`

**Conventions:**
- Colocated with source files
- Standalone component imports (not declarations)
- Signal testing with function call syntax
- TestBed.createComponent() pattern

**Example:**
```typescript
import { TestBed } from '@angular/core/testing';
import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Dashboard);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should have correct title', () => {
    const fixture = TestBed.createComponent(Dashboard);
    expect(fixture.componentInstance.title()).toEqual('Redis Dashboard');
  });
});
```

---

## Feature Scaffold Guide

### When Adding a New Feature

Follow this process to ensure consistency:

#### 1. Determine File Categories Needed

**For a typical feature component:**
- Component logic file (`.ts`)
- Component template (`.html`)
- Component styles (`.css`)
- Unit test (`.spec.ts`)
- Route definition (in `app.routes.ts`)

**Example:** Adding a "Connection Manager" feature:
```
src/app/features/connections/
  ├── connections.ts          # Component logic
  ├── connections.html        # Template
  ├── connections.css         # Styles
  └── connections.spec.ts     # Tests
```

#### 2. Component File Structure

**Component Logic (`connections.ts`):**
```typescript
import { Component, signal } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-connections',
  imports: [TableModule, ButtonModule],
  templateUrl: './connections.html',
  styleUrl: './connections.css'
})
export class Connections {
  protected readonly connections = signal<Connection[]>([]);
  
  protected addConnection(): void {
    // Logic here
  }
}

interface Connection {
  id: string;
  name: string;
  host: string;
  port: number;
}
```

**Key Points:**
- Class name: `Connections` (no "Component" suffix)
- Signals with `protected readonly`
- PrimeNG module imports
- Separate template and style files
- Interface definitions for domain models

**Template (`connections.html`):**
```html
<div class="connections-container">
  <div class="header">
    <h1>Redis Connections</h1>
    <p-button label="New Connection" icon="pi pi-plus" (onClick)="addConnection()" />
  </div>

  <p-table [value]="connections()">
    <ng-template #header>
      <tr>
        <th>Name</th>
        <th>Host</th>
        <th>Port</th>
      </tr>
    </ng-template>
    <ng-template #body let-conn>
      <tr>
        <td>{{ conn.name }}</td>
        <td>{{ conn.host }}</td>
        <td>{{ conn.port }}</td>
      </tr>
    </ng-template>
  </p-table>
</div>
```

**Styles (`connections.css`):**
```css
:host {
  display: block;
  padding: 2rem;
}

.connections-container {
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}
```

**Test (`connections.spec.ts`):**
```typescript
import { TestBed } from '@angular/core/testing';
import { Connections } from './connections';

describe('Connections', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Connections],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Connections);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should have empty connections initially', () => {
    const fixture = TestBed.createComponent(Connections);
    expect(fixture.componentInstance.connections()).toEqual([]);
  });
});
```

#### 3. Add Route

**In `src/app/app.routes.ts`:**
```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'connections',
    loadComponent: () => import('./features/connections/connections.component')
      .then(m => m.Connections)
  }
];
```

---

## Integration Rules

### UI Domain Rules

**Required Patterns:**
1. **All components must be standalone** with explicit imports array
2. **Use signals for state** - `protected readonly signal()`
3. **PrimeNG for all UI elements** - no other UI libraries
4. **Three-file structure** - separate .ts, .html, .css files

**Constraints:**
- Never use inline templates or styles
- Never use "Component" suffix in class names
- Never mix UI frameworks (only PrimeNG)
- Never use class properties for reactive state (use signals)

### Routing Domain Rules

**Required Patterns:**
1. **Centralized route configuration** in `app.routes.ts`
2. **Lazy loading** with `loadComponent`
3. **No feature modules** - standalone components only

**Constraints:**
- Never use `loadChildren` for modules
- Never create NgModules
- Always use kebab-case for paths

### Application Bootstrap Rules

**Required Patterns:**
1. **All providers in `app.config.ts`**
2. **Use provider functions** - `provide*()` syntax
3. **Specific provider order** - error handling → change detection → routing → animations → UI

**Constraints:**
- Never use NgModule-based bootstrapping
- Never inline provider configuration in main.ts
- Always include PrimeNG Aura theme configuration

### Theming and Styling Rules

**Required Patterns:**
1. **PrimeNG Aura theme** globally configured
2. **OKLCH color space** for all colors
3. **CSS custom properties** for themeable values
4. **Component-scoped styles** in separate files

**Constraints:**
- Never override PrimeNG theme unless absolutely necessary
- Never use RGB/HSL for new colors (use OKLCH)
- Never add inline styles in templates
- Minimize global styles (use `styles.css` sparingly)

### Testing Rules

**Required Patterns:**
1. **Colocated `.spec.ts` files** with source
2. **Import standalone components** in TestBed
3. **Call signals as functions** in tests
4. **Include "should create" test** as first test

**Constraints:**
- Never use `declarations` in TestBed (use `imports`)
- Never forget `detectChanges()` for DOM tests
- Always use `async/await` with `compileComponents()`

---

## Example Feature Prompt Usage

### User Request:
"Create a key browser component that shows a list of Redis keys with search functionality"

### AI Should Generate:

**1. Component File (`src/app/features/key-browser/key-browser.ts`):**
```typescript
import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-key-browser',
  imports: [FormsModule, TableModule, InputTextModule, ButtonModule],
  templateUrl: './key-browser.html',
  styleUrl: './key-browser.css'
})
export class KeyBrowser {
  protected readonly allKeys = signal<RedisKey[]>([]);
  protected readonly searchTerm = signal('');
  
  protected readonly filteredKeys = computed(() => {
    const term = this.searchTerm().toLowerCase();
    return this.allKeys().filter(key => 
      key.name.toLowerCase().includes(term)
    );
  });
  
  protected refreshKeys(): void {
    // Implementation
  }
}

interface RedisKey {
  name: string;
  type: string;
  ttl: number;
}
```

**2. Template File (`src/app/features/key-browser/key-browser.html`):**
```html
<div class="key-browser">
  <div class="toolbar">
    <span class="p-input-icon-left">
      <i class="pi pi-search"></i>
      <input 
        type="text" 
        pInputText 
        [(ngModel)]="searchTerm" 
        placeholder="Search keys..."
      />
    </span>
    <p-button 
      label="Refresh" 
      icon="pi pi-refresh" 
      (onClick)="refreshKeys()"
    />
  </div>

  <p-table [value]="filteredKeys()">
    <ng-template #header>
      <tr>
        <th>Key Name</th>
        <th>Type</th>
        <th>TTL</th>
      </tr>
    </ng-template>
    <ng-template #body let-key>
      <tr>
        <td>{{ key.name }}</td>
        <td>{{ key.type }}</td>
        <td>{{ key.ttl }}</td>
      </tr>
    </ng-template>
  </p-table>
</div>
```

**3. Styles File (`src/app/features/key-browser/key-browser.css`):**
```css
:host {
  display: block;
  padding: 1.5rem;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.p-input-icon-left {
  width: 300px;
}
```

**4. Test File (`src/app/features/key-browser/key-browser.spec.ts`):**
```typescript
import { TestBed } from '@angular/core/testing';
import { KeyBrowser } from './key-browser';

describe('KeyBrowser', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KeyBrowser],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(KeyBrowser);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should filter keys based on search term', () => {
    const fixture = TestBed.createComponent(KeyBrowser);
    const component = fixture.componentInstance;
    
    component.allKeys.set([
      { name: 'user:1', type: 'string', ttl: -1 },
      { name: 'session:abc', type: 'hash', ttl: 3600 }
    ]);
    component.searchTerm.set('user');
    
    expect(component.filteredKeys().length).toBe(1);
    expect(component.filteredKeys()[0].name).toBe('user:1');
  });
});
```

**5. Route Addition (in `src/app/app.routes.ts`):**
```typescript
{
  path: 'key-browser',
  loadComponent: () => import('./features/key-browser/key-browser.component')
    .then(m => m.KeyBrowser)
}
```

---

## Domain-Specific Guidance

### Redis-Specific Patterns

When working with Redis features, follow these domain conventions:

**Data Model Naming:**
- Redis connections: `Connection`, `RedisConnection`
- Redis keys: `RedisKey`, `Key`
- Redis commands: `Command`, `RedisCommand`
- Server info: `ServerInfo`, `RedisServerInfo`

**Common Redis Operations:**
- Connection management: `connect()`, `disconnect()`, `testConnection()`
- Key operations: `getKey()`, `setKey()`, `deleteKey()`, `searchKeys()`
- Data type operations: `getString()`, `getHash()`, `getList()`, etc.
- Server operations: `getInfo()`, `getStats()`, `flushDb()`

**Redis Data Types:**
```typescript
type RedisDataType = 'string' | 'list' | 'set' | 'zset' | 'hash' | 'stream';

interface RedisKey {
  name: string;
  type: RedisDataType;
  ttl: number;  // -1 for no expiration
  size?: number;
}

interface RedisConnection {
  id: string;
  name: string;
  host: string;
  port: number;
  password?: string;
  database: number;  // 0-15
  ssl: boolean;
}
```

---

## Summary

When building new features for this project:

1. **Use Angular 20 standalone components** with signals
2. **Use PrimeNG exclusively** for UI components
3. **Follow the three-file structure** (.ts, .html, .css)
4. **Class names without "Component" suffix**
5. **Signals with `protected readonly`** for state
6. **Separate route configuration** with lazy loading
7. **OKLCH colors** and CSS custom properties
8. **Colocated tests** with proper TestBed setup
9. **Redis domain models** and naming conventions
10. **Modern Angular patterns** (@for, @if, etc.)

This ensures all generated code is consistent with the existing codebase architecture and conventions.
