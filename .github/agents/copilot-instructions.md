# Roman.RedisManager Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-22

## Active Technologies
- C# on .NET 10.0 + ASP.NET Core 10, Wolverine 5.9.2, StackExchange.Redis 2.10.1 (001-redis-search-attributes)
- Redis (standalone and cluster), appsettings-based options for topology and limits (001-redis-search-attributes)
- C# with .NET 10.0 + ASP.NET Core Authentication/Authorization, Wolverine 5.9.2, StackExchange.Redis 2.10.1, Options pattern configuration (001-oidc-role-authorization)
- Application configuration (`appsettings*.json`) for identity providers, role mappings, and permission overrides; no new persistent datastore (001-oidc-role-authorization)
- C# / .NET 10.0 + ASP.NET Core 10, Wolverine 5.9.2, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1, StackExchange.Redis 2.10.1 (007-spa-oidc-config)
- N/A (configuration-backed read model from Options) (007-spa-oidc-config)

- C# / .NET 10.0 + xUnit 2.9.3, Shouldly 4.3.0, Stryker.NET (dotnet-stryker tool), PowerShell automation scripts (001-mutation-test-quality)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

# Add commands for C# / .NET 10.0

## Code Style

C# / .NET 10.0: Follow standard conventions

## Recent Changes
- 007-spa-oidc-config: Added C# / .NET 10.0 + ASP.NET Core 10, Wolverine 5.9.2, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1, StackExchange.Redis 2.10.1
- 001-oidc-role-authorization: Added C# with .NET 10.0 + ASP.NET Core Authentication/Authorization, Wolverine 5.9.2, StackExchange.Redis 2.10.1, Options pattern configuration
- 001-redis-search-attributes: Added C# on .NET 10.0 + ASP.NET Core 10, Wolverine 5.9.2, StackExchange.Redis 2.10.1


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
