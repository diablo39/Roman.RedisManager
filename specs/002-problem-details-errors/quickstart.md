# Quickstart: Implementing RFC 9457 Error Responses

1. **Register ProblemDetails service**
   ```csharp
   builder.Services.AddProblemDetails();
   ```
   Add this alongside `AddControllers()` in `Program.cs`.

2. **Configure middleware**
   ```csharp
   var app = builder.Build();
   app.UseExceptionHandler();
   app.UseStatusCodePages();
   app.MapControllers();
   ```
   These lines ensure that both unhandled exceptions and explicit status-code
   results produce a problem details body when the response would otherwise be
   empty.

3. **Add request metadata**
   Customize the built-in problem details options to inject trace information:
   ```csharp
   builder.Services.AddProblemDetails(options =>
   {
       options.CustomizeProblemDetails = ctx =>
       {
           var http = ctx.HttpContext;
           var traceId = Activity.Current?.TraceId.ToString() ?? http.TraceIdentifier;
           ctx.ProblemDetails.Extensions["traceId"] = traceId;
           if (http.Request.Headers.TryGetValue("traceparent", out var tp))
           {
               ctx.ProblemDetails.Extensions["traceparent"] = tp.ToString();
           }
           ctx.ProblemDetails.Extensions["requestPath"] = http.Request.Path.ToString();
       };
   });
   ```
   Use `traceId` as the primary support correlation identifier. Keep `traceparent`
   optional and diagnostic-only.

4. **Validate behavior**
   - Call an existing controller with invalid input; expect a JSON body matching
     the schema in `contracts/problem-details-schema.md`.
   - Trigger a server exception (e.g. throw in a handler) and observe 500 body.
   - Request a nonexistent route and verify a 404 problem details response.

5. **Documentation & tests**
   - Add examples to `Roman.RedisManager.Web.http`.
   - Write unit tests around any custom factory or middleware to ensure the
   extensions are present, with `traceId` always populated and `traceparent`
   optional when available.

6. **Optional**: Update Swagger/OpenAPI examples to reference the `ProblemDetails`
   schema for error responses.
