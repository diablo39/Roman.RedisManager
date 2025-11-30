# Backend API Domain Analysis

## Overview
The backend API domain uses ASP.NET Core 10.0 Web API with attribute-based routing, OpenAPI documentation, and serves as both an API server and static file host for the Angular SPA.

## Key Patterns and Conventions

### API Controller Structure
Controllers follow the ASP.NET Core Web API pattern:

**From `Controllers/WeatherForecastController.cs`:**
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
```

**Key Patterns:**

1. **Controller Attributes:**
   ```csharp
   [ApiController]
   [Route("[controller]")]
   ```
   - `[ApiController]`: Enables API-specific behaviors (automatic model validation, binding source inference)
   - `[Route("[controller]")]`: Route based on controller name (e.g., `/weatherforecast`)

2. **Base Class:**
   ```csharp
   public class WeatherForecastController : ControllerBase
   ```
   - Inherit from `ControllerBase` (not `Controller`)
   - `ControllerBase` is for APIs without view support

3. **HTTP Method Attributes:**
   ```csharp
   [HttpGet(Name = "GetWeatherForecast")]
   public IEnumerable<WeatherForecast> Get()
   ```
   - Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` attributes
   - Named routes for OpenAPI documentation

4. **Return Types:**
   ```csharp
   public IEnumerable<WeatherForecast> Get()
   ```
   - Return DTOs or collections directly
   - Can use `ActionResult<T>` for more control

### DTO Models
Data Transfer Objects are defined as simple classes:

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

**Conventions:**
- Properties with `{ get; set; }` for data binding
- Calculated properties using expression-bodied members
- Nullable reference types (`string?`) when appropriate
- Located in same namespace as controllers (or separate Models folder)

### Application Configuration
**From `Program.cs`:**
```csharp
namespace Roman.RedisManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseDefaultFiles(); 
            app.UseStaticFiles();  

            app.MapControllers(); 

            // SPA fallback: serve index.html for non-API routes (Angular routing support)
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
```

**Configuration Patterns:**

1. **Service Registration:**
   ```csharp
   builder.Services.AddControllers();
   builder.Services.AddOpenApi();
   ```
   - Register controllers for API endpoints
   - Register OpenAPI for documentation

2. **Middleware Pipeline:**
   ```csharp
   app.UseHttpsRedirection();
   app.UseAuthorization();
   app.UseDefaultFiles(); 
   app.UseStaticFiles();
   ```
   - Order matters: redirects, then auth, then static files
   - `UseDefaultFiles()`: Serves index.html for directory requests
   - `UseStaticFiles()`: Serves files from wwwroot

3. **Endpoint Mapping:**
   ```csharp
   app.MapControllers();
   app.MapFallbackToFile("index.html");
   ```
   - Map API controllers first
   - Fallback to index.html for SPA routing support

4. **Environment-Specific Config:**
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.MapOpenApi();
   }
   ```
   - OpenAPI only in development

### Project Configuration
**From `Roman.RedisManager.Web.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
  </ItemGroup>

  <Target Name="BuildClientApp" BeforeTargets="Build;Publish">
    <Message Importance="high" Text="Building client app: roman-redis-manager-ui" />
    <Exec WorkingDirectory="$(ProjectDir)\..\Redis.RedisManager.UI" Command="npm install" />
    <Exec WorkingDirectory="$(ProjectDir)\..\Redis.RedisManager.UI" Command="npx ng build --configuration production --output-path=../Roman.RedisManager.Web/wwwroot" />
  </Target>

</Project>
```

**Key Features:**
- **.NET 10.0 Target**: Latest .NET framework
- **Nullable Reference Types**: Enabled for better null safety
- **Implicit Usings**: Common using statements added automatically
- **Build Integration**: Automatically builds Angular app before .NET build/publish

### Application Settings
**From `appsettings.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**From `appsettings.Development.json`:**
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

**Pattern:**
- Environment-specific configuration files
- Logging levels configured per namespace
- `AllowedHosts` for security

### Launch Settings
**From `Properties/launchSettings.json`:**
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5099",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7244;http://localhost:5099",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Conventions:**
- Multiple launch profiles (HTTP/HTTPS)
- Browser launch disabled (SPA handles UI)
- Specific ports configured

## Tools and Technologies

### Framework
- **ASP.NET Core 10.0**: Web API framework
- **Microsoft.AspNetCore.OpenApi 10.0.0**: OpenAPI/Swagger support

### Language Features
- **C# 12**: Latest C# language features
- **Nullable Reference Types**: Improved null safety
- **Implicit Usings**: Reduced boilerplate

### Serialization
- **System.Text.Json**: Built-in JSON serialization (implicit)

## Implementation Guidelines

### Creating New Controllers
Template for new API controllers:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<MyDto>> GetAll()
        {
            // Implementation
            return Ok(data);
        }

        [HttpGet("{id}")]
        public ActionResult<MyDto> GetById(int id)
        {
            // Implementation
            return Ok(data);
        }

        [HttpPost]
        public ActionResult<MyDto> Create(MyDto dto)
        {
            // Implementation
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
    }
}
```

### Creating DTOs
Best practices for data transfer objects:

```csharp
namespace Roman.RedisManager.Web.Controllers
{
    public class MyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // Non-nullable
        public string? Description { get; set; }          // Nullable
        public DateTime CreatedAt { get; set; }
    }
}
```

### Adding Services
Register services in `Program.cs`:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
builder.Services.AddHttpClient();  // If needed
```

### Error Handling
Use ActionResult for proper HTTP status codes:

```csharp
[HttpGet("{id}")]
public ActionResult<MyDto> GetById(int id)
{
    var item = _repository.FindById(id);
    if (item == null)
        return NotFound();
    
    return Ok(item);
}
```

### Dependency Injection
Inject services via constructor:

```csharp
public class MyController : ControllerBase
{
    private readonly IMyService _service;
    
    public MyController(IMyService service)
    {
        _service = service;
    }
}
```

### CORS Configuration
If needed for development:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// In middleware pipeline:
app.UseCors();
```

### API Versioning
For future API versions, consider:
- URL versioning: `/api/v1/[controller]`
- Header versioning
- Query string versioning
