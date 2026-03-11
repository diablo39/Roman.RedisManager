using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Web.Authentication.Providers
{
    public class GenericOidcProviderProfile : IOidcProviderProfile
    {
        public OidcProviderKind Kind => OidcProviderKind.GenericOidc;

        public void ApplyDefaults(OidcProviderConfiguration provider, BearerAuthenticationProfileOptions options)
        {
            options.ValidIssuer = provider.Authority;
            options.ValidAudience = provider.ClientId;
        }
    }
}
