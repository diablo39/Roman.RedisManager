using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Roman.RedisManager.Web.OpenApi
{
    public class SecurityRequirementOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            if (metadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return Task.CompletedTask;
            }

            var authorizeAttributes = metadata.OfType<AuthorizeAttribute>().ToArray();
            if (authorizeAttributes.Length == 0)
            {
                return Task.CompletedTask;
            }

            var scopes = authorizeAttributes
                .Where(attribute => !string.IsNullOrEmpty(attribute.Policy))
                .Select(attribute => attribute.Policy!)
                .Distinct()
                .ToList();

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = scopes
            });

            return Task.CompletedTask;
        }
    }
}
