# Style Guide: Startup and Hosting

> Conventions unique to this project's Program.cs.

## Minimal Hosting Model
Uses `WebApplication.CreateBuilder` + `app.Run()` — not `Startup.cs` class.

## Service Registration Order
1. Options pattern binding + validation.
2. Singleton repository registrations.
3. `AddControllers()`.
4. `AddOpenApi()`.
5. `UseWolverine(...)` — last in service configuration.

## Wolverine Assembly Scanning
Dynamically discovers handlers from all loaded assemblies matching `Roman.RedisManager*`:
```csharp
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
{
    if (assembly.GetName().Name!.StartsWith("Roman.RedisManager"))
    {
        opts.Discovery.IncludeAssembly(assembly);
    }
}
```

## Middleware Pipeline Order
```
MapOpenApi → UseSwaggerUI (dev only)
→ UseHttpsRedirection
→ UseAuthorization
→ UseDefaultFiles → UseStaticFiles
→ MapControllers
→ MapFallbackToFile("index.html")
```

## SPA Fallback
`MapFallbackToFile("index.html")` is the last middleware — enables Angular SPA routing.

## Dev-Only Swagger
OpenAPI and Swagger UI are conditionally added:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}
```
