using System.ComponentModel;
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Application.CQRS
{
    /// <summary>
    /// Identifies whether authentication bootstrap information is usable for sign-in.
    /// </summary>
    public enum AuthenticationBootstrapState
    {
        /// <summary>
        /// At least one provider is sign-in-capable.
        /// </summary>
        Available,

        /// <summary>
        /// No provider is currently sign-in-capable.
        /// </summary>
        Unavailable
    }

    /// <summary>
    /// Query message used to retrieve frontend authentication bootstrap settings.
    /// </summary>
    public class AuthenticationBootstrapQuery
    {
    }

    /// <summary>
    /// Optional OIDC metadata override values for browser-based clients.
    /// </summary>
    public record AuthenticationBootstrapMetadataOverridesDto(
        [property: Description("OIDC issuer identifier override.")]
        string? Issuer,

        [property: Description("OIDC authorization endpoint override.")]
        string? AuthorizationEndpoint,

        [property: Description("OIDC token endpoint override.")]
        string? TokenEndpoint,

        [property: Description("OIDC user info endpoint override.")]
        string? UserInfoEndpoint,

        [property: Description("OIDC end-session endpoint override.")]
        string? EndSessionEndpoint);

    /// <summary>
    /// Browser-safe OIDC settings used by SPA clients to initiate authentication.
    /// </summary>
    public record AuthenticationBootstrapOidcProfileDto(
        [property: Description("OIDC authority (issuer base URL).")]
        string Authority,

        [property: Description("Public client identifier for the SPA.")]
        string ClientId,

        [property: Description("Frontend redirect URI used after sign-in.")]
        string RedirectUri,

        [property: Description("Space-delimited scopes requested during sign-in.")]
        string Scope,

        [property: Description("OIDC response type used by the frontend client.")]
        string ResponseType,

        [property: Description("Optional frontend redirect URI used after sign-out.")]
        string? PostLogoutRedirectUri,

        [property: Description("Optional silent renew callback URI.")]
        string? SilentRedirectUri,

        [property: Description("Indicates whether automatic silent renew is enabled.")]
        bool? AutomaticSilentRenew,

        [property: Description("Optional discovery metadata override values.")]
        AuthenticationBootstrapMetadataOverridesDto? MetadataOverrides);

    /// <summary>
    /// One sign-in provider entry exposed to the frontend bootstrap response.
    /// </summary>
    public record AuthenticationBootstrapProviderDto(
        [property: Description("Stable unique key for the provider.")]
        string ProviderKey,

        [property: Description("Display name used by the frontend login UI.")]
        string DisplayName,

        [property: Description("Provider kind (EntraId, Google, GenericOidc).")]
        OidcProviderKind Kind,

        [property: Description("OIDC configuration used by the frontend for this provider.")]
        AuthenticationBootstrapOidcProfileDto Oidc);

    /// <summary>
    /// Authentication bootstrap response returned to unauthenticated frontend clients.
    /// </summary>
    public record AuthenticationBootstrapQueryResult(
        [property: Description("Endpoint-level bootstrap state: available or unavailable.")]
        string BootstrapState,

        [property: Description("Machine-readable reason codes when bootstrapState is unavailable.")]
        IReadOnlyCollection<string> UnavailableReasonCodes,

        [property: Description("Enabled and sign-in-capable providers.")]
        IReadOnlyCollection<AuthenticationBootstrapProviderDto> Providers,

        [property: Description("UTC timestamp indicating when the bootstrap response was generated.")]
        DateTime GeneratedAtUtc,

        [property: Description("Bootstrap response schema version.")]
        string Version);

    public static class AuthenticationBootstrapQueryHandler
    {
        private const string _version = "1.0";
        private const string _noEnabledProvidersReason = "no_enabled_providers";
        private const string _noSignInCapableProvidersReason = "no_sign_in_capable_providers";

        public static AuthenticationBootstrapQueryResult Handle(
            AuthenticationBootstrapQuery query,
            IOptions<OidcAuthenticationConfiguration> configuration)
        {
            _ = query;

            var providers = configuration.Value.Providers
                .Where(provider => provider.Enabled)
                .Select(TryMapProvider)
                .Where(result => result.IsCapable)
                .Select(result => result.Provider!)
                .ToList();

            var enabledProviders = configuration.Value.Providers.Count(provider => provider.Enabled);
            var unavailableReasonCodes = BuildUnavailableReasonCodes(enabledProviders, providers.Count);

            var state = providers.Count == 0
                ? AuthenticationBootstrapState.Unavailable
                : AuthenticationBootstrapState.Available;

            return new AuthenticationBootstrapQueryResult(
                state.ToString().ToLowerInvariant(),
                unavailableReasonCodes,
                providers,
                DateTime.UtcNow,
                _version);
        }

        private static IReadOnlyCollection<string> BuildUnavailableReasonCodes(int enabledProviders, int mappedProviders)
        {
            if (mappedProviders > 0)
            {
                return Array.Empty<string>();
            }

            return enabledProviders == 0
                ? new[] { _noEnabledProvidersReason }
                : new[] { _noSignInCapableProvidersReason };
        }

        private static (bool IsCapable, AuthenticationBootstrapProviderDto? Provider) TryMapProvider(OidcProviderConfiguration provider)
        {
            if (string.IsNullOrWhiteSpace(provider.BootstrapRedirectUri) ||
                string.IsNullOrWhiteSpace(provider.BootstrapScope))
            {
                return (false, null);
            }

            var metadata = BuildMetadataOverrides(provider);
            var oidc = new AuthenticationBootstrapOidcProfileDto(
                provider.Authority,
                provider.ClientId,
                provider.BootstrapRedirectUri,
                provider.BootstrapScope,
                string.IsNullOrWhiteSpace(provider.BootstrapResponseType) ? "code" : provider.BootstrapResponseType,
                provider.BootstrapPostLogoutRedirectUri,
                provider.BootstrapSilentRedirectUri,
                provider.BootstrapAutomaticSilentRenew,
                metadata);

            return (true, new AuthenticationBootstrapProviderDto(provider.ProviderKey, provider.DisplayName, provider.Kind, oidc));
        }

        private static AuthenticationBootstrapMetadataOverridesDto? BuildMetadataOverrides(OidcProviderConfiguration provider)
        {
            var hasAnyOverride =
                !string.IsNullOrWhiteSpace(provider.BootstrapMetadataIssuer) ||
                !string.IsNullOrWhiteSpace(provider.BootstrapMetadataAuthorizationEndpoint) ||
                !string.IsNullOrWhiteSpace(provider.BootstrapMetadataTokenEndpoint) ||
                !string.IsNullOrWhiteSpace(provider.BootstrapMetadataUserInfoEndpoint) ||
                !string.IsNullOrWhiteSpace(provider.BootstrapMetadataEndSessionEndpoint);

            if (!hasAnyOverride)
            {
                return null;
            }

            return new AuthenticationBootstrapMetadataOverridesDto(
                provider.BootstrapMetadataIssuer,
                provider.BootstrapMetadataAuthorizationEndpoint,
                provider.BootstrapMetadataTokenEndpoint,
                provider.BootstrapMetadataUserInfoEndpoint,
                provider.BootstrapMetadataEndSessionEndpoint);
        }
    }
}
