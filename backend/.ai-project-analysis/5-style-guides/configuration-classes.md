# Style Guide: Configuration Classes

> Conventions unique to this project.

## Cross-Layer Namespace
Configuration classes physically live in `Infrastructure/Configuration/` but use namespace `Roman.RedisManager.Web.Configuration`:
```csharp
namespace Roman.RedisManager.Web.Configuration
```

## Section Name Constant
Root configuration classes define a `const string SectionName` for binding:
```csharp
public const string SectionName = "Redis";
```

## Required + Data Annotation Validation
All mandatory properties use **both** C# `required` keyword and `[Required]` attribute:
```csharp
[Required]
[MinLength(1)]
[ConfigurationKeyName("Servers")]
public required IEnumerable<RedisServerConfiguration> Servers { get; set; }
```

## Explicit ConfigurationKeyName
Every property has an explicit `[ConfigurationKeyName("...")]` attribute mapping it to the JSON key — even when the C# property name already matches:
```csharp
[ConfigurationKeyName("Name")]
public required string Name { get; set; }
```

## Nullable Optional Properties
Optional configuration values use nullable types:
```csharp
[ConfigurationKeyName("Password")]
public string? Password { get; set; }
```

## Collection Type
Configuration collections use `IEnumerable<T>` (not `List<T>` or arrays).
