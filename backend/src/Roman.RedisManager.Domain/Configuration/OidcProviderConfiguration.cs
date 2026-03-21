using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public enum OidcProviderKind
    {
        EntraId,
        Google,
        GenericOidc
    }

    public class OidcProviderConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ProviderKey")]
        public required string ProviderKey { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("DisplayName")]
        public required string DisplayName { get; set; }

        [ConfigurationKeyName("Enabled")]
        public bool Enabled { get; set; } = true;

        [Required]
        [ConfigurationKeyName("Kind")]
        public required OidcProviderKind Kind { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Authority")]
        public required string Authority { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ClientId")]
        public required string ClientId { get; set; }

        [ConfigurationKeyName("ValidateAudience")]
        public bool ValidateAudience { get; set; } = false;

        [ConfigurationKeyName("SigningKey")]
        public string? SigningKey { get; set; }

    }

    public class OidcAuthenticationConfiguration : IValidatableObject
    {
        public const string SectionName = "Security:Authentication";

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Providers")]
        public required IEnumerable<OidcProviderConfiguration> Providers { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var providers = Providers?.ToList() ?? new List<OidcProviderConfiguration>();

            if (!providers.Any(provider => provider.Enabled))
            {
                yield return new ValidationResult("At least one enabled OIDC provider is required.", new[] { nameof(Providers) });
            }

            var duplicateKey = providers
                .GroupBy(provider => provider.ProviderKey, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateKey is not null)
            {
                yield return new ValidationResult($"Duplicate provider key '{duplicateKey.Key}'.", new[] { nameof(Providers) });
            }
        }
    }
}
