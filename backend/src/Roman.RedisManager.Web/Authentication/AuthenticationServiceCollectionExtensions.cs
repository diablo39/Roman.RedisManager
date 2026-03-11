using Microsoft.AspNetCore.Authentication;
using Roman.RedisManager.Web.Authentication.Providers;

namespace Roman.RedisManager.Web.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredIdentity(this IServiceCollection services)
        {
            services.AddSingleton<IOidcProviderProfile, EntraIdProviderProfile>();
            services.AddSingleton<IOidcProviderProfile, GoogleProviderProfile>();
            services.AddSingleton<IOidcProviderProfile, GenericOidcProviderProfile>();
            services.AddSingleton<OidcProviderProfileResolver>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "Bearer";
                    options.DefaultChallengeScheme = "Bearer";
                })
                .AddScheme<AuthenticationSchemeOptions, BearerHeaderAuthenticationHandler>("Bearer", _ =>
                {
                });

            return services;
        }
    }
}
