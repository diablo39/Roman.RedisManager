# Style Guide: Global Usings

> Conventions unique to this project.

## Separate GlobalUsings.cs Files Per Project
Each project that needs global usings has its own `GlobalUsings.cs` file — not shared across projects.

## Extension Namespace Imports
The Application project globally imports extension namespaces:
```csharp
global using Roman.RedisManager.Extensions;
```

## Test Framework Imports
The Test project globally imports both the test framework and assertion library:
```csharp
global using Xunit;
global using Shouldly;
```

## Minimal Global Usings
Only commonly-used namespaces are imported globally. Project-specific imports (like `Roman.RedisManager.Domain.Entities`) remain as per-file `using` directives.
