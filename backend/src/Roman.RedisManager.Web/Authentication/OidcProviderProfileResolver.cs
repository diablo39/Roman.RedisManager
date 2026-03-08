using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Web.Authentication
{
    public class OidcProviderProfileResolver
    {
        private readonly IReadOnlyDictionary<OidcProviderKind, IOidcProviderProfile> _profiles;
        private readonly OidcAuthenticationConfiguration _authenticationConfiguration;

        public OidcProviderProfileResolver(
            IEnumerable<IOidcProviderProfile> profiles,
            IOptions<OidcAuthenticationConfiguration> authenticationConfiguration)
        {
            _profiles = profiles.ToDictionary(profile => profile.Kind);
            _authenticationConfiguration = authenticationConfiguration.Value;
        }

        public OidcProviderConfiguration? ResolveEnabledProvider(string providerKey)
        {
            return _authenticationConfiguration.Providers
                .FirstOrDefault(provider => provider.Enabled &&
                    string.Equals(provider.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));
        }

        public OidcProviderConfiguration? ResolveProviderByIssuer(string? issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                return null;
            }

            return _authenticationConfiguration.Providers
                .FirstOrDefault(provider => provider.Enabled &&
                    issuer.StartsWith(provider.Authority, StringComparison.OrdinalIgnoreCase));
        }

        public BearerAuthenticationProfileOptions ResolveBearerDefaults(OidcProviderConfiguration provider)
        {
            var options = new BearerAuthenticationProfileOptions();
            if (_profiles.TryGetValue(provider.Kind, out var profile))
            {
                profile.ApplyDefaults(provider, options);
            }

            return options;
        }
    }
}
