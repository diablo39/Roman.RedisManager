using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Roman.RedisManager.Web.OpenApi
{
    public class SecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.AddComponent("Bearer", (IOpenApiSecurityScheme)new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter a valid Bearer token"
            });

            return Task.CompletedTask;
        }
    }
}
