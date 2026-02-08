# Style Guide: Tests

> Conventions unique to this project.

## Test Method Naming
`MethodName_Condition_ExpectedBehavior` with underscores separating the three parts:
```csharp
ListRedisServers_WhenCalled_ReturnsNonNullAndNonEmptyCollection
ListRedisServers_ReturnedServers_HaveNameAndEndpoints
SearchForKeys_ShouldReturnNonNullAndNonEmptyKeys
```

## Shouldly Assertions Only
All assertions use Shouldly — never `Assert.*` from xUnit:
```csharp
result.ShouldNotBeNull();
result.ShouldNotBeEmpty();
servers.ShouldAllBe(server => !string.IsNullOrWhiteSpace(server.Name));
servers.SelectMany(s => s.Endpoints).ShouldContain(e => e.Contains(':'));
```

## No Mocking Framework
Tests instantiate **real implementations** — no Moq, NSubstitute, or FakeItEasy:
```csharp
IRedisServerRepository repository = new RedisServerRepository(Options.Create(GetConfiguration()));
```

## Options.Create() for Configuration
Test configuration is provided via `Options.Create(config)` — creating real `IOptions<T>` wrappers:
```csharp
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
```

## Collection Expressions in Test Data
Test data uses C# collection expressions (`[..., ...]`):
```csharp
Servers = [
    new RedisServerConfiguration { ... },
    new RedisServerConfiguration { ... }
]
```

## Interface-Typed Variables
Repositories in tests are declared as their interface type:
```csharp
IRedisServerRepository repository = new RedisServerRepository(...);
```

## Global Usings
`Xunit` and `Shouldly` are imported via `GlobalUsings.cs` — not repeated in each test file.

## [Fact] Only — No [Theory]
All tests use `[Fact]`. No parameterized `[Theory]` tests exist.
