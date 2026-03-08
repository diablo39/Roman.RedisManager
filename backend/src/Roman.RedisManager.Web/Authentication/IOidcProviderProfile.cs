using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Web.Authentication
{
    public interface IOidcProviderProfile
    {
        OidcProviderKind Kind { get; }

        void ApplyDefaults(OidcProviderConfiguration provider, BearerAuthenticationProfileOptions options);
    }

    public class BearerAuthenticationProfileOptions
    {
        public string? ValidIssuer { get; set; }

        public string? ValidAudience { get; set; }
    }
}
