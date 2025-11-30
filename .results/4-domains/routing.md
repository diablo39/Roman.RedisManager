# Routing Domain Analysis

## Overview
The routing domain uses Angular Router with a centralized route configuration and SPA fallback support from the ASP.NET Core backend.

## Key Patterns and Conventions

### Route Configuration
Routes are defined in a dedicated configuration file:

**From `src/app/app.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [];
```

**Conventions:**
- Export a `routes` constant of type `Routes`
- Currently empty array (placeholder for future routes)
- Routes will be added as the application grows

### Router Integration
The router is configured at the application level:

**From `src/app/app.config.ts`:**
```typescript
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    provideRouter(routes),
    // ...
  ]
};
```

**Pattern:**
- Import `provideRouter` from `@angular/router`
- Import route configuration from `app.routes.ts`
- Add to `providers` array in ApplicationConfig

### Router Outlet
The main app component includes the router outlet:

**From `src/app/app.html`:**
```html
<router-outlet />
```

**Usage:**
- Self-closing tag syntax: `<router-outlet />`
- Placed after main content structure
- Renders routed components dynamically

### Component-Level Router Import
Components that use routing features must import RouterOutlet:

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

## Backend SPA Fallback

### ASP.NET Core Configuration
The backend ensures all non-API routes serve the Angular app:

**From `src/Roman.RedisManager.Web/Program.cs`:**
```csharp
app.UseDefaultFiles(); 
app.UseStaticFiles();  

app.MapControllers(); 

// SPA fallback: serve index.html for non-API routes (Angular routing support)
app.MapFallbackToFile("index.html");
```

**Pattern:**
- `UseDefaultFiles()`: Serves index.html for directory requests
- `UseStaticFiles()`: Serves static assets from wwwroot
- `MapControllers()`: Registers API controller routes
- `MapFallbackToFile("index.html")`: Catches all non-API routes and serves index.html

**Critical Constraint:**
- API routes must be distinguishable from SPA routes
- Controller routes use `[Route("[controller]")]` attribute routing
- This ensures API calls go to controllers, not the fallback

## Tools and Technologies

### Angular Router
- **@angular/router**: Core routing library
- **Standalone routing**: No RouterModule needed with provideRouter

### Backend Routing
- **ASP.NET Core Routing**: Attribute-based controller routing
- **Static File Middleware**: Serves Angular build output
- **Fallback Routing**: SPA-friendly fallback to index.html

## Implementation Guidelines

### Adding New Routes
When adding routes to `app.routes.ts`:

```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'connections', component: ConnectionsComponent },
  { path: '**', component: NotFoundComponent }
];
```

**Conventions:**
- Use path strings without leading slash
- Lazy loading for feature modules if needed
- Wildcard route (`**`) for 404 handling

### Route Guards and Resolvers
When adding route protection:
- Use functional guards (recommended in modern Angular)
- Define guards as standalone functions
- Add to route definition via `canActivate`, `canActivateChild`, etc.

### API vs SPA Routes
**API Routes Pattern:**
```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
```

- API routes: `/weatherforecast`, `/api/connections`, etc.
- SPA routes: Everything else falls back to Angular

**Best Practice:**
- Prefix API routes with `/api/` for clarity (optional but recommended)
- Avoid route naming conflicts between API controllers and Angular routes

### Navigation
In Angular components:
```typescript
import { Router } from '@angular/router';

constructor(private router: Router) {}

navigateToPage() {
  this.router.navigate(['/dashboard']);
}
```

Or using RouterLink in templates:
```html
<a routerLink="/dashboard">Dashboard</a>
```

### Route Parameters
For parameterized routes:
```typescript
{ path: 'connection/:id', component: ConnectionDetailComponent }
```

Access via ActivatedRoute:
```typescript
import { ActivatedRoute } from '@angular/router';

constructor(private route: ActivatedRoute) {
  this.route.params.subscribe(params => {
    const id = params['id'];
  });
}
```
