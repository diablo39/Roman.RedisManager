using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;

namespace Roman.RedisManager.Tests.Web.Authentication
{
    public class BearerAuthenticationFlowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BearerAuthenticationFlowTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ProtectedEndpoint_WithValidBearerToken_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "redis-reader", "editor", "admin");

            // Act
            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithRolesArrayClaim_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var additionalClaims = new Dictionary<string, object>
            {
                ["roles"] = new[] { "reader", "admin" }
            };

            TestAuthTokenFactory.ApplyBearer(
                client,
                "https://login.microsoftonline.com/common/v2.0",
                "entra",
                additionalClaims);

            // Act
            var response = await client.GetAsync("/api/redis-server-groups?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
