using Microsoft.Extensions.Options;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class AuthenticationBootstrapQueryHandlerTests
    {
        [Fact]
        public void Handle_WithEnabledSignInCapableProviders_ReturnsAvailableState()
        {
            // Arrange
            var configuration = CreateOptions(
                CreateProvider("entra", true),
                CreateProvider("generic", true));

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Count.ShouldBe(2);
            result.UnavailableReasonCodes.ShouldBeEmpty();
        }

        [Fact]
        public void Handle_WithDisabledProviders_ExcludesThemFromResponse()
        {
            // Arrange
            var configuration = CreateOptions(
                CreateProvider("entra", true),
                CreateProvider("google", false));

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Count.ShouldBe(1);
            result.Providers.Single().ProviderKey.ShouldBe("entra");
        }

        [Fact]
        public void Handle_WithMissingBootstrapFields_ReturnsUnavailableWithReasonCode()
        {
            // Arrange
            var provider = CreateProvider("entra", true);
            provider.BootstrapRedirectUri = null;
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("unavailable");
            result.Providers.ShouldBeEmpty();
            result.UnavailableReasonCodes.ShouldContain("no_sign_in_capable_providers");
        }

        [Fact]
        public void Handle_WithNoEnabledProviders_ReturnsUnavailableNoEnabledProvidersReason()
        {
            // Arrange
            var configuration = CreateOptions(CreateProvider("entra", false));

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("unavailable");
            result.Providers.ShouldBeEmpty();
            result.UnavailableReasonCodes.ShouldContain("no_enabled_providers");
        }

        [Fact]
        public void Handle_WithGenericOidcMetadataOverrides_MapsOverrides()
        {
            // Arrange
            var provider = CreateProvider("generic", true);
            provider.BootstrapMetadataIssuer = "https://issuer.dev.local";
            provider.BootstrapMetadataAuthorizationEndpoint = "https://issuer.dev.local/connect/authorize";
            provider.BootstrapMetadataTokenEndpoint = "https://issuer.dev.local/connect/token";
            provider.BootstrapMetadataUserInfoEndpoint = "https://issuer.dev.local/connect/userinfo";
            provider.BootstrapMetadataEndSessionEndpoint = "https://issuer.dev.local/connect/endsession";
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            var mapped = result.Providers.Single();
            mapped.Oidc.MetadataOverrides.ShouldNotBeNull();
            mapped.Oidc.MetadataOverrides!.Issuer.ShouldBe("https://issuer.dev.local");
            mapped.Oidc.MetadataOverrides.AuthorizationEndpoint.ShouldBe("https://issuer.dev.local/connect/authorize");
            mapped.Oidc.MetadataOverrides.TokenEndpoint.ShouldBe("https://issuer.dev.local/connect/token");
            mapped.Oidc.MetadataOverrides.UserInfoEndpoint.ShouldBe("https://issuer.dev.local/connect/userinfo");
            mapped.Oidc.MetadataOverrides.EndSessionEndpoint.ShouldBe("https://issuer.dev.local/connect/endsession");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Handle_WithMissingResponseType_UsesCodeFallback(string? responseType)
        {
            // Arrange
            var provider = CreateProvider("entra", true);
            provider.BootstrapResponseType = responseType!;
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Single().Oidc.ResponseType.ShouldBe("code");
        }

        [Fact]
        public void Handle_WithExplicitResponseType_PreservesConfiguredValue()
        {
            // Arrange
            var provider = CreateProvider("entra", true);
            provider.BootstrapResponseType = "query";
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Single().Oidc.ResponseType.ShouldBe("query");
        }

        [Fact]
        public void Handle_WithoutMetadataOverrides_ReturnsNullMetadataOverrides()
        {
            // Arrange
            var provider = CreateProvider("generic", true);
            provider.BootstrapMetadataIssuer = null;
            provider.BootstrapMetadataAuthorizationEndpoint = null;
            provider.BootstrapMetadataTokenEndpoint = null;
            provider.BootstrapMetadataUserInfoEndpoint = null;
            provider.BootstrapMetadataEndSessionEndpoint = null;
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Single().Oidc.MetadataOverrides.ShouldBeNull();
        }

        [Theory]
        [InlineData("issuer")]
        [InlineData("authorization")]
        [InlineData("token")]
        [InlineData("userinfo")]
        [InlineData("endsession")]
        public void Handle_WithSingleMetadataOverride_ReturnsMetadataObject(string field)
        {
            // Arrange
            var provider = CreateProvider("generic", true);
            provider.BootstrapMetadataIssuer = null;
            provider.BootstrapMetadataAuthorizationEndpoint = null;
            provider.BootstrapMetadataTokenEndpoint = null;
            provider.BootstrapMetadataUserInfoEndpoint = null;
            provider.BootstrapMetadataEndSessionEndpoint = null;

            if (field == "issuer") provider.BootstrapMetadataIssuer = "https://issuer.only";
            if (field == "authorization") provider.BootstrapMetadataAuthorizationEndpoint = "https://issuer.only/connect/authorize";
            if (field == "token") provider.BootstrapMetadataTokenEndpoint = "https://issuer.only/connect/token";
            if (field == "userinfo") provider.BootstrapMetadataUserInfoEndpoint = "https://issuer.only/connect/userinfo";
            if (field == "endsession") provider.BootstrapMetadataEndSessionEndpoint = "https://issuer.only/connect/endsession";

            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Single().Oidc.MetadataOverrides.ShouldNotBeNull();
        }

        [Fact]
        public void Handle_NeverExposesSigningKey()
        {
            // Arrange
            var provider = CreateProvider("entra", true);
            provider.SigningKey = "super-secret-signing-key";
            var configuration = CreateOptions(provider);

            // Act
            var result = AuthenticationBootstrapQueryHandler.Handle(new AuthenticationBootstrapQuery(), configuration);

            // Assert
            result.BootstrapState.ShouldBe("available");
            result.Providers.Single().ProviderKey.ShouldBe("entra");
            result.ToString().ShouldNotContain("super-secret-signing-key");
        }

        private static IOptions<OidcAuthenticationConfiguration> CreateOptions(params OidcProviderConfiguration[] providers)
        {
            return Options.Create(new OidcAuthenticationConfiguration
            {
                Providers = providers
            });
        }

        private static OidcProviderConfiguration CreateProvider(string key, bool enabled)
        {
            return new OidcProviderConfiguration
            {
                ProviderKey = key,
                DisplayName = key,
                Enabled = enabled,
                Kind = key == "generic" ? OidcProviderKind.GenericOidc : OidcProviderKind.EntraId,
                Authority = $"https://{key}.issuer.local",
                ClientId = $"{key}-client-id",
                ValidateAudience = false,
                BootstrapRedirectUri = "http://localhost:5173/auth/callback",
                BootstrapScope = "openid profile email",
                BootstrapResponseType = "code",
                BootstrapPostLogoutRedirectUri = "http://localhost:5173/auth/logout-callback",
                BootstrapSilentRedirectUri = "http://localhost:5173/auth/silent-callback",
                BootstrapAutomaticSilentRenew = true
            };
        }
    }
}
