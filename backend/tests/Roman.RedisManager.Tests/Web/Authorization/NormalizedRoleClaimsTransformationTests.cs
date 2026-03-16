using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Web.Authentication;
using Roman.RedisManager.Web.Authentication.Providers;
using Roman.RedisManager.Web.Authorization;
using System.Security.Claims;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class NormalizedRoleClaimsTransformationTests
    {
        private static NormalizedRoleClaimsTransformation CreateTransformation(
            string providerKey,
            string issuer,
            OidcProviderKind kind,
            params RoleClaimMappingConfiguration[] mappings)
        {
            var authConfig = new OidcAuthenticationConfiguration
            {
                Providers = new[]
                {
                    new OidcProviderConfiguration
                    {
                        ProviderKey = providerKey,
                        DisplayName = providerKey,
                        Enabled = true,
                        Kind = kind,
                        Authority = issuer,
                        ClientId = $"{providerKey}-client-id"
                    }
                }
            };

            var roleConfig = new AuthorizationRoleMappingConfiguration
            {
                Roles = mappings.Select(m => new AuthorizationRoleConfiguration { RoleName = m.RoleName }).ToArray(),
                RoleClaimMappings = mappings
            };

            IOidcProviderProfile profile = kind switch
            {
                OidcProviderKind.EntraId => new EntraIdProviderProfile(),
                OidcProviderKind.Google => new GoogleProviderProfile(),
                _ => new GenericOidcProviderProfile()
            };

            var resolver = new OidcProviderProfileResolver(
                new[] { profile },
                Options.Create(authConfig));
            var evaluator = new RoleClaimMappingEvaluator(Options.Create(roleConfig));

            return new NormalizedRoleClaimsTransformation(resolver, evaluator);
        }

        [Fact]
        public async Task TransformAsync_WithReaderClaimMapping_AddsReaderRole()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "reader",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "readers_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://login.microsoftonline.com/common/v2.0"),
                new Claim("groups", "readers_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("reader").ShouldBeTrue();
        }

        [Fact]
        public async Task TransformAsync_WithEditorClaimMapping_AddsEditorRole()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "editor",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "editors_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://login.microsoftonline.com/common/v2.0"),
                new Claim("groups", "editors_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("editor").ShouldBeTrue();
        }

        [Fact]
        public async Task TransformAsync_WithAdminClaimMapping_AddsAdminRole()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "admin",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "admin_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://login.microsoftonline.com/common/v2.0"),
                new Claim("groups", "admin_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("admin").ShouldBeTrue();
        }

        [Fact]
        public async Task TransformAsync_WithNoMatchingClaims_DoesNotAddRoles()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "reader",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "readers_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://login.microsoftonline.com/common/v2.0"),
                new Claim("groups", "unrelated_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("reader").ShouldBeFalse();
        }

        [Fact]
        public async Task TransformAsync_WithUnauthenticatedPrincipal_DoesNotAddRoles()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "reader",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "readers_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(); // not authenticated
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.Claims.ShouldNotContain(c => c.Type == ClaimTypes.Role);
        }

        [Fact]
        public async Task TransformAsync_WithUnknownIssuer_DoesNotAddRoles()
        {
            // Arrange
            var transformation = CreateTransformation(
                "entra",
                "https://login.microsoftonline.com/common/v2.0",
                OidcProviderKind.EntraId,
                new RoleClaimMappingConfiguration
                {
                    RoleName = "reader",
                    ProviderKey = "entra",
                    ClaimKey = "groups",
                    AllowedValues = new[] { "readers_group" },
                    MatchMode = ClaimMatchMode.Any
                });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://unknown-issuer.example.com"),
                new Claim("groups", "readers_group")
            }, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var transformed = await transformation.TransformAsync(principal);

            // Assert
            transformed.IsInRole("reader").ShouldBeFalse();
        }
    }
}
