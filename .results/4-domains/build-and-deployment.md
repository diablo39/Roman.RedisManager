# Build and Deployment Domain Analysis

## Overview
The build system integrates both Angular frontend and ASP.NET Core backend into a unified deployment package, with the Angular app automatically built and bundled into the backend's wwwroot directory.

## Key Patterns and Conventions

### Integrated Build Process
The .NET project automatically builds the Angular app as part of its build pipeline:

**From `Roman.RedisManager.Web.csproj`:**
```xml
<Target Name="BuildClientApp" BeforeTargets="Build;Publish">
  <Message Importance="high" Text="Building client app: roman-redis-manager-ui" />
  <Exec WorkingDirectory="$(ProjectDir)\..\Redis.RedisManager.UI" Command="npm install" />
  <Exec WorkingDirectory="$(ProjectDir)\..\Redis.RedisManager.UI" Command="npx ng build --configuration production --output-path=../Roman.RedisManager.Web/wwwroot" />
</Target>
```

**Build Flow:**
1. MSBuild target `BuildClientApp` runs before `Build` and `Publish` targets
2. Navigate to Angular project directory
3. Run `npm install` to ensure dependencies are present
4. Run `npx ng build --configuration production` with output to backend's wwwroot
5. Continue with .NET build/publish

**Critical Details:**
- Target name: `BuildClientApp`
- Triggers: `BeforeTargets="Build;Publish"`
- Working directory: Relative path to Angular project
- Output path: `../Roman.RedisManager.Web/wwwroot` (relative to Angular project)
- Configuration: `production` for optimized builds

### Angular Build Configuration
**From `angular.json`:**
```json
{
  "projects": {
    "roman-redis-manager-ui": {
      "architect": {
        "build": {
          "builder": "@angular/build:application",
          "options": {
            "browser": "src/main.ts",
            "polyfills": ["zone.js"],
            "tsConfig": "tsconfig.app.json",
            "assets": [
              {
                "glob": "**/*",
                "input": "public"
              }
            ],
            "styles": ["src/styles.css"]
          },
          "configurations": {
            "production": {
              "budgets": [
                {
                  "type": "initial",
                  "maximumWarning": "500kB",
                  "maximumError": "1MB"
                },
                {
                  "type": "anyComponentStyle",
                  "maximumWarning": "4kB",
                  "maximumError": "8kB"
                }
              ],
              "outputHashing": "all"
            },
            "development": {
              "optimization": false,
              "extractLicenses": false,
              "sourceMap": true
            }
          }
        }
      }
    }
  }
}
```

**Production Build Features:**
- **Bundle budgets**: Warns/fails if bundles exceed size limits
  - Initial bundle: 500kB warning, 1MB error
  - Component styles: 4kB warning, 8kB error
- **Output hashing**: All files hashed for cache busting
- **Optimization**: Enabled (minification, tree-shaking, etc.)

**Development Build Features:**
- **No optimization**: Faster builds
- **Source maps**: For debugging
- **No license extraction**: Faster builds

### Backend Static File Serving
**From `Program.cs`:**
```csharp
app.UseDefaultFiles(); 
app.UseStaticFiles();  

app.MapControllers(); 

// SPA fallback: serve index.html for non-API routes (Angular routing support)
app.MapFallbackToFile("index.html");
```

**Serving Strategy:**
1. `UseDefaultFiles()`: Automatically serves index.html for directory requests
2. `UseStaticFiles()`: Serves all files from wwwroot (JS, CSS, images, etc.)
3. `MapFallbackToFile("index.html")`: Catches non-API routes and serves index.html for Angular routing

### Package Management

**Frontend (`package.json`):**
```json
{
  "scripts": {
    "ng": "ng",
    "start": "ng serve",
    "build": "ng build",
    "watch": "ng build --watch --configuration development",
    "test": "ng test"
  }
}
```

**Available npm scripts:**
- `npm start`: Development server (ng serve)
- `npm run build`: Production build
- `npm run watch`: Development build with watch mode
- `npm test`: Run unit tests

**Backend (`Roman.RedisManager.Web.csproj`):**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
```

NuGet packages managed via PackageReference items in .csproj file.

### Development Workflow

**Frontend Development:**
```bash
cd src/Roman.RedisManager.UI
npm install
npm start  # Runs on http://localhost:4200 (default)
```

**Backend Development:**
```bash
cd src/Roman.RedisManager.Web
dotnet run  # Runs on configured ports (5099/7244)
```

**Full Stack Development:**
- Option 1: Run both separately (frontend on 4200, backend on configured ports)
- Option 2: Build frontend, run backend only (serves built frontend)

### Deployment Build

**Full build command:**
```bash
cd src/Roman.RedisManager.Web
dotnet publish -c Release
```

**Build process:**
1. MSBuild triggers `BuildClientApp` target
2. npm install runs in Angular project
3. Angular production build outputs to wwwroot
4. .NET compiles and publishes with frontend included
5. Output in `bin/Release/net10.0/publish/`

### Build Output Structure
After publish, the output includes:
```
publish/
├── wwwroot/
│   ├── index.html
│   ├── main.[hash].js
│   ├── polyfills.[hash].js
│   ├── styles.[hash].css
│   └── assets/
├── Roman.RedisManager.Web.dll
├── appsettings.json
└── web.config (if IIS deployment)
```

## Tools and Technologies

### Frontend Build Tools
- **@angular/build**: Modern esbuild-based Angular builder
- **@angular/cli**: Command-line interface
- **TypeScript**: Compilation to JavaScript
- **npm**: Package management

### Backend Build Tools
- **MSBuild**: .NET build system
- **dotnet CLI**: Command-line tooling
- **NuGet**: Package management

### Integration
- **MSBuild Exec task**: Runs npm and Angular CLI commands
- **BeforeTargets**: Ensures build order

## Implementation Guidelines

### Local Development Setup
1. Clone repository
2. Restore backend dependencies: `dotnet restore`
3. Navigate to Angular project: `cd src/Roman.RedisManager.UI`
4. Install frontend dependencies: `npm install`
5. Run frontend: `npm start` (optional, for development)
6. Run backend: `dotnet run` from Web project

### Adding npm Dependencies
```bash
cd src/Roman.RedisManager.UI
npm install <package-name>
npm install --save-dev <dev-package-name>
```

### Adding NuGet Packages
```bash
cd src/Roman.RedisManager.Web
dotnet add package <package-name>
```

### Modifying Build Output Path
If changing where Angular builds to:
1. Update `--output-path` in `BuildClientApp` target
2. Update `angular.json` if setting a default output path
3. Ensure path is relative and points to wwwroot

### Environment-Specific Builds
Angular environments:
```bash
ng build --configuration development
ng build --configuration production
```

.NET configurations:
```bash
dotnet build -c Debug
dotnet build -c Release
dotnet publish -c Release
```

### Build Optimization
**Frontend:**
- Production build automatically optimizes
- Adjust budgets in `angular.json` if needed
- Use `--source-map=false` for smaller production builds

**Backend:**
- Publish with ReadyToRun: `dotnet publish -c Release -p:PublishReadyToRun=true`
- Self-contained deployment: `dotnet publish -c Release --self-contained`

### Continuous Integration
For CI/CD pipelines:
```bash
# Restore dependencies
dotnet restore

# Build and publish (includes Angular build)
dotnet publish -c Release -o ./publish

# Run tests
dotnet test
cd src/Roman.RedisManager.UI && npm test -- --watch=false
```

### Troubleshooting Build Issues
Common issues:
- **npm not found**: Ensure npm is in PATH
- **Angular CLI not found**: MSBuild target uses `npx ng` (doesn't require global install)
- **Output path issues**: Check relative path resolution in MSBuild
- **Build order**: Angular must build before backend can serve files
