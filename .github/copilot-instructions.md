# GitHub Copilot Instructions for Roman.RedisManager

## Overview

This file provides AI coding assistants with comprehensive guidance for generating code that aligns with the Roman.RedisManager project's architecture, conventions, and style. All patterns and conventions described here are derived from actual code analysis—nothing is invented or assumed.

**Purpose:**
- Enable consistent code generation across the project
- Document established patterns and architectural decisions
- Guide feature implementation aligned with existing conventions
- Prevent inconsistent or non-compliant code additions

**Project Type:** Full-stack web application for Redis database management
- **Frontend:** Angular 20.3.0 standalone components with PrimeNG
- **Backend:** ASP.NET Core 10.0 Web API
- **Architecture:** SPA with integrated build pipeline

---

## File Category Reference

### Angular Components
**Location:** `src/Roman.RedisManager.UI/src/app/`  
**Examples:** `app.ts`, `app.html`, `app.css`

**Key Conventions:**
- **No `.component` suffix** in filenames (use `{name}.ts`, not `{name}.component.ts`)
- **No "Component" suffix** in class names (use `App`, not `AppComponent`)
- **Standalone components** with explicit `imports` array
- **Signal-based state** using `signal()` for reactive values
- **Separate files** for template (.html), styles (.css), and logic (.ts)
- **PrimeNG integration** via component-level imports

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
  protected readonly title = signal('Dashboard');
}
```

### Component Templates
**Location:** `src/Roman.RedisManager.UI/src/app/`  
**Examples:** `app.html`

**Key Conventions:**
- **Signal function syntax**: `{{ signalName() }}` for accessing signal values
- **Self-closing router outlet**: `<router-outlet />`
- **CSS custom properties** in `<style>` blocks using OKLCH color space
- **Semantic HTML5** elements (`<main>`, `<section>`, etc.)

**Example:**
```html
<style>
  :host {
    --primary-color: oklch(51.01% 0.274 263.83);
  }
</style>

<main class="dashboard">
  <h1>{{ title() }}</h1>
</main>

<router-outlet />
```

### Component Styles
**Location:** `src/Roman.RedisManager.UI/src/app/`  
**Examples:** `app.css`

**Key Conventions:**
- Often **minimal or empty** (styles in template or via PrimeNG)
- Use **CSS custom properties** for themeable values
- Leverage **PrimeFlex utilities** before writing custom CSS
- **ViewEncapsulation.Emulated** by default (scoped styles)

**Example:**
```css
:host {
  display: block;
  padding: 1rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}
```

### Routing Configuration
**Location:** `src/Roman.RedisManager.UI/src/app/app.routes.ts`  
**Example:** `app.routes.ts`

**Key Conventions:**
- Export const named **`routes`** of type `Routes`
- **Standalone component references** (no modules)
- Use **`loadComponent`** for lazy loading
- Avoid conflicts with backend API routes

**Example:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  {
    path: 'settings',
    loadComponent: () => import('./settings/settings').then(m => m.Settings)
  },
  { path: '**', component: NotFound }
];
```

### Application Configuration
**Location:** `src/Roman.RedisManager.UI/src/app/app.config.ts`  
**Example:** `app.config.ts`

**Key Conventions:**
- Export const named **`appConfig`** of type `ApplicationConfig`
- **Functional providers only** (no class-based providers)
- **Specific provider order**: error handling, core, routing, animations, libraries
- **PrimeNG Aura theme** configured globally
- **Event coalescing** enabled for Zone.js
- **Async animations** for better initial load

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
    providePrimeNG({ theme: { preset: Aura } })
  ]
};
```

### Unit Tests
**Location:** Colocated with components  
**Examples:** `app.spec.ts`

**Key Conventions:**
- **`.spec.ts` suffix** for test files
- **Import standalone components** in TestBed `imports` array
- **Async beforeEach** with `await compileComponents()`
- **Fixture-based testing** with manual change detection
- **Type-safe DOM queries** with `as HTMLElement`
- **Descriptive "should" statements** for test names

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
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  it('should display title', () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Dashboard');
  });
});
```

### API Controllers
**Location:** `src/Roman.RedisManager.Web/Controllers/`  
**Examples:** `WeatherForecastController.cs`

**Key Conventions:**
- **Inherit from `ControllerBase`** (not `Controller`)
- **`[ApiController]` and `[Route("[controller]")]`** attributes
- **Named routes** in HTTP method attributes
- **Direct return types** or `ActionResult<T>`
- **Modern C# 12** features (collection expressions, etc.)

**Example:**
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionService _service;
        
        public ConnectionsController(IConnectionService service)
        {
            _service = service;
        }
        
        [HttpGet(Name = "GetAllConnections")]
        public IEnumerable<ConnectionDto> GetAll()
        {
            return _service.GetAll();
        }
        
        [HttpGet("{id}", Name = "GetConnectionById")]
        public ActionResult<ConnectionDto> GetById(int id)
        {
            var conn = _service.GetById(id);
            if (conn == null) return NotFound();
            return Ok(conn);
        }
        
        [HttpPost(Name = "CreateConnection")]
        public ActionResult<ConnectionDto> Create(ConnectionDto dto)
        {
            var created = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}
```

### DTO Models
**Location:** `src/Roman.RedisManager.Web/Controllers/`  
**Examples:** `WeatherForecast.cs`

**Key Conventions:**
- **Auto-properties** with `{ get; set; }`
- **Expression-bodied members** for calculated properties
- **Modern .NET types** (`DateOnly`, `TimeOnly`)
- **Explicit nullable annotations** (`string?`)
- **Defaults for non-nullable** (`= string.Empty`)

**Example:**
```csharp
namespace Roman.RedisManager.Web.Controllers
{
    public class ConnectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 6379;
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Calculated property
        public string ConnectionString => $"{Host}:{Port}";
    }
}
```

---

## Feature Scaffold Guide

### Creating a New Angular Component

**Required Files:**
1. `{name}.ts` - Component logic
2. `{name}.html` - Template
3. `{name}.css` - Styles
4. `{name}.spec.ts` - Tests

**Steps:**
1. Create TypeScript component file:
   ```typescript
   import { Component, signal } from '@angular/core';
   import { ButtonModule } from 'primeng/button';
   
   @Component({
     selector: 'app-{name}',
     imports: [ButtonModule],
     templateUrl: './{name}.html',
     styleUrl: './{name}.css'
   })
   export class {Name} {
     protected readonly data = signal<any[]>([]);
   }
   ```

2. Create HTML template file:
   ```html
   <div class="component-container">
     <h2>{{ title() }}</h2>
     <!-- Component content -->
   </div>
   ```

3. Create CSS file (can be empty):
   ```css
   /* Component-specific styles */
   ```

4. Create test file:
   ```typescript
   import { TestBed } from '@angular/core/testing';
   import { {Name} } from './{name}';
   
   describe('{Name}', () => {
     beforeEach(async () => {
       await TestBed.configureTestingModule({
         imports: [{Name}],
       }).compileComponents();
     });
     
     it('should create', () => {
       const fixture = TestBed.createComponent({Name});
       expect(fixture.componentInstance).toBeTruthy();
     });
   });
   ```

5. If routed, add to `app.routes.ts`:
   ```typescript
   { path: '{name}', component: {Name} }
   ```

### Creating a New API Controller

**Required Files:**
1. `{Name}Controller.cs` - Controller
2. `{Name}Dto.cs` - DTO (or in same file)

**Steps:**
1. Create controller in `Controllers/` folder:
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   
   namespace Roman.RedisManager.Web.Controllers
   {
       [ApiController]
       [Route("[controller]")]
       public class {Name}Controller : ControllerBase
       {
           [HttpGet(Name = "GetAll{Name}")]
           public IEnumerable<{Name}Dto> GetAll()
           {
               // Implementation
           }
       }
   }
   ```

2. Create DTO:
   ```csharp
   namespace Roman.RedisManager.Web.Controllers
   {
       public class {Name}Dto
       {
           public int Id { get; set; }
           public string Name { get; set; } = string.Empty;
       }
   }
   ```

3. Register services in `Program.cs` if needed:
   ```csharp
   builder.Services.AddScoped<I{Name}Service, {Name}Service>();
   ```

### Adding a New Route

1. Import component in `app.routes.ts`
2. Add route definition:
   ```typescript
   { path: 'new-feature', component: NewFeature }
   ```
3. Ensure route doesn't conflict with API endpoints

### Adding HTTP Client to Angular

1. Add provider to `app.config.ts`:
   ```typescript
   import { provideHttpClient } from '@angular/common/http';
   
   providers: [
     // ... existing
     provideHttpClient(),
   ]
   ```

2. Inject in component:
   ```typescript
   import { HttpClient } from '@angular/common/http';
   
   export class MyComponent {
     private http = inject(HttpClient);
   }
   ```

---

## Integration Rules

### Architectural Constraints

**Frontend (Angular):**
- ✅ **MUST** use standalone components with imports array
- ✅ **MUST** use signals for reactive state
- ✅ **MUST** import PrimeNG components at component level
- ✅ **MUST** use separate template and style files
- ✅ **MUST** follow naming convention without `.component` suffix
- ❌ **MUST NOT** use NgModules
- ❌ **MUST NOT** use class names ending in "Component"
- ❌ **MUST NOT** use inline templates or styles

**Backend (ASP.NET Core):**
- ✅ **MUST** inherit controllers from `ControllerBase`
- ✅ **MUST** use `[ApiController]` and `[Route("[controller]")]` attributes
- ✅ **MUST** use nullable reference types
- ✅ **MUST** use modern C# 12 features
- ❌ **MUST NOT** use `Controller` base class for APIs
- ❌ **MUST NOT** hardcode routes (use `[controller]` token)

**Routing:**
- ✅ **MUST** define all routes in `app.routes.ts`
- ✅ **MUST** use `provideRouter()` in app config
- ✅ **MUST** avoid route conflicts with API endpoints
- ✅ **MUST** use SPA fallback pattern on backend
- ❌ **MUST NOT** use RouterModule

**Theming:**
- ✅ **MUST** use PrimeNG Aura theme preset
- ✅ **MUST** use PrimeFlex for layouts before custom CSS
- ✅ **MUST** use OKLCH color space for custom colors
- ❌ **MUST NOT** override PrimeNG theme globally without good reason

**Testing:**
- ✅ **MUST** import standalone components in test `imports` array
- ✅ **MUST** use async `beforeEach` with `await compileComponents()`
- ✅ **MUST** colocate tests with source files
- ❌ **MUST NOT** use `declarations` array in TestBed

**Build & Deployment:**
- ✅ **MUST** let .NET build process build Angular app
- ✅ **MUST** output Angular build to `wwwroot/`
- ❌ **MUST NOT** manually copy Angular builds

---

## Example Prompt Usage

### Example 1: Create a Redis Connection Manager Component

**User Prompt:**
> "Create a component to display and manage Redis connections with a table showing connection name, host, port, and status. Include buttons to connect, disconnect, and delete."

**Expected AI Response:**
1. Create `connection-manager.ts`:
   ```typescript
   import { Component, signal } from '@angular/core';
   import { TableModule } from 'primeng/table';
   import { ButtonModule } from 'primeng/button';
   
   @Component({
     selector: 'app-connection-manager',
     imports: [TableModule, ButtonModule],
     templateUrl: './connection-manager.html',
     styleUrl: './connection-manager.css'
   })
   export class ConnectionManager {
     protected readonly connections = signal<Connection[]>([]);
     
     connect(id: number) { /* ... */ }
     disconnect(id: number) { /* ... */ }
     delete(id: number) { /* ... */ }
   }
   ```

2. Create `connection-manager.html`:
   ```html
   <p-table [value]="connections()">
     <ng-template pTemplate="header">
       <tr>
         <th>Name</th>
         <th>Host</th>
         <th>Port</th>
         <th>Status</th>
         <th>Actions</th>
       </tr>
     </ng-template>
     <ng-template pTemplate="body" let-conn>
       <tr>
         <td>{{ conn.name }}</td>
         <td>{{ conn.host }}</td>
         <td>{{ conn.port }}</td>
         <td>{{ conn.status }}</td>
         <td>
           <p-button label="Connect" (click)="connect(conn.id)" />
           <p-button label="Delete" (click)="delete(conn.id)" />
         </td>
       </tr>
     </ng-template>
   </p-table>
   ```

3. Create `connection-manager.css` (empty or minimal)

4. Create `connection-manager.spec.ts` with tests

5. Add route if needed:
   ```typescript
   { path: 'connections', component: ConnectionManager }
   ```

### Example 2: Create Redis Connection API Endpoint

**User Prompt:**
> "Create an API endpoint to manage Redis connections with CRUD operations"

**Expected AI Response:**
1. Create `ConnectionsController.cs`:
   ```csharp
   [ApiController]
   [Route("[controller]")]
   public class ConnectionsController : ControllerBase
   {
       [HttpGet(Name = "GetAllConnections")]
       public IEnumerable<ConnectionDto> GetAll() { /* ... */ }
       
       [HttpPost(Name = "CreateConnection")]
       public ActionResult<ConnectionDto> Create(ConnectionDto dto) { /* ... */ }
       
       [HttpDelete("{id}", Name = "DeleteConnection")]
       public IActionResult Delete(int id) { /* ... */ }
   }
   ```

2. Create `ConnectionDto.cs`:
   ```csharp
   public class ConnectionDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
       public string Host { get; set; } = string.Empty;
       public int Port { get; set; } = 6379;
       public string? Password { get; set; }
   }
   ```

### Example 3: Add Data Filtering Feature

**User Prompt:**
> "Add a search/filter component for Redis keys with a text input and search button"

**Expected AI Response:**
1. Create `key-search.ts`:
   ```typescript
   import { Component, signal, output } from '@angular/core';
   import { InputTextModule } from 'primeng/inputtext';
   import { ButtonModule } from 'primeng/button';
   
   @Component({
     selector: 'app-key-search',
     imports: [InputTextModule, ButtonModule],
     templateUrl: './key-search.html',
     styleUrl: './key-search.css'
   })
   export class KeySearch {
     protected readonly searchTerm = signal('');
     readonly search = output<string>();
     
     onSearch() {
       this.search.emit(this.searchTerm());
     }
   }
   ```

2. Create `key-search.html`:
   ```html
   <div class="flex gap-2">
     <input 
       pInputText 
       [(ngModel)]="searchTerm" 
       placeholder="Search keys..." />
     <p-button 
       label="Search" 
       icon="pi pi-search" 
       (click)="onSearch()" />
   </div>
   ```

---

## Additional Notes

### Technology Stack
- **Frontend:** Angular 20.3.0, TypeScript 5.9.2, PrimeNG 20.3.0, PrimeFlex 3.3.1
- **Backend:** ASP.NET Core 10.0, C# 12
- **Build:** Integrated (Angular builds into .NET wwwroot)
- **Testing:** Jasmine/Karma (frontend), built-in testing (backend)

### Code Quality Standards
- **TypeScript:** Strict mode enabled with comprehensive checks
- **C#:** Nullable reference types enabled
- **Formatting:** Prettier for frontend (100 char width, single quotes)
- **Structure:** Colocated files, clear separation of concerns

### When in Doubt
1. Check existing code patterns in the same domain
2. Prefer standalone/modern patterns over legacy approaches
3. Use PrimeNG components instead of building custom UI
4. Follow the naming conventions strictly
5. Keep files focused and single-purpose

---

**Generated:** 2025-11-30  
**Source:** Automated analysis of Roman.RedisManager codebase  
**Version:** 1.0
