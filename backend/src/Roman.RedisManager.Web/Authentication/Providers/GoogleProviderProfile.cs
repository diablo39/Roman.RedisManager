using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Web.Authentication.Providers
{
    public class GoogleProviderProfile : IOidcProviderProfile
    {
        public OidcProviderKind Kind => OidcProviderKind.Google;

        public void ApplyDefaults(OidcProviderConfiguration provider, BearerAuthenticationProfileOptions options)
        {
            options.ValidIssuer = provider.Authority;
            options.ValidAudience = provider.ClientId;
        }
    }
}
