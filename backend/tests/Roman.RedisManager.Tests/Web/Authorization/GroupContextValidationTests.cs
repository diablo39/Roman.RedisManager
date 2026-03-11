using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class GroupContextValidationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GroupContextValidationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task DeleteKey_WithoutGroupContext_ReturnsForbidden()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "admin");

            // Act
            var response = await client.DeleteAsync("/api/redis-keys/test-key");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
    }
}
