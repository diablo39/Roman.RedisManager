using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Web.Authentication;
using Roman.RedisManager.Web.Authentication.Providers;
using Roman.RedisManager.Web.Authorization;
using System.Security.Claims;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class PrincipalRoleTransformationTests
    {
        [Fact]
        public async Task TransformAsync_WithMappedClaim_AddsRoleClaim()
        {
            // Arrange
            var authConfiguration = new OidcAuthenticationConfiguration
            {
                Providers = new[]
                {
                    new OidcProviderConfiguration
                    {
                        ProviderKey = "entra",
                        DisplayName = "Entra",
                        Enabled = true,
                        Kind = OidcProviderKind.EntraId,
                        Authority = "https://login.microsoftonline.com/common/v2.0",
                        ClientId = "entra-client-id"
                    }
                }
            };

            var roleConfiguration = new AuthorizationRoleMappingConfiguration
            {
                Roles = new[] { new AuthorizationRoleConfiguration { RoleName = "redis-reader" } },
                RoleClaimMappings = new[]
                {
                    new RoleClaimMappingConfiguration
                    {
                        RoleName = "redis-reader",
                        ProviderKey = "entra",
                        ClaimKey = "groups",
                        AllowedValues = new[] { "admin_group" },
                        MatchMode = ClaimMatchMode.Any
                    }
                }
            };

            var resolver = new OidcProviderProfileResolver(
                new IOidcProviderProfile[] { new EntraIdProviderProfile() },
                Options.Create(authConfiguration));
            var evaluator = new RoleClaimMappingEvaluator(Options.Create(roleConfiguration));
            var transformation = new NormalizedRoleClaimsTransformation(resolver, evaluator);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://login.microsoftonline.com/common/v2.0"),
                new Claim("groups", "admin_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("redis-reader").ShouldBeTrue();
        }
    }
}
