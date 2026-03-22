using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class AuthenticationBootstrapControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthenticationBootstrapControllerTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task GetBootstrap_WithoutToken_Returns200AndAvailableBootstrapState()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/authentication/bootstrap");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<AuthenticationBootstrapQueryResult>();
            payload.ShouldNotBeNull();
            payload.BootstrapState.ShouldBe("available");
            payload.Providers.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task GetBootstrap_WhenNoSignInCapableProviders_ReturnsUnavailableState()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Security:Authentication:Providers:0:BootstrapRedirectUri"] = string.Empty,
                        ["Security:Authentication:Providers:0:BootstrapScope"] = string.Empty,
                        ["Security:Authentication:Providers:1:BootstrapRedirectUri"] = string.Empty,
                        ["Security:Authentication:Providers:1:BootstrapScope"] = string.Empty,
                        ["Security:Authentication:Providers:2:BootstrapRedirectUri"] = string.Empty,
                        ["Security:Authentication:Providers:2:BootstrapScope"] = string.Empty
                    });
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/authentication/bootstrap");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<AuthenticationBootstrapQueryResult>();
            payload.ShouldNotBeNull();
            payload.BootstrapState.ShouldBe("unavailable");
            payload.Providers.ShouldBeEmpty();
            payload.UnavailableReasonCodes.ShouldContain("no_sign_in_capable_providers");
        }

        [Fact]
        public async Task GetBootstrap_ResponseBody_DoesNotIncludeSigningKey()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/authentication/bootstrap");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            body.ShouldNotContain("SigningKey");
            body.ShouldNotContain("development-only-signing-key-change-me-1234567890");
        }
    }
}
