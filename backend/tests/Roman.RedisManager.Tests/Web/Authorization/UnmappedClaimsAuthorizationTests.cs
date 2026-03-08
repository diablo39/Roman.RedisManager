using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class UnmappedClaimsAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UnmappedClaimsAuthorizationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ProtectedReadEndpoint_WithUnmappedClaims_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "https://issuer.example.com", "generic");

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
    }
}
