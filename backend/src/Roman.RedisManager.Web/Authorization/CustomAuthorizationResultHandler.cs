using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Text.Json;

namespace Roman.RedisManager.Web.Authorization
{
    public class CustomAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var policyName = ResolvePolicyName(authorizeResult);
                var response = new
                {
                    status = 403,
                    title = "Forbidden",
                    detail = $"You do not have the required '{policyName}' policy to access this resource."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static string ResolvePolicyName(PolicyAuthorizationResult authorizeResult)
        {
            var failedRequirements = authorizeResult.AuthorizationFailure?.FailedRequirements;
            if (failedRequirements is not null)
            {
                foreach (var requirement in failedRequirements)
                {
                    if (requirement is RolesAuthorizationRequirement rolesRequirement)
                    {
                        var roles = rolesRequirement.AllowedRoles;
                        if (roles.Contains("reader"))
                        {
                            return AuthorizationPolicies.Reader;
                        }

                        if (roles.Contains("editor"))
                        {
                            return AuthorizationPolicies.Editor;
                        }
                    }
                }
            }

            return "unknown";
        }
    }
}
