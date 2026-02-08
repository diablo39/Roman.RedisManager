# Domain: Web API

## Overview
The Web API layer is the HTTP presentation layer built on ASP.NET Core 10.0. Controllers expose RESTful endpoints and delegate all business logic to Wolverine CQRS handlers via `IMessageBus`.

## File Structure
```
src/Roman.RedisManager.Web/
├── Controllers/
│   └── RedisServersController.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Properties/
    └── launchSettings.json
```

## Controller Pattern

Controllers follow a strict thin-controller approach. They inherit from `ControllerBase`, use `[ApiController]` for automatic model validation, and route with `[Route("api/{resource}")]` using kebab-case.

### Example: RedisServersController

```csharp
[Route("api/redis-servers")]
[ApiController]
public class RedisServersController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RedisServersController(IMessageBus bus) => _bus = bus;

    [HttpGet]
    public async Task<RedisServersQueryResult> GetServers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return await _bus.InvokeAsync<RedisServersQueryResult>(
            new RedisServersQuery { PageNumber = pageNumber, PageSize = pageSize });
    }
}
```

### Key Observations
1. **Expression-bodied constructor**: `public RedisServersController(IMessageBus bus) => _bus = bus;`
2. **No direct repository usage**: The controller injects `IMessageBus`, not `IRedisServerRepository`.
3. **Query parameter defaults**: Pagination defaults are defined in method signature (`pageNumber = 1`, `pageSize = 10`).
4. **Async dispatch**: All handler calls use `await _bus.InvokeAsync<TResult>(message)`.
5. **Return type**: Action methods return the DTO directly (not `IActionResult` or `ActionResult<T>`).

## Startup / Hosting (Program.cs)

The application uses the minimal hosting model with `WebApplication.CreateBuilder`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Options pattern with validation
builder.Services
    .AddOptions<RedisConfiguration>()
    .Bind(builder.Configuration.GetSection(RedisConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// DI registration
builder.Services.AddSingleton<IRedisServerRepository, RedisServerRepository>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Wolverine mediator setup
builder.UseWolverine(opts =>
{
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (assembly.GetName().Name!.StartsWith("Roman.RedisManager"))
        {
            opts.Discovery.IncludeAssembly(assembly);
        }
    }
    opts.Durability.Mode = DurabilityMode.MediatorOnly;
});
```

### Middleware Pipeline Order
```csharp
app.MapOpenApi();              // OpenAPI endpoint (dev only)
app.UseSwaggerUI(...)          // Swagger UI (dev only)
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");  // SPA fallback
```

### Key Observations
1. **Wolverine assembly scanning**: All assemblies starting with `Roman.RedisManager` are included for handler discovery.
2. **MediatorOnly mode**: No durable messaging — purely in-process request/response.
3. **Singleton repositories**: `RedisServerRepository` registered as singleton.
4. **SPA fallback**: `MapFallbackToFile("index.html")` serves an Angular frontend from `wwwroot/`.
5. **OpenAPI**: Available only in Development environment.
