# Domain: Configuration

## Overview
Configuration is implemented using the ASP.NET Core **Options pattern** with strongly-typed classes, data annotation validation, and binding from `appsettings.json` sections.

## File Structure
```
src/Roman.RedisManager.Infrastructure/Configuration/
├── RedisConfiguration.cs
└── RedisServerConfiguration.cs

src/Roman.RedisManager.Web/
├── appsettings.json
└── appsettings.Development.json
```

## Configuration Classes

### RedisConfiguration — Root Configuration Section

```csharp
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Web.Configuration
{
    public class RedisConfiguration
    {
        public const string SectionName = "Redis";

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Servers")]
        public required IEnumerable<RedisServerConfiguration> Servers { get; set; }
    }
}
```

### RedisServerConfiguration — Nested Configuration Object

```csharp
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Roman.RedisManager.Web.Configuration
{
    public class RedisServerConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Name")]
        public required string Name { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Endpoints")]
        public required IEnumerable<string> Endpoints { get; set; }

        [ConfigurationKeyName("Password")]
        public string? Password { get; set; }

        [ConfigurationKeyName("User")]
        public string? User { get; set; }
    }
}
```

### Key Conventions

1. **`const string SectionName`**: Root config classes define a constant for their JSON section name (e.g., `"Redis"`).
2. **`required` modifier**: Mandatory properties use C# `required` keyword.
3. **`[Required]` + `[MinLength(1)]`**: Data annotation validation ensures non-empty values.
4. **`[ConfigurationKeyName]`**: Explicit mapping from JSON property names to C# properties.
5. **Nullable optional properties**: Optional settings like `Password` and `User` are `string?`.
6. **Cross-layer namespace**: These classes physically live in `Infrastructure/Configuration/` but use namespace `Roman.RedisManager.Web.Configuration` — this allows the Web layer to reference them without adding a project reference to Infrastructure for configuration types.

## JSON Configuration Structure (appsettings.json)

```json
{
  "Redis": {
    "Servers": [
      {
        "Name": "TestRedis",
        "Endpoints": ["localhost:6379"],
        "User": "",
        "Password": ""
      }
    ]
  }
}
```

## Binding and Validation (Program.cs)

```csharp
builder.Services
    .AddOptions<RedisConfiguration>()
    .Bind(builder.Configuration.GetSection(RedisConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

- **`.AddOptions<T>()`**: Registers the options type in DI.
- **`.Bind(...)`**: Binds the `"Redis"` section to `RedisConfiguration`.
- **`.ValidateDataAnnotations()`**: Enables `[Required]`, `[MinLength]`, etc.
- **`.ValidateOnStart()`**: Fails fast at startup if validation fails.

## Environment-Specific Configuration

`appsettings.Development.json` only overrides logging — no Redis-specific dev overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
