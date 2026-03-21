using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Web.Authorization;
using System.Security.Claims;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class RoleClaimMappingEvaluatorTests
    {
        [Fact]
        public void ResolveRoles_WithMatchingClaim_ReturnsMappedRole()
        {
            // Arrange
            var configuration = new AuthorizationRoleMappingConfiguration
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

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "admin_group")
            }, "test"));

            var evaluator = new RoleClaimMappingEvaluator(Options.Create(configuration));

            // Act
            var roles = evaluator.ResolveRoles(principal, "entra");

            // Assert
            roles.ShouldContain("redis-reader");
            roles.Count.ShouldBe(1);
        }

        [Fact]
        public void ResolveRoles_WithProviderMismatch_ReturnsNoRoles()
        {
            // Arrange
            var configuration = new AuthorizationRoleMappingConfiguration
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

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "admin_group")
            }, "test"));

            var evaluator = new RoleClaimMappingEvaluator(Options.Create(configuration));

            // Act
            var roles = evaluator.ResolveRoles(principal, "google");

            // Assert
            roles.ShouldBeEmpty();
        }

        [Fact]
        public void ResolveRoles_WhenAllowedValuesUpdatedInConfiguration_ReflectsNewMappingWithoutCodeChange()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "ops_readers")
            }, "test"));

            var beforeUpdate = new AuthorizationRoleMappingConfiguration
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

            var afterUpdate = new AuthorizationRoleMappingConfiguration
            {
                Roles = new[] { new AuthorizationRoleConfiguration { RoleName = "redis-reader" } },
                RoleClaimMappings = new[]
                {
                    new RoleClaimMappingConfiguration
                    {
                        RoleName = "redis-reader",
                        ProviderKey = "entra",
                        ClaimKey = "groups",
                        AllowedValues = new[] { "ops_readers" },
                        MatchMode = ClaimMatchMode.Any
                    }
                }
            };

            var evaluatorBeforeUpdate = new RoleClaimMappingEvaluator(Options.Create(beforeUpdate));
            var evaluatorAfterUpdate = new RoleClaimMappingEvaluator(Options.Create(afterUpdate));

            // Act
            var rolesBeforeUpdate = evaluatorBeforeUpdate.ResolveRoles(principal, "entra");
            var rolesAfterUpdate = evaluatorAfterUpdate.ResolveRoles(principal, "entra");

            // Assert
            rolesBeforeUpdate.ShouldBeEmpty();
            rolesAfterUpdate.ShouldContain("redis-reader");
        }

        [Fact]
        public void ResolveRoles_WithMatchModeAll_RequiresAllValuesPresent()
        {
            // Arrange
            var configuration = new AuthorizationRoleMappingConfiguration
            {
                Roles = new[] { new AuthorizationRoleConfiguration { RoleName = "admin" } },
                RoleClaimMappings = new[]
                {
                    new RoleClaimMappingConfiguration
                    {
                        RoleName = "admin",
                        ProviderKey = "entra",
                        ClaimKey = "groups",
                        AllowedValues = new[] { "admin_group", "super_admin_group" },
                        MatchMode = ClaimMatchMode.All
                    }
                }
            };

            var principalWithAll = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "admin_group"),
                new Claim("groups", "super_admin_group")
            }, "test"));

            var principalWithOne = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "admin_group")
            }, "test"));

            var evaluator = new RoleClaimMappingEvaluator(Options.Create(configuration));

            // Act
            var rolesWithAll = evaluator.ResolveRoles(principalWithAll, "entra");
            var rolesWithOne = evaluator.ResolveRoles(principalWithOne, "entra");

            // Assert
            rolesWithAll.ShouldContain("admin");
            rolesWithOne.ShouldBeEmpty();
        }

        [Fact]
        public void ResolveRoles_WithDifferentCase_MatchesCaseInsensitively()
        {
            // Arrange
            var configuration = new AuthorizationRoleMappingConfiguration
            {
                Roles = new[] { new AuthorizationRoleConfiguration { RoleName = "reader" } },
                RoleClaimMappings = new[]
                {
                    new RoleClaimMappingConfiguration
                    {
                        RoleName = "reader",
                        ProviderKey = "ENTRA",
                        ClaimKey = "groups",
                        AllowedValues = new[] { "Readers_Group" },
                        MatchMode = ClaimMatchMode.Any
                    }
                }
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("groups", "readers_group")
            }, "test"));

            var evaluator = new RoleClaimMappingEvaluator(Options.Create(configuration));

            // Act
            var roles = evaluator.ResolveRoles(principal, "entra");

            // Assert
            roles.ShouldContain("reader");
        }
    }
}
