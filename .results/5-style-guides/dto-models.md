# DTO Models Style Guide

## Unique Conventions

This style guide covers **project-specific patterns** for Data Transfer Objects (DTOs).

### 1. Simple Property Pattern

**Project convention:**
- DTOs use simple auto-properties
- Located in Controllers namespace (for now)

**From `Controllers/WeatherForecast.cs`:**
```csharp
namespace Roman.RedisManager.Web.Controllers
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
```

**Key patterns:**
- Auto-properties: `{ get; set; }`
- Calculated properties: `=> expression`
- Nullable reference types: `string?`

### 2. Calculated Properties

**Project-specific approach:**
- Use expression-bodied members for calculated values
- No setter for calculated properties

**From `WeatherForecast.cs`:**
```csharp
public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
```

**Pattern:**
```csharp
public class ProductDto
{
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
    // Calculated property
    public decimal TotalPrice => Price * Quantity;
}
```

**Not used:**
```csharp
// ✗ Old calculated property syntax
public decimal TotalPrice
{
    get { return Price * Quantity; }
}
```

### 3. Modern .NET Types

**Project convention:**
- Use modern .NET types when appropriate
- `DateOnly` for dates without time
- `TimeOnly` for times without date (when needed)

**From `WeatherForecast.cs`:**
```csharp
public DateOnly Date { get; set; }
```

**Pattern:**
```csharp
// Date-only values
public DateOnly BirthDate { get; set; }
public DateOnly StartDate { get; set; }

// Full date-time when needed
public DateTime CreatedAt { get; set; }

// Time-only values
public TimeOnly OpenTime { get; set; }
```

### 4. Nullable Reference Types

**Project requirement:**
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Explicit nullability annotations

**From `WeatherForecast.cs`:**
```csharp
public string? Summary { get; set; }  // Nullable string
```

**Pattern:**
```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Non-nullable with default
    public string? Email { get; set; }                 // Nullable
    public string? PhoneNumber { get; set; }           // Nullable
}
```

### 5. DTO Location Convention

**Current project structure:**
- DTOs in same file/namespace as controllers
- Simple, flat structure

**Pattern:**
```csharp
namespace Roman.RedisManager.Web.Controllers
{
    // DTO definition
    public class WeatherForecast
    {
        // ...
    }
    
    // Controller using DTO
    public class WeatherForecastController : ControllerBase
    {
        public IEnumerable<WeatherForecast> Get() { ... }
    }
}
```

**As project grows, consider:**
```
Controllers/
├── Models/
│   ├── WeatherForecast.cs
│   ├── ConnectionDto.cs
│   └── RedisKeyDto.cs
└── WeatherForecastController.cs
```

### 6. No Validation Attributes Yet

**Current state:**
- No data validation attributes on DTOs
- Clean, simple property definitions

**Future pattern (when needed):**
```csharp
using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Range(18, 120)]
    public int Age { get; set; }
}
```

## Key Takeaways

When creating DTOs in this project:

1. **Auto-Properties**: Use `{ get; set; }` for simple properties
2. **Calculated Properties**: Use expression-bodied members `=>`
3. **Modern Types**: Use `DateOnly`, `TimeOnly` when appropriate
4. **Nullability**: Explicit nullable annotations (`string?`)
5. **Location**: Currently in Controllers namespace
6. **No Validation**: Add validation attributes when needed
7. **Initialization**: Use `= string.Empty` for non-nullable strings

## DTO Template

**Standard DTO structure for this project:**
```csharp
namespace Roman.RedisManager.Web.Controllers
{
    public class ConnectionDto
    {
        // Identity
        public int Id { get; set; }
        
        // Required properties with defaults
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        
        // Optional properties
        public int? Port { get; set; }
        public string? Password { get; set; }
        
        // Value types
        public int DatabaseIndex { get; set; }
        public bool UseSsl { get; set; }
        
        // Dates
        public DateTime CreatedAt { get; set; }
        public DateTime? LastConnected { get; set; }
        
        // Calculated property
        public string ConnectionString => 
            $"{Host}:{Port ?? 6379},ssl={UseSsl}";
    }
}
```

## Common Patterns

### Request DTOs
```csharp
public class CreateConnectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
}
```

### Response DTOs
```csharp
public class ConnectionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}
```

### List DTOs
```csharp
public class ConnectionListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    
    // Minimal info for list views
}
```

### Detail DTOs
```csharp
public class ConnectionDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Full info for detail views
    public List<string> RecentKeys { get; set; } = [];
    public Dictionary<string, string> ServerInfo { get; set; } = [];
}
```

## Naming Conventions

**DTO Naming Patterns:**
```csharp
// Entity name + "Dto"
public class ConnectionDto { }
public class RedisKeyDto { }

// Or operation-specific
public class CreateConnectionRequest { }
public class UpdateConnectionRequest { }
public class ConnectionResponse { }
public class ConnectionListItem { }
```

## Collections

**Modern C# collection initialization:**
```csharp
public class ContainerDto
{
    // Empty array initialization (C# 12)
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = [];
    
    // Or traditional
    public List<string> Items { get; set; } = new List<string>();
}
```
