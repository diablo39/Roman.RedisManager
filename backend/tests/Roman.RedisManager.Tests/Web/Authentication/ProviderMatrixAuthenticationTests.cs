using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;

namespace Roman.RedisManager.Tests.Web.Authentication
{
    public class ProviderMatrixAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProviderMatrixAuthenticationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ProtectedEndpoint_WithProviderMatrixTokens_AllowsConfiguredProviders()
        {
            // Arrange
            var matrix = new[]
            {
                new { Issuer = "https://login.microsoftonline.com/common/v2.0", Provider = "entra" },
                new { Issuer = "https://accounts.google.com", Provider = "google" },
                new { Issuer = "https://issuer.example.com", Provider = "generic" }
            };

            // Act
            var statuses = new List<HttpStatusCode>();
            foreach (var item in matrix)
            {
                var client = _factory.CreateClient();
                TestAuthTokenFactory.ApplyBearer(client, item.Issuer, item.Provider, "redis-reader", "editor", "admin");
                var response = await client.GetAsync("/api/redis-server-groups");
                statuses.Add(response.StatusCode);
            }

            // Assert
            statuses.ShouldAllBe(status => status == HttpStatusCode.OK);
        }
    }
}
