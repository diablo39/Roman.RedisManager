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

        [ConfigurationKeyName("BootstrapRedirectUri")]
        public string? BootstrapRedirectUri { get; set; }

        [ConfigurationKeyName("BootstrapScope")]
        public string? BootstrapScope { get; set; }

        [ConfigurationKeyName("BootstrapResponseType")]
        public string BootstrapResponseType { get; set; } = "code";

        [ConfigurationKeyName("BootstrapPostLogoutRedirectUri")]
        public string? BootstrapPostLogoutRedirectUri { get; set; }

        [ConfigurationKeyName("BootstrapSilentRedirectUri")]
        public string? BootstrapSilentRedirectUri { get; set; }

        [ConfigurationKeyName("BootstrapAutomaticSilentRenew")]
        public bool? BootstrapAutomaticSilentRenew { get; set; }

        [ConfigurationKeyName("BootstrapMetadataIssuer")]
        public string? BootstrapMetadataIssuer { get; set; }

        [ConfigurationKeyName("BootstrapMetadataAuthorizationEndpoint")]
        public string? BootstrapMetadataAuthorizationEndpoint { get; set; }

        [ConfigurationKeyName("BootstrapMetadataTokenEndpoint")]
        public string? BootstrapMetadataTokenEndpoint { get; set; }

        [ConfigurationKeyName("BootstrapMetadataUserInfoEndpoint")]
        public string? BootstrapMetadataUserInfoEndpoint { get; set; }

        [ConfigurationKeyName("BootstrapMetadataEndSessionEndpoint")]
        public string? BootstrapMetadataEndSessionEndpoint { get; set; }

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
