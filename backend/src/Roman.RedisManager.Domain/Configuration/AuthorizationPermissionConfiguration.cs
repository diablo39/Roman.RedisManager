using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public enum PermissionOverrideMode
    {
        ReplaceDefaults,
        NarrowDefaults
    }

    public class PermissionRuleConfiguration
    {
        [ConfigurationKeyName("Action")]
        public PermissionAction Action { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("AllowedRoles")]
        public required IEnumerable<string> AllowedRoles { get; set; }
    }

    public class AuthorizationPermissionConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Global")]
        public required IEnumerable<PermissionRuleConfiguration> Global { get; set; }
    }
}
