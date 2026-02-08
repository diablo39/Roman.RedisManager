# Style Guide: Extension Methods

> Conventions unique to this project.

## Class Naming
`{Type}Extensions` — e.g., `StringExtensions` for methods extending `string`.

## Namespace
Flat namespace: `Roman.RedisManager.Extensions` — all extension types share this namespace.

## Consumption via GlobalUsings.cs
Rather than importing per-file, the consuming project uses:
```csharp
global using Roman.RedisManager.Extensions;
```

## Using Declaration for Disposables
`using var` syntax (not `using (var x = ...)` block syntax):
```csharp
using var md5 = System.Security.Cryptography.MD5.Create();
```

## Fully Qualified Framework Types
Framework types are sometimes used fully qualified inline rather than via `using` directives:
```csharp
System.Security.Cryptography.MD5.Create()
System.Text.Encoding.UTF8.GetBytes(input)
```

## Zero Dependencies
Extension methods use only BCL types — no NuGet packages.
