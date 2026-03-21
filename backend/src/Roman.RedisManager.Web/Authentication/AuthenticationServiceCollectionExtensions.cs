using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Web.Authentication.Providers;

namespace Roman.RedisManager.Web.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        private const string _bearerScheme = "Bearer";
        private const string _unknownScheme = "Bearer-unknown";
        private static readonly JsonWebTokenHandler _tokenHandler = new();

        public static IServiceCollection AddConfiguredIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IOidcProviderProfile, EntraIdProviderProfile>();
            services.AddSingleton<IOidcProviderProfile, GoogleProviderProfile>();
            services.AddSingleton<IOidcProviderProfile, GenericOidcProviderProfile>();
            services.AddSingleton<OidcProviderProfileResolver>();

            var authenticationConfiguration = configuration
                .GetSection(OidcAuthenticationConfiguration.SectionName)
                .Get<OidcAuthenticationConfiguration>()
                ?? throw new InvalidOperationException("Authentication configuration is required.");

            var enabledProviders = authenticationConfiguration.Providers
                .Where(provider => provider.Enabled)
                .ToArray();

            var authenticationBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = _bearerScheme;
                    options.DefaultChallengeScheme = _bearerScheme;
                })
                .AddPolicyScheme(_bearerScheme, _bearerScheme, options =>
                {
                    options.ForwardDefaultSelector = context => SelectScheme(context.Request, enabledProviders);
                });

            foreach (var provider in enabledProviders)
            {
                var schemeName = BuildProviderSchemeName(provider.ProviderKey);
                authenticationBuilder.AddJwtBearer(schemeName, options => ConfigureProviderJwtOptions(options, provider));
            }

            authenticationBuilder.AddJwtBearer(_unknownScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.NoResult();
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false
                };
            });

            return services;
        }

        private static string SelectScheme(HttpRequest request, IReadOnlyCollection<OidcProviderConfiguration> providers)
        {
            if (!request.Headers.TryGetValue("Authorization", out var authorizationValues))
            {
                return _unknownScheme;
            }

            var authorizationHeader = authorizationValues.ToString();
            var issuer = ExtractIssuer(authorizationHeader);
            if (string.IsNullOrWhiteSpace(issuer))
            {
                return _unknownScheme;
            }

            var provider = providers.FirstOrDefault(candidate =>
                issuer.StartsWith(candidate.Authority, StringComparison.OrdinalIgnoreCase));

            return provider is null ? _unknownScheme : BuildProviderSchemeName(provider.ProviderKey);
        }

        private static string BuildProviderSchemeName(string providerKey)
        {
            return $"Bearer-{providerKey}";
        }

        private static void ConfigureProviderJwtOptions(JwtBearerOptions options, OidcProviderConfiguration provider)
        {
            var hasSigningKey = !string.IsNullOrWhiteSpace(provider.SigningKey);
            if (!hasSigningKey)
            {
                options.Authority = provider.Authority;
                options.RequireHttpsMetadata = provider.Authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = provider.Authority,
                ValidateAudience = provider.ValidateAudience,
                ValidAudience = provider.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            if (hasSigningKey)
            {
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(provider.SigningKey!));
            }
        }

        private static string? ExtractIssuer(string authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            if (!_tokenHandler.CanReadToken(token))
            {
                return null;
            }

            var jwt = _tokenHandler.ReadJsonWebToken(token);
            return jwt.Issuer;
        }
    }
}
