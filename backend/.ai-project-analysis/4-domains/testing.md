# Domain: Testing

## Overview
Tests are written using **xUnit** as the test framework and **Shouldly** as the assertion library. The test project references only the Domain and Infrastructure layers — not the Web or Application layers.

## File Structure
```
tests/Roman.RedisManager.Tests/
├── RedisRepositoryTests.cs
├── RedisServerRepositoryTests.cs
├── GlobalUsings.cs
└── Roman.RedisManager.Tests.csproj
```

## Test Conventions

### Global Usings
```csharp
global using Xunit;
global using Shouldly;
```

Both `Xunit` and `Shouldly` are imported globally so every test file can use `[Fact]` and `.ShouldNotBeNull()` without per-file imports.

### Test Class: RedisServerRepositoryTests

```csharp
namespace Roman.RedisManager.Tests
{
    public class RedisServerRepositoryTests
    {
        [Fact]
        public void ListRedisServers_WhenCalled_ReturnsNonNullAndNonEmptyCollection()
        {
            IRedisServerRepository repository = new RedisServerRepository(Options.Create(GetConfiguration()));
            var result = repository.ListRedisServers();
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public void ListRedisServers_ReturnedServers_HaveNameAndEndpoints()
        {
            IRedisServerRepository repository = new RedisServerRepository(Options.Create(GetConfiguration()));
            var servers = repository.ListRedisServers().ToList();
            servers.ShouldAllBe(server => !string.IsNullOrWhiteSpace(server.Name));
            servers.ShouldAllBe(server => server.Endpoints != null && server.Endpoints.Any());
            servers.SelectMany(s => s.Endpoints).ShouldContain(e => e.Contains(':'));
        }

        private RedisConfiguration GetConfiguration()
        {
            return new RedisConfiguration
            {
                Servers = [
                    new RedisServerConfiguration { Name = "Test server 1", Endpoints = ["localhost:6379"] },
                    new RedisServerConfiguration { Name = "Test server 2", Endpoints = ["localhost:16379"] }
                ]
            };
        }
    }
}
```

### Test Class: RedisRepositoryTests

```csharp
namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact]
        public void SearchForKeys_ShouldReturnNonNullAndNonEmptyKeys()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");
            IRedisRepository redisRepository = new RedisRepository(connectionMultiplexer);
            RedisSearchResult result = redisRepository.SearchForKeys(predicate: string.Empty);
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }
    }
}
```

### Key Conventions

1. **Naming pattern**: `MethodName_Condition_ExpectedBehavior` (e.g., `ListRedisServers_WhenCalled_ReturnsNonNullAndNonEmptyCollection`).
2. **[Fact] only**: No `[Theory]` or parameterized tests — all tests are simple facts.
3. **Shouldly assertions**: `.ShouldNotBeNull()`, `.ShouldNotBeEmpty()`, `.ShouldAllBe()`, `.ShouldContain()`.
4. **No mocking framework**: Tests instantiate real repository classes with either:
   - `Options.Create(config)` for configuration-based repositories.
   - `ConnectionMultiplexer.Connect(...)` for Redis-dependent repositories (integration tests).
5. **Private helper methods**: Shared test configuration created via `GetConfiguration()`.
6. **Collection expressions**: Test data uses C# collection expressions (`[..., ...]`).
7. **Interface-typed variables**: Repositories are declared as their interface type (e.g., `IRedisServerRepository repository = new RedisServerRepository(...)`).

## NuGet Dependencies
```xml
<PackageReference Include="coverlet.collector" Version="6.0.4" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="Shouldly" Version="4.3.0" />
```

## Project References
```xml
<ProjectReference Include="..\..\src\Roman.RedisManager.Domain\Roman.RedisManager.Domain.csproj" />
<ProjectReference Include="..\..\src\Roman.RedisManager.Infrastructure\Roman.RedisManager.Infrastructure.csproj" />
```

Tests reference Domain + Infrastructure only.
