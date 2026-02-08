# Style Guide: Project Files

> Conventions unique to this project.

## Shared Property Group
All `.csproj` files share:
```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

## Minimal Dependencies
- **Domain**: Zero NuGet packages.
- **Extensions**: Zero NuGet packages.
- **Application**: Zero NuGet packages — only project references.
- **Infrastructure**: Only `StackExchange.Redis`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Configuration.Abstractions`.
- **Web**: `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore.SwaggerUI`, `WolverineFx`.

## Project Reference Direction
Dependency flows inward (Clean Architecture):
```
Web → Application, Infrastructure
Application → Domain, Extensions
Infrastructure → Domain
Tests → Domain, Infrastructure
```

## Empty Folder Placeholders
Domain `.csproj` includes `<Folder>` items for planned directories:
```xml
<Folder Include="DomainServices\" />
<Folder Include="ApplicationServices\" />
```
