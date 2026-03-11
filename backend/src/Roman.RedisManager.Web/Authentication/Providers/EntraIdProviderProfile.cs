using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Web.Authentication.Providers
{
    public class EntraIdProviderProfile : IOidcProviderProfile
    {
        public OidcProviderKind Kind => OidcProviderKind.EntraId;

        public void ApplyDefaults(OidcProviderConfiguration provider, BearerAuthenticationProfileOptions options)
        {
            options.ValidIssuer = provider.Authority;
            options.ValidAudience = provider.ClientId;
        }
    }
}
