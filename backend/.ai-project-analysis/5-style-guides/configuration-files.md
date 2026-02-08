# Style Guide: Configuration Files

> Conventions unique to this project's JSON configuration.

## appsettings.json Structure
```json
{
  "Logging": { ... },
  "AllowedHosts": "*",
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

## Custom Section Naming
Application-specific configuration lives under a top-level key matching the `SectionName` constant (e.g., `"Redis"`).

## Server Array Pattern
Redis servers are defined as an array of objects, each with `Name`, `Endpoints` (array), and optional `User`/`Password`.

## Empty Strings for Optional Values
Optional credentials use empty strings (`""`) rather than omitting the keys or using `null`.

## Environment-Specific Overrides
`appsettings.Development.json` only overrides `Logging` — no Redis-specific dev settings.
