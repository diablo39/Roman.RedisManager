# Domain: Extensions

## Overview
The Extensions project provides reusable utility extension methods in a standalone class library with zero external dependencies.

## File Structure
```
src/Roman.RedisManager.Extensions/
├── StringExtensions.cs
└── Roman.RedisManager.Extensions.csproj
```

## Extension Method Pattern

### StringExtensions

```csharp
namespace Roman.RedisManager.Extensions
{
    public static class StringExtensions
    {
        public static string ToMd5Hash(this string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
```

### Key Conventions

1. **Class naming**: `{Type}Extensions` — e.g., `StringExtensions` for `string` extension methods.
2. **Static class**: Extension method containers are `public static class`.
3. **Method naming**: Descriptive verb prefix (e.g., `ToMd5Hash`) indicating transformation.
4. **Namespace**: `Roman.RedisManager.Extensions` — a flat namespace for all extensions.
5. **`using var`**: Disposable resources use `using var` declaration syntax.
6. **Pure function**: No side effects, no state, no external dependencies.
7. **Framework-only**: Uses only `System.Security.Cryptography` and `System.Text.Encoding` — no NuGet dependencies.

## Consumption via Global Usings

The Application project imports extensions globally:

```csharp
// src/Roman.RedisManager.Application/GlobalUsings.cs
global using Roman.RedisManager.Extensions;
```

This makes all extension methods in the `Roman.RedisManager.Extensions` namespace available throughout the Application project without per-file imports.

## Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Zero external dependencies — pure utility library.
