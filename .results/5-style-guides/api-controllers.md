# API Controllers Style Guide

## Unique Conventions

This style guide covers **project-specific patterns** for ASP.NET Core Web API controllers.

### 1. Controller Naming Without "Controller" in Route

**Project pattern:**
- Use `[Route("[controller]")]` attribute
- Controller name becomes the route (minus "Controller" suffix)

**From `Controllers/WeatherForecastController.cs`:**
```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    // Route: /weatherforecast
}
```

**Pattern:**
```csharp
// Controller: ProductsController → Route: /products
// Controller: UsersController → Route: /users
[Route("[controller]")]
```

**Not used in this project:**
```csharp
// ✗ Hardcoded route
[Route("api/products")]

// ✗ Custom route naming
[Route("api/[controller]s")]
```

### 2. ControllerBase Inheritance

**Project convention:**
- Inherit from `ControllerBase` (not `Controller`)
- API controllers don't need view support

**Pattern:**
```csharp
public class WeatherForecastController : ControllerBase
{
    // No view-related methods needed
}
```

**Why `ControllerBase`:**
- Lighter weight (no view rendering overhead)
- API-specific base class
- Standard for Web API projects

### 3. Named Route Pattern

**Project-specific approach:**
- HTTP method attributes include route names
- Enables route reference in CreatedAtAction, etc.

**From `WeatherForecastController.cs`:**
```csharp
[HttpGet(Name = "GetWeatherForecast")]
public IEnumerable<WeatherForecast> Get()
{
    // ...
}
```

**Pattern:**
```csharp
[HttpGet(Name = "GetAll")]
[HttpGet("{id}", Name = "GetById")]
[HttpPost(Name = "Create")]
[HttpPut("{id}", Name = "Update")]
[HttpDelete("{id}", Name = "Delete")]
```

### 4. Collection Initialization Syntax

**Modern C# pattern used:**
- Collection expression syntax for arrays

**From `WeatherForecastController.cs`:**
```csharp
private static readonly string[] Summaries =
[
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
];
```

**This is C# 12 syntax:**
```csharp
// ✓ Collection expression (C# 12)
string[] items = ["one", "two", "three"];

// ✗ Old syntax
string[] items = new[] { "one", "two", "three" };
```

### 5. Direct Return Types

**Project pattern:**
- Return DTOs or collections directly
- No explicit `ActionResult` wrapper in simple cases

**From `WeatherForecastController.cs`:**
```csharp
[HttpGet(Name = "GetWeatherForecast")]
public IEnumerable<WeatherForecast> Get()
{
    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        // ...
    }).ToArray();
}
```

**Pattern:**
```csharp
// Simple success responses
public IEnumerable<MyDto> GetAll() { ... }
public MyDto Get(int id) { ... }

// When you need status code control
public ActionResult<MyDto> Get(int id) 
{
    var item = _repo.Find(id);
    if (item == null) return NotFound();
    return Ok(item);
}
```

### 6. LINQ for Data Generation

**Project approach:**
- Use LINQ for data construction
- Method chaining pattern

**From `WeatherForecastController.cs`:**
```csharp
return Enumerable.Range(1, 5).Select(index => new WeatherForecast
{
    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    TemperatureC = Random.Shared.Next(-20, 55),
    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
})
.ToArray();
```

**Pattern demonstrates:**
- `Enumerable.Range()` for sequence generation
- `.Select()` for projection
- Object initialization syntax
- `Random.Shared` (modern .NET random number generation)
- `.ToArray()` for materialization

### 7. Controller Location and Namespace

**Project structure:**
- Controllers in `Controllers/` folder
- Namespace matches project structure

**Pattern:**
```csharp
namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyController : ControllerBase
    {
        // ...
    }
}
```

## Key Takeaways

When creating controllers in this project:

1. **Inheritance**: Use `ControllerBase` for API controllers
2. **Routing**: Use `[Route("[controller]")]` for convention-based routes
3. **HTTP Methods**: Include named routes in HTTP attributes
4. **Return Types**: Use direct types for simple cases, `ActionResult<T>` when needed
5. **Modern C#**: Use C# 12 features (collection expressions, etc.)
6. **LINQ**: Leverage LINQ for data operations
7. **Namespace**: Follow `Roman.RedisManager.Web.Controllers` pattern

## Controller Template

**Standard controller structure for this project:**
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        // Dependencies via constructor injection
        private readonly IProductService _productService;
        
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        
        [HttpGet(Name = "GetAllProducts")]
        public IEnumerable<ProductDto> GetAll()
        {
            return _productService.GetAll();
        }
        
        [HttpGet("{id}", Name = "GetProductById")]
        public ActionResult<ProductDto> GetById(int id)
        {
            var product = _productService.GetById(id);
            if (product == null)
                return NotFound();
            
            return Ok(product);
        }
        
        [HttpPost(Name = "CreateProduct")]
        public ActionResult<ProductDto> Create(ProductDto dto)
        {
            var created = _productService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        
        [HttpPut("{id}", Name = "UpdateProduct")]
        public ActionResult<ProductDto> Update(int id, ProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest();
            
            var updated = _productService.Update(dto);
            if (updated == null)
                return NotFound();
            
            return Ok(updated);
        }
        
        [HttpDelete("{id}", Name = "DeleteProduct")]
        public IActionResult Delete(int id)
        {
            var success = _productService.Delete(id);
            if (!success)
                return NotFound();
            
            return NoContent();
        }
    }
}
```

## Common Patterns

### Dependency Injection
```csharp
private readonly IMyService _service;

public MyController(IMyService service)
{
    _service = service;
}
```

### Action Results
```csharp
return Ok(data);              // 200
return Created(uri, data);    // 201
return NoContent();           // 204
return BadRequest();          // 400
return NotFound();            // 404
return Conflict();            // 409
```

### Model Validation
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

### Route Parameters
```csharp
[HttpGet("{id}")]
public ActionResult<MyDto> Get(int id) { ... }

[HttpGet("{category}/{id}")]
public ActionResult<MyDto> Get(string category, int id) { ... }
```

### Query Parameters
```csharp
[HttpGet]
public IEnumerable<MyDto> GetAll([FromQuery] string search, [FromQuery] int page = 1)
{
    // ...
}
```
