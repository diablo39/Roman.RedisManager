using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public enum ClaimMatchMode
    {
        Any,
        All
    }

    public class AuthorizationRoleConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("RoleName")]
        public required string RoleName { get; set; }

        [ConfigurationKeyName("Description")]
        public string? Description { get; set; }
    }

    public class RoleClaimMappingConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("RoleName")]
        public required string RoleName { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ProviderKey")]
        public required string ProviderKey { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ClaimKey")]
        public required string ClaimKey { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("AllowedValues")]
        public required IEnumerable<string> AllowedValues { get; set; }

        [ConfigurationKeyName("MatchMode")]
        public ClaimMatchMode MatchMode { get; set; } = ClaimMatchMode.Any;
    }

    public class AuthorizationRoleMappingConfiguration : IValidatableObject
    {
        public const string SectionName = "Security:Authorization";

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Roles")]
        public required IEnumerable<AuthorizationRoleConfiguration> Roles { get; set; }

        [ConfigurationKeyName("RoleClaimMappings")]
        public IEnumerable<RoleClaimMappingConfiguration> RoleClaimMappings { get; set; } = Array.Empty<RoleClaimMappingConfiguration>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var roles = Roles?.ToList() ?? new List<AuthorizationRoleConfiguration>();
            var duplicateRole = roles
                .GroupBy(role => role.RoleName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateRole is not null)
            {
                yield return new ValidationResult($"Duplicate role '{duplicateRole.Key}'.", new[] { nameof(Roles) });
            }
        }
    }
}
