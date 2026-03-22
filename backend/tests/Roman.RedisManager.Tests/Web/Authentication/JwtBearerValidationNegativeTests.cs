using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authentication
{
    public class JwtBearerValidationNegativeTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public JwtBearerValidationNegativeTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var claims = new Dictionary<string, object>
            {
                ["exp"] = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds()
            };

            var token = TestAuthTokenFactory.CreateBearerToken(
                "https://login.microsoftonline.com/common/v2.0",
                "entra",
                new[] { "admin" },
                claims);
            TestAuthTokenFactory.ApplyBearerToken(client, token);

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithInvalidSignature_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = TestAuthTokenFactory.CreateBearerToken(
                "https://login.microsoftonline.com/common/v2.0",
                "entra",
                new[] { "admin" },
                signingKey: "different-signing-key-for-negative-test-12345");
            TestAuthTokenFactory.ApplyBearerToken(client, token);

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithUnknownIssuer_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = TestAuthTokenFactory.CreateBearerToken(
                "https://unknown-issuer.local",
                "entra",
                new[] { "admin" });
            TestAuthTokenFactory.ApplyBearerToken(client, token);

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithAudienceMismatch_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var claims = new Dictionary<string, object>
            {
                ["aud"] = "different-audience"
            };

            var token = TestAuthTokenFactory.CreateBearerToken(
                "https://login.microsoftonline.com/common/v2.0",
                "entra",
                new[] { "admin" },
                claims);
            TestAuthTokenFactory.ApplyBearerToken(client, token);

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithMissingAuthorizationHeader_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithMalformedBearerToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-valid-jwt-token");

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    }
}
